using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SearchChooserAPI.Models;

namespace SearchChooserAPI.Helper
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MandatoryColumnAttribute : Attribute
    {
    }

    public static class QueryableProjectionExtensions
    {
        public static IQueryable<TResult> SelectColumns<TResult>(
            this IQueryable<TResult> query,
            IEnumerable<string> columns,
            ColumnMode mode = ColumnMode.Include)
            where TResult : class, new()
        {
            var allProperties = typeof(TResult).GetProperties().Select(p => p.Name).ToList();
            var validProperties = new HashSet<string>(allProperties, StringComparer.OrdinalIgnoreCase);

            var invalidColumns = columns.Where(c => !validProperties.Contains(c)).ToList();
            if (invalidColumns.Count != 0)
            {
                throw new ArgumentException($"Invalid column(s): {string.Join(", ", invalidColumns)}. Valid columns are: {string.Join(", ", allProperties)}");
            }

            var resolved = mode == ColumnMode.Exclude
                ? allProperties.Except(columns, StringComparer.OrdinalIgnoreCase)
                : columns;

            var mandatoryProps = typeof(TResult).GetProperties()
                .Where(p => p.GetCustomAttribute<MandatoryColumnAttribute>() != null)
                .Select(p => p.Name);

            var selectedColumns = new HashSet<string>(resolved, StringComparer.OrdinalIgnoreCase);
            foreach (var mandatory in mandatoryProps)
            {
                selectedColumns.Add(mandatory);
            }

            var parameter = Expression.Parameter(typeof(TResult), "x");
            var bindings = new List<MemberBinding>();
            foreach (var columnName in selectedColumns)
            {
                bindings.Add(Expression.Bind(
                    typeof(TResult).GetProperty(columnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)!,
                    Expression.Property(parameter, columnName)
                ));
            }

            var init = Expression.MemberInit(
                Expression.New(typeof(TResult)),
                bindings
            );

            var lambda = Expression.Lambda<Func<TResult, TResult>>(init, parameter);

            return query.Select(lambda);
        }
    }
}
