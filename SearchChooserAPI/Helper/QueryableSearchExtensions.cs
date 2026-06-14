using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SearchChooserAPI.Helper
{
    public static class QueryableSearchExtensions
    {
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
