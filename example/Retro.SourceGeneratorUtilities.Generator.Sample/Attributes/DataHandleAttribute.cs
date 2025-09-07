namespace Retro.SourceGeneratorUtilities.Generator.Sample.Attributes;

internal abstract class DataHandleBaseAttribute : Attribute
{
  public Type[] ComparableTypes { get; init; } = [];
}

[AttributeUsage(AttributeTargets.Struct)]
internal class DataHandleAttribute(Type provider, string repositoryName) : DataHandleBaseAttribute
{
  public Type Provider { get; } = provider;
  public string RepositoryName { get; } = repositoryName;
}

[AttributeUsage(AttributeTargets.Struct)]
internal class DataHandleAttribute<T> : DataHandleBaseAttribute
    where T : struct;