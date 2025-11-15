using System.Collections.Immutable;
using Retro.SourceGeneratorUtilities.Test.Utils;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;

namespace Retro.SourceGeneratorUtilities.Results.Test.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class PropertyOnlyAttribute : Attribute {
  
  public int Property { get; init; } = 1;
  
}

[AttributeInfoType<PropertyOnlyAttribute>]
public record PropertyOnlyAttributeInfo {
  public required int Property { get; init; } = 1;
}

[AttributeUsage(AttributeTargets.Class)]
public class MultipleAllowedAttribute(string? constructorProperty = null) : Attribute {
  
  public string? ConstructorProperty { get; } = constructorProperty;

  public int Property { get; init; } = 1;

}

[AttributeInfoType<MultipleAllowedAttribute>]
public record MultipleAllowedAttributeInfo(string? ConstructorProperty) {
  
  public required int Property { get; init; } = 1;
  
}

public class AttributeInfoTest {

  [Test]
  public void TestValidateAttributeModel() {
    const string classDeclaration = """
                                    using Retro.SourceGeneratorUtilities.Results.Test.Attributes;
                                    
                                    namespace TestNamespace;

                                    [PropertyOnly(Property = 2)]
                                    public class TestClass;
                                    
                                    [PropertyOnly(Property = 5)]
                                    public struct TestStruct;
                                    """;

    var compilation = GeneratorTestHelpers.CreateCompilation(classDeclaration);
    var compiledClass = compilation.GetTypeByMetadataName("TestNamespace.TestClass");
    var compiledStruct = compilation.GetTypeByMetadataName("TestNamespace.TestStruct");
    Assert.Multiple(() => {
      Assert.That(compiledClass, Is.Not.Null);
      Assert.That(compiledStruct, Is.Not.Null);
    });

    var classAttributes = compiledClass.GetAttributes()
        .Where(x => x.AttributeClass?.Name == nameof(PropertyOnlyAttribute))
        .ToImmutableList();
    Assert.That(classAttributes, Has.Count.EqualTo(1));

    var classInfo = compiledClass.GetPropertyOnlyAttributeInfo();
    Assert.That(classInfo.Property, Is.EqualTo(2));
    
    var structAttributes = compiledStruct.GetAttributes()
        .Where(x => x.AttributeClass?.Name == nameof(PropertyOnlyAttribute))
        .ToImmutableList();
    Assert.That(structAttributes, Has.Count.EqualTo(1));

    var structInfo = compiledStruct.GetPropertyOnlyAttributeInfo();
    Assert.That(structInfo.Property, Is.EqualTo(5));
  }
  
  [Test]
  public void TestValidateAttributeModelCollection() {
    const string classDeclaration = """
                                    using Retro.SourceGeneratorUtilities.Results.Test.Attributes;

                                    namespace TestNamespace;

                                    [MultipleAllowedAttribute(Property = 2)]
                                    [MultipleAllowedAttribute("Name", Property = 6)]
                                    public class TestClass;
                                    """;

    var compilation = GeneratorTestHelpers.CreateCompilation(classDeclaration);
    var compiledClass = compilation.GetTypeByMetadataName("TestNamespace.TestClass");
    Assert.That(compiledClass, Is.Not.Null);

    var attributes = compiledClass.GetAttributes()
        .GetMultipleAllowedAttributeInfos()
        .ToImmutableList();
    Assert.That(attributes, Has.Count.EqualTo(2));
    Assert.Multiple(() => {
      Assert.That(attributes[0].ConstructorProperty, Is.Null);
      Assert.That(attributes[0].Property, Is.EqualTo(2));
      
      Assert.That(attributes[1].ConstructorProperty, Is.EqualTo("Name"));
      Assert.That(attributes[1].Property, Is.EqualTo(6));
    });
  }
  
}