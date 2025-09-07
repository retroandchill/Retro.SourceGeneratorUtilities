using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Generator.Sample.Attributes;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;

namespace Retro.SourceGeneratorUtilities.Generator.Sample.Model;

[AttributeInfoType<DataHandleBaseAttribute>]
public record DataHandleBaseInfo
{
  public ITypeSymbol[] ComparableTypes { get; init; }
}

[AttributeInfoType<DataHandleAttribute>]
public record DataHandleInfo(ITypeSymbol Type, string RepositoryName) : DataHandleBaseInfo;

[AttributeInfoType(typeof(DataHandleAttribute<>))]
public record DataHandleInfoOneParam(ITypeSymbol EntryType) : DataHandleBaseInfo;