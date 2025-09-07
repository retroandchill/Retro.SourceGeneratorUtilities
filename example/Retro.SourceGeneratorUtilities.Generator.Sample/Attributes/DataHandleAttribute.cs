namespace Retro.SourceGeneratorUtilities.Generator.Sample.Attributes;

[AttributeUsage(AttributeTargets.Struct)]
internal class DataHandleAttribute<T> : Attribute where T : struct
{
  public DataHandleAttribute()
  {
        
  }
    
  public DataHandleAttribute(Type provider, string repositoryName)
  {
    Provider = provider;
    RepositoryName = repositoryName;
  }

  public Type? Provider { get; }
  public string? RepositoryName { get; }
}