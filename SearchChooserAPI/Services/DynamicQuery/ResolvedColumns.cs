using System.Reflection;

namespace Meccano.DynamicQuery;

public sealed class ResolvedColumns<TResponse> where TResponse : class
{
    public IReadOnlyList<PropertyInfo> Properties { get; }

    public ResolvedColumns(IReadOnlyList<PropertyInfo> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);

        if (properties.Count == 0)
            throw new ArgumentException("At least one column must be resolved.");

        Properties = properties;
    }
}
