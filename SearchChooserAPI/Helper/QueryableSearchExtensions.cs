using SearchChooserAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SearchChooserAPI.Helper
{
    public static class QueryableSearchExtensions
    {

        public static IQueryable<TResult> DynamicSearchAndFilter<TResult>(
           this IQueryable<TResult> query,
           string? term,
           IEnumerable<FilterCriteria>? filters)
           where TResult : class, new()
        {
            bool hasSearch = !string.IsNullOrWhiteSpace(term);
            bool hasFilters = filters != null && filters.Any();

            if (!hasSearch && !hasFilters)
                return query;

            var param = Expression.Parameter(typeof(TResult), "x");
            Expression? finalExpression = null;

            if (hasSearch)
            {
                //var props = typeof(TResult).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                var selectedProperties = GetSelectedProperties<TResult>(query.Expression);


                // Get properties to search matching the selected/type properties
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

                finalExpression = searchCombined ?? Expression.Constant(false);
            }

            if (hasFilters)
            {
                Expression? filterCombined = null;

                foreach (var filter in filters!)
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

                if (filterCombined != null)
                {
                    finalExpression = finalExpression == null
                        ? filterCombined
                        : Expression.AndAlso(finalExpression, filterCombined);
                }
            }

            if (finalExpression == null) return query;

            var lambda = Expression.Lambda<Func<TResult, bool>>(finalExpression, param);
            return query.Where(lambda);
        }

        public static IQueryable<T> SearchColumns<T>(
            this IQueryable<T> query,
            string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return query;

            var selectedProperties = GetSelectedProperties<T>(query.Expression);

            var parameter = Expression.Parameter(typeof(T), "x");
            var searchTerms = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Get properties to search matching the selected/type properties
            var searchProperties = typeof(T).GetProperties()
                .Where(p => selectedProperties.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (!searchProperties.Any()) return query;

            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
            var toStringMethod = typeof(object).GetMethod("ToString", Type.EmptyTypes)!;

            Expression? finalAndExpression = null;

            foreach (var term in searchTerms)
            {
                Expression? orExpression = null;
                var termLowerConstant = Expression.Constant(term.ToLower());

                foreach (var prop in searchProperties)
                {
                    Expression propAccess = Expression.Property(parameter, prop);
                    Expression stringExpression;

                    // Handle non-string types by converting them to string
                    if (prop.PropertyType == typeof(string))
                    {
                        stringExpression = Expression.Coalesce(propAccess, Expression.Constant(string.Empty));
                    }
                    else
                    {
                        var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);
                        bool isNullableValueType = underlyingType != null;
                        bool isReferenceType = !prop.PropertyType.IsValueType;

                        if (isNullableValueType)
                        {
                            // Nullable value type: build conditional x.Prop == null ? "" : x.Prop.Value.ToString()
                            var isNull = Expression.Equal(propAccess, Expression.Constant(null, prop.PropertyType));
                            var valueAccess = Expression.Property(propAccess, "Value");
                            var propToStringMethod = underlyingType!.GetMethod("ToString", Type.EmptyTypes) ?? toStringMethod;
                            var toStringCall = Expression.Call(valueAccess, propToStringMethod);
                            stringExpression = Expression.Condition(isNull, Expression.Constant(string.Empty), toStringCall);
                        }
                        else if (isReferenceType)
                        {
                            // Reference type other than string: x.Prop == null ? "" : x.Prop.ToString()
                            var isNull = Expression.Equal(propAccess, Expression.Constant(null, prop.PropertyType));
                            var propToStringMethod = prop.PropertyType.GetMethod("ToString", Type.EmptyTypes) ?? toStringMethod;
                            var toStringCall = Expression.Call(propAccess, propToStringMethod);
                            stringExpression = Expression.Condition(isNull, Expression.Constant(string.Empty), toStringCall);
                        }
                        else
                        {
                            // Non-nullable value type: x.Prop.ToString()
                            var propToStringMethod = prop.PropertyType.GetMethod("ToString", Type.EmptyTypes) ?? toStringMethod;
                            stringExpression = Expression.Call(propAccess, propToStringMethod);
                        }
                    }

                    // Lowercase the string conversion: stringExpression.ToLower()
                    var toLowerCall = Expression.Call(stringExpression, toLowerMethod);

                    // Search check: stringExpression.ToLower().Contains(term)
                    var containsCall = Expression.Call(toLowerCall, containsMethod, termLowerConstant);

                    if (orExpression == null)
                    {
                        orExpression = containsCall;
                    }
                    else
                    {
                        orExpression = Expression.OrElse(orExpression, containsCall);
                    }
                }

                if (orExpression != null)
                {
                    if (finalAndExpression == null)
                    {
                        finalAndExpression = orExpression;
                    }
                    else
                    {
                        finalAndExpression = Expression.AndAlso(finalAndExpression, orExpression);
                    }
                }
            }

            if (finalAndExpression == null) return query;

            var lambda = Expression.Lambda<Func<T, bool>>(finalAndExpression, parameter);
            return query.Where(lambda);
        }

        private static List<string> GetSelectedProperties<T>(Expression expression)
        {
            var selectedProperties = new List<string>();

            // Find the outermost Select method call in the expression tree
            var selectCall = FindSelectCall(expression);
            if (selectCall != null && selectCall.Arguments.Count == 2)
            {
                // The second argument is the selector lambda
                var selectorLambda = UnwrapLambda(selectCall.Arguments[1]);
                if (selectorLambda != null && selectorLambda.Body is MemberInitExpression memberInit)
                {
                    foreach (var binding in memberInit.Bindings)
                    {
                        selectedProperties.Add(binding.Member.Name);
                    }
                }
            }

            // Fallback to all readable properties of T if no select call was found in the expression tree
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
                // Recurse into the source argument (usually the first argument)
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
}
