using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Utilities.Types;
using Retro.SourceGeneratorUtilities.Test.Utils;

namespace Retro.SourceGeneratorUtilities.Test.Types;

public class TypeExtensionsTest {

  [Test]
  public void TestIsSameType() {
    var compilation = GeneratorTestHelpers.CreateCompilation(
        """
        using System;
        using System.Collections.Generic;
        """);

    var typeSymbols = new Dictionary<Type, ITypeSymbol?> {
        // Primitives
        { typeof(void), compilation.GetTypeByMetadataName("System.Void") },
        { typeof(int), compilation.GetTypeByMetadataName("System.Int32") },
        { typeof(string), compilation.GetTypeByMetadataName("System.String") },
        { typeof(object), compilation.GetTypeByMetadataName("System.Object") },

        // Regular class
        { typeof(Attribute), compilation.GetTypeByMetadataName("System.Attribute") },

        // Generic class (open)
        { typeof(List<>), compilation.GetTypeByMetadataName("System.Collections.Generic.List`1") },

        // Generic class (closed)
        {
            typeof(List<int>),
            compilation.GetTypeByMetadataName("System.Collections.Generic.List`1")
                ?.Construct(compilation.GetSpecialType(SpecialType.System_Int32))
        },
        
        // Interface vs concrete type
        { typeof(IEnumerable<>), compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1") },

        // Different generic parameter counts
        { typeof(Func<>), compilation.GetTypeByMetadataName("System.Func`1") },
        { typeof(Func<,>), compilation.GetTypeByMetadataName("System.Func`2") },

        // Array types
        { typeof(int[]), compilation.CreateArrayTypeSymbol(compilation.GetSpecialType(SpecialType.System_Int32)) },
        { typeof(string[]), compilation.CreateArrayTypeSymbol(compilation.GetSpecialType(SpecialType.System_String)) },
        { typeof(int[,]), compilation.CreateArrayTypeSymbol(compilation.GetSpecialType(SpecialType.System_Int32), 2) },

        // Pointer types
        { typeof(void*), compilation.CreatePointerTypeSymbol(compilation.GetTypeByMetadataName("System.Void")!) },
        { typeof(int*), compilation.CreatePointerTypeSymbol(compilation.GetSpecialType(SpecialType.System_Int32)) }
    };
    
    Assert.Multiple(() => {
      foreach (var (type, symbol) in typeSymbols) {
        Assert.That(symbol?.IsSameType(type), Is.True,
                    $"Failed comparing {symbol?.ToDisplayString()} with {type}");
      }

      // Verify non-matching types
      Assert.That(typeSymbols[typeof(int)]?.IsSameType(typeof(string)), Is.False,
                  "int should not match string");

      Assert.That(typeSymbols[typeof(List<>)]?.IsSameType(typeof(List<int>)), Is.False,
                  "Open generic should not match closed generic");

      Assert.That(typeSymbols[typeof(List<int>)]?.IsSameType(typeof(List<string>)), Is.False,
                  "List<int> should not match List<string>");
      
      // Generic interface vs concrete type
      Assert.That(typeSymbols[typeof(IEnumerable<>)]?.IsSameType(typeof(List<>)), Is.False,
                  "IEnumerable<T> should not match List<T>");

      // Different generic parameter counts
      Assert.That(typeSymbols[typeof(Func<>)]?.IsSameType(typeof(Func<,>)), Is.False,
                  "Func<T> should not match Func<T1,T2>");

      // Array type tests
      Assert.That(typeSymbols[typeof(int[])]?.IsSameType(typeof(string[])), Is.False,
                  "int[] should not match string[]");

      Assert.That(typeSymbols[typeof(int[])]?.IsSameType(typeof(int[,])), Is.False,
                  "int[] should not match int[,]");

      Assert.That(typeSymbols[typeof(int[])]?.IsSameType(typeof(int)), Is.False,
                  "int[] should not match int");

      // Pointer type tests
      Assert.That(typeSymbols[typeof(void*)]?.IsSameType(typeof(int*)), Is.False,
                  "void* should not match int*");

      Assert.That(typeSymbols[typeof(int*)]?.IsSameType(typeof(int)), Is.False,
                  "int* should not match int");

      Assert.That(typeSymbols[typeof(int*)]?.IsSameType(typeof(int[])), Is.False,
                  "int* should not match int[]");
    });
  }

  [Test]
  public void TestIsOfType() {
    var compilation = GeneratorTestHelpers.CreateCompilation(
        """
        using System;
        using System.Collections.Generic;
        using Microsoft.CodeAnalysis;
        """);

    var typeSymbols = new Dictionary<Type, INamedTypeSymbol?> {
        // Base types and interfaces
        { typeof(object), compilation.GetTypeByMetadataName("System.Object") },
        { typeof(Attribute), compilation.GetTypeByMetadataName("System.Attribute") },
        { typeof(IEquatable<>), compilation.GetTypeByMetadataName("System.IEquatable`1") },

        // Covariant interfaces
        { typeof(IEnumerable<>), compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1") },
        { typeof(IReadOnlyList<>), compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyList`1") },

        // Contravariant interfaces
        { typeof(IComparer<>), compilation.GetTypeByMetadataName("System.Collections.Generic.IComparer`1") }, {
            typeof(IEqualityComparer<>),
            compilation.GetTypeByMetadataName("System.Collections.Generic.IEqualityComparer`1")
        },

        // Concrete implementations
        { typeof(List<>), compilation.GetTypeByMetadataName("System.Collections.Generic.List`1") }
    };

    // Construct some closed generic types
    var listOfString = typeSymbols[typeof(List<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_String));
    var listOfObject = typeSymbols[typeof(List<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_Object));
    var ienumOfString =
        typeSymbols[typeof(IEnumerable<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_String));
    var ienumOfObject =
        typeSymbols[typeof(IEnumerable<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_Object));
    var comparerOfString =
        typeSymbols[typeof(IComparer<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_String));
    var comparerOfObject =
        typeSymbols[typeof(IComparer<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_Object));

    Assert.Multiple(() => {
      // Basic inheritance
      Assert.That(typeSymbols[typeof(Attribute)]!.IsAssignableFrom<object>(), Is.True,
                  "Attribute should be of type object");
      Assert.That(typeSymbols[typeof(object)]!.IsAssignableFrom<Attribute>(), Is.False,
                  "Object should not be of type Attribute");

      // Generic interface implementation
      Assert.That(listOfString.IsAssignableFrom<IEnumerable<string>>(), Is.True,
                  "List<string> should implement IEnumerable<string>");

      // Covariant interface tests (if T : U then IEnumerable<T> : IEnumerable<U>)
      Assert.That(ienumOfString.IsAssignableFrom<IEnumerable<object>>(), Is.True,
                  "IEnumerable<string> should be convertible to IEnumerable<object>");
      Assert.That(listOfString.IsAssignableFrom<IEnumerable<object>>(), Is.True,
                  "List<string> should be convertible to IEnumerable<object>");
      Assert.That(ienumOfObject.IsAssignableFrom<IEnumerable<string>>(), Is.False,
                  "IEnumerable<object> should not be convertible to IEnumerable<string>");

      // Contravariant interface tests (if T : U then IComparer<U> : IComparer<T>)
      Assert.That(comparerOfObject.IsAssignableFrom<IComparer<string>>(), Is.True,
                  "IComparer<object> should be convertible to IComparer<string>");
      Assert.That(comparerOfString.IsAssignableFrom<IComparer<object>>(), Is.False,
                  "IComparer<string> should not be convertible to IComparer<object>");

      // Generic type identity
      Assert.That(listOfString.IsAssignableFrom<List<object>>(), Is.False,
                  "List<string> should not be convertible to List<object>");
      Assert.That(listOfObject.IsAssignableFrom<List<string>>(), Is.False,
                  "List<object> should not be convertible to List<string>");
    });
  }

  [Test]
  public void TestIsOfTypeReverse() {
    var compilation = GeneratorTestHelpers.CreateCompilation(
        """
        using System;
        using System.Collections.Generic;
        using Microsoft.CodeAnalysis;
        """);

    var typeSymbols = new Dictionary<Type, INamedTypeSymbol?> {
        // Base types and interfaces
        { typeof(object), compilation.GetTypeByMetadataName("System.Object") },
        { typeof(Attribute), compilation.GetTypeByMetadataName("System.Attribute") },
        { typeof(IEquatable<>), compilation.GetTypeByMetadataName("System.IEquatable`1") },

        // Covariant interfaces
        { typeof(IEnumerable<>), compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1") },
        { typeof(IReadOnlyList<>), compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyList`1") },

        // Contravariant interfaces
        { typeof(IComparer<>), compilation.GetTypeByMetadataName("System.Collections.Generic.IComparer`1") }, {
            typeof(IEqualityComparer<>),
            compilation.GetTypeByMetadataName("System.Collections.Generic.IEqualityComparer`1")
        },

        // Concrete implementations
        { typeof(List<>), compilation.GetTypeByMetadataName("System.Collections.Generic.List`1") }
    };

    // Construct some closed generic types
    var listOfString = typeSymbols[typeof(List<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_String));
    var listOfObject = typeSymbols[typeof(List<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_Object));
    var ienumOfString =
        typeSymbols[typeof(IEnumerable<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_String));
    var ienumOfObject =
        typeSymbols[typeof(IEnumerable<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_Object));
    var comparerOfString =
        typeSymbols[typeof(IComparer<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_String));
    var comparerOfObject =
        typeSymbols[typeof(IComparer<>)]!.Construct(compilation.GetSpecialType(SpecialType.System_Object));

    Assert.Multiple(() => {
      // Basic inheritance
      Assert.That(typeSymbols[typeof(Attribute)]!.IsAssignableTo<object>(), Is.False,
                  "Object should not be of type Attribute");
      Assert.That(typeSymbols[typeof(object)]!.IsAssignableTo<Attribute>(), Is.True,
                  "Attribute should be of type object");

      // Generic interface implementation
      Assert.That(ienumOfString.IsAssignableTo<List<string>>(), Is.True,
                  "List<string> should implement IEnumerable<string>");

      // Covariant interface tests (if T : U then IEnumerable<T> : IEnumerable<U>)
      Assert.That(ienumOfObject.IsAssignableTo<IEnumerable<string>>(), Is.True,
                  "IEnumerable<string> should be convertible to IEnumerable<object>");
      Assert.That(ienumOfObject.IsAssignableTo<List<string>>(), Is.True,
                  "List<string> should be convertible to IEnumerable<object>");
      Assert.That(ienumOfString.IsAssignableTo<IEnumerable<object>>(), Is.False,
                  "IEnumerable<object> should not be convertible to IEnumerable<string>");

      // Contravariant interface tests (if T : U then IComparer<U> : IComparer<T>)
      Assert.That(comparerOfString.IsAssignableTo<IComparer<object>>(), Is.True,
                  "IComparer<object> should be convertible to IComparer<string>");
      Assert.That(comparerOfObject.IsAssignableTo<IComparer<string>>(), Is.False,
                  "IComparer<string> should not be convertible to IComparer<object>");

      // Generic type identity
      Assert.That(listOfObject.IsAssignableTo<List<string>>(), Is.False,
                  "List<string> should not be convertible to List<object>");
      Assert.That(listOfString.IsAssignableTo<List<object>>(), Is.False,
                  "List<object> should not be convertible to List<string>");
    });
  }
}