using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Generator.Sample.Attributes;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;

namespace Retro.SourceGeneratorUtilities.Generator.Sample.Model;

[AttributeInfoType(typeof(DataHandleAttribute<>))]
public readonly record struct DataHandleAttributeInfo(ITypeSymbol EntryType, ITypeSymbol? Type, string? RepositoryName)
{
  // ReSharper disable once IntroduceOptionalParameters.Global
  public DataHandleAttributeInfo(ITypeSymbol entryType) : this(entryType, null, null) {  }
    
  [MemberNotNullWhen(true, nameof(Type), nameof(RepositoryName))]
  public bool IsValid => Type is not null && RepositoryName is not null;
}