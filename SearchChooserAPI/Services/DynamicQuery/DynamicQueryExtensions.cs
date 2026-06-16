using System.Linq.Expressions;
using System.Reflection;

namespace Meccano.DynamicQuery;

public static class DynamicQueryExtensions
{
    public static IQueryable<TResult> SelectColumns<TResult>(this IQueryable<TResult> query,IDynamicQueryRequest<TResult> request)
        where TResult : class, new()
    {
        var resolved = ColumnResolver.Resolve<TResult>(request.Columns, request.Mode);

        var parameter = Expression.Parameter(typeof(TResult), "x");
        var bindings = resolved.Properties
            .Select(prop => Expression.Bind(prop, Expression.Property(parameter, prop)))
            .ToList();

        var body = Expression.MemberInit(Expression.New(typeof(TResult)), bindings);
        var lambda = Expression.Lambda<Func<TResult, TResult>>(body, parameter);

        return query.Select(lambda);
    }

    public static IQueryable<TResult> DynamicSearch<TResult>(this IQueryable<TResult> query, string? term)
        where TResult : class, new()
    {
        if (string.IsNullOrWhiteSpace(term))
            return query;

        var param = Expression.Parameter(typeof(TResult), "x");
        var selectedProperties = GetSelectedProperties<TResult>(query.Expression);

        var props = typeof(TResult).GetProperties()
            .Where(p => selectedProperties.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();

        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
        var termExpr = Expression.Constant(term!.ToLower());

        Expression? searchCombined = null;

        foreach (var prop in props)
        {
            Expression propExpr = Expression.Property(param, prop);
            Expression? currentPropCondition = null;

            if (prop.PropertyType == typeof(string))
            {
                var lower = Expression.Call(propExpr, toLowerMethod);
                currentPropCondition = Expression.Call(lower, containsMethod, termExpr);
            }
            else
            {
                try
                {
                    Type targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    object? convertedValue = null;

                    if (targetType == typeof(Guid))
                    {
                        if (Guid.TryParse(term, out var guidVal)) convertedValue = guidVal;
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(term, targetType);
                    }

                    if (convertedValue != null)
                    {
                        Expression valueExpr = Expression.Constant(convertedValue, prop.PropertyType);
                        currentPropCondition = Expression.Equal(propExpr, valueExpr);
                    }
                }
                catch { continue; }
            }

            if (currentPropCondition != null)
            {
                searchCombined = searchCombined == null
                    ? currentPropCondition
                    : Expression.OrElse(searchCombined, currentPropCondition);
            }
        }

        var predicate = searchCombined ?? Expression.Constant(false);
        var lambda = Expression.Lambda<Func<TResult, bool>>(predicate, param);
        return query.Where(lambda);
    }

    public static IQueryable<TResult> DynamicFilter<TResult>(
        this IQueryable<TResult> query, IEnumerable<FilterCriteria>? filters)
        where TResult : class, new()
    {
        if (filters == null || !filters.Any())
            return query;

        var param = Expression.Parameter(typeof(TResult), "x");
        Expression? filterCombined = null;

        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.ColumnName)) continue;

            var prop = typeof(TResult).GetProperty(filter.ColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (prop == null) continue;

            try
            {
                Expression propExpr = Expression.Property(param, prop);
                Type targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                object? val1 = null;
                if (targetType == typeof(Guid))
                {
                    if (Guid.TryParse(filter.Value, out var g))
                        val1 = g;
                }
                else
                {
                    val1 = Convert.ChangeType(filter.Value, targetType);
                }

                if (val1 == null) continue;

                Expression val1Expr = Expression.Constant(val1, prop.PropertyType);
                Expression? condition = null;

                switch (filter.Operator)
                {
                    case FilterOperator.Eq: condition = Expression.Equal(propExpr, val1Expr); break;
                    case FilterOperator.Neq: condition = Expression.NotEqual(propExpr, val1Expr); break;
                    case FilterOperator.Gt: condition = Expression.GreaterThan(propExpr, val1Expr); break;
                    case FilterOperator.Lt: condition = Expression.LessThan(propExpr, val1Expr); break;
                    case FilterOperator.Range:
                        if (!string.IsNullOrEmpty(filter.Value2))
                        {
                            object? val2 = null;
                            if (targetType == typeof(Guid)) { if (Guid.TryParse(filter.Value2, out var g)) val2 = g; }
                            else { val2 = Convert.ChangeType(filter.Value2, targetType); }

                            if (val2 != null)
                            {
                                Expression val2Expr = Expression.Constant(val2, prop.PropertyType);
                                var gte = Expression.GreaterThanOrEqual(propExpr, val1Expr);
                                var lte = Expression.LessThanOrEqual(propExpr, val2Expr);
                                condition = Expression.AndAlso(gte, lte);
                            }
                        }
                        break;
                }

                if (condition != null)
                {
                    filterCombined = filterCombined == null ? condition : Expression.AndAlso(filterCombined, condition);
                }
            }
            catch { continue; }
        }

        if (filterCombined == null) return query;

        var lambda = Expression.Lambda<Func<TResult, bool>>(filterCombined, param);
        return query.Where(lambda);
    }

    public static IQueryable<TResult> DynamicSearchAndFilter<TResult>(
        this IQueryable<TResult> query, string? term, IEnumerable<FilterCriteria>? filters)
        where TResult : class, new()
    {
        return query.DynamicSearch(term).DynamicFilter(filters);
    }

    public static IQueryable<TResult> ApplyDynamicQuery<TResult>(
        this IQueryable<TResult> query, IDynamicQueryRequest<TResult> request)
        where TResult : class, new()
    {
        return query
            .SelectColumns(request)
            .DynamicSearchAndFilter(request.Search, request.Filters);
    }

    private static List<string> GetSelectedProperties<T>(Expression expression)
    {
        var selectedProperties = new List<string>();

        var selectCall = FindSelectCall(expression);
        if (selectCall != null && selectCall.Arguments.Count == 2)
        {
            var selectorLambda = UnwrapLambda(selectCall.Arguments[1]);
            if (selectorLambda != null && selectorLambda.Body is MemberInitExpression memberInit)
            {
                foreach (var binding in memberInit.Bindings)
                {
                    selectedProperties.Add(binding.Member.Name);
                }
            }
        }

        if (!selectedProperties.Any())
        {
            selectedProperties = typeof(T).GetProperties()
                .Where(p => p.CanRead)
                .Select(p => p.Name)
                .ToList();
        }

        return selectedProperties;
    }

    private static MethodCallExpression? FindSelectCall(Expression expression)
    {
        if (expression is MethodCallExpression call)
        {
            if (call.Method.Name == "Select" && call.Method.DeclaringType == typeof(Queryable))
            {
                return call;
            }
            if (call.Arguments.Count > 0)
            {
                return FindSelectCall(call.Arguments[0]);
            }
        }
        return null;
    }

    private static LambdaExpression? UnwrapLambda(Expression expression)
    {
        if (expression is UnaryExpression unary && unary.NodeType == ExpressionType.Quote)
        {
            return unary.Operand as LambdaExpression;
        }
        return expression as LambdaExpression;
    }
}
