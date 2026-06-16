using System.Reflection;

namespace Meccano.DynamicQuery;

public static class ColumnResolver
{
    public static ResolvedColumns<TResponse> Resolve<TResponse>(List<string>? columns, ColumnMode mode)
        where TResponse : class
    {
        var allProperties = typeof(TResponse).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var allNames = allProperties.Select(p => p.Name).ToList();
        var validNames = new HashSet<string>(allNames, StringComparer.OrdinalIgnoreCase);

        var inputColumns = columns?.Select(c => c.Trim()).ToList() ?? allNames;

        var invalid = inputColumns.Where(c => !validNames.Contains(c)).ToList();
        if (invalid.Count != 0)
            throw new ArgumentException(
                $"Invalid column(s): {string.Join(", ", invalid)}." +
                $"Valid columns are: {string.Join(", ", allNames)}");

        var resolved = mode == ColumnMode.Exclude
            ? allNames.Except(inputColumns, StringComparer.OrdinalIgnoreCase)
            : inputColumns;

        var mandatory = allProperties
            .Where(p => p.GetCustomAttribute<MandatoryColumnAttribute>() != null)
            .Select(p => p.Name);

        var selectedSet = new HashSet<string>(resolved, StringComparer.OrdinalIgnoreCase);
        foreach (var m in mandatory)
            selectedSet.Add(m);

        var props = selectedSet
            .Select(name => allProperties.First(p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        return new ResolvedColumns<TResponse>(props);
    }
}
