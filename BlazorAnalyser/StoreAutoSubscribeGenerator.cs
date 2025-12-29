using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BlazorAnalyzer;

[Generator]
public class StoreAutoSubscribeGenerator : IIncrementalGenerator
{
    // https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md
    // https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md#augment-user-code
    // Another good resource: https://andrewlock.net/creating-a-source-generator-part-1-creating-an-incremental-source-generator/

    [SuppressMessage("ReSharper", "RawStringCanBeSimplified")]
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static postInitializationContext =>
        {
            postInitializationContext.AddSource(
                "BlazorLayout.StateManagement.AutoSubscribeAttribute.g.cs",
                """
                namespace BlazorLayout.StateManagement;

                /// <summary>
                /// Put this on <see cref="StoreBase"/>-derived store classes and its partial properties to generate their accessors.
                /// </summary>
                [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
                internal sealed class AutoSubscribeAttribute : global::System.Attribute;

                """);
        });

        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "BlazorLayout.StateManagement.AutoSubscribeAttribute",
            predicate: static (syntaxNode, cancellationToken) => syntaxNode is ClassDeclarationSyntax,
            transform: static (context, cancellationToken) =>
            {
                var classDeclarationSyntax = (ClassDeclarationSyntax)context.TargetNode;
                if (context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
                    return null;

                var properties = new List<PropertyModel>();
                foreach (var member in classDeclarationSyntax.Members)
                {
                    if (member is not PropertyDeclarationSyntax propertyDeclarationSyntax)
                        continue;

                    if (context.SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax) is not IPropertySymbol propertySymbol)
                        continue;

                    var ok = false;
                    foreach (var attributeData in propertySymbol.GetAttributes())
                    {
                        if (attributeData.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                            == "global::BlazorLayout.StateManagement.AutoSubscribeAttribute")
                        {
                            ok = true;
                            break;
                        }
                    }

                    if (!ok) continue;

                    if (!propertySymbol.IsPartialDefinition)
                        continue;

                    properties.Add(new(
                        Type: propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        Name: propertyDeclarationSyntax.Identifier.Text,
                        PropertyAccessors: propertySymbol.DeclaredAccessibility,
                        GetAccessors: propertySymbol.GetMethod?.DeclaredAccessibility ?? Accessibility.NotApplicable,
                        SetAccessors: propertySymbol.SetMethod?.DeclaredAccessibility
                    ));
                }

                return new ClassModel(
                    Namespace: classSymbol.ContainingNamespace!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    Name: classSymbol.Name,
                    TypeParams: classSymbol.TypeParameters,
                    Properties: properties
                );
            }
        ).Where(model => model is not null);

        context.RegisterSourceOutput(pipeline, static (context, model) =>
        {
            var cleanNamespace = model!.Namespace.StartsWith("global::")
                ? model.Namespace.Substring("global::".Length)
                : model.Namespace;

            var sb = new StringBuilder();

            sb.Append($$"""
                        namespace {{cleanNamespace}};

                        public partial class {{model.Name}}
                        """);

            var typeParamsCount = model.TypeParams.Length;
            if (typeParamsCount is not 0)
            {
                sb.Append("<");
                for (var i = 0; i < typeParamsCount; i++)
                {
                    if (i is not 0) sb.Append(", ");
                    sb.Append(model.TypeParams[i].Name);
                }

                sb.Append(">\n");
            }

            sb.Append("""
                      {

                      """);

            foreach (var property in model.Properties)
            {
                var initializer = ReadOnlyCollectionInitializer(property.Type);
                var propertyAccessors = property.PropertyAccessors.Canonical();
                var getAccessors = property.PropertyAccessors == property.GetAccessors
                    ? null
                    : property.GetAccessors.Canonical();
                var setAccessors = property.PropertyAccessors == property.SetAccessors
                    ? null
                    : property.SetAccessors?.Canonical();

                var hasSetter = property.SetAccessors is not null;

                sb.Append($$"""
                                private {{property.Type}} __{{property.Name}}{{initializer}};
                                {{propertyAccessors}}partial {{property.Type}} {{property.Name}}
                                {
                                    {{getAccessors}}get
                                    {
                                        SubscribeCaller();
                                        return __{{property.Name}};
                                    }

                            """);

                if (hasSetter)
                {
                    sb.Append($$"""
                                        {{setAccessors}}set
                                        {
                                            __{{property.Name}} = value;
                                            NotifySubscribers();
                                        }

                                """);
                }

                sb.Append("""
                              }


                          """);
            }

            sb.Append("""
                      }

                      """);

            context.AddSource(
                $"{cleanNamespace}.{model.Name}.g.cs",
                SourceText.From(sb.ToString(), Encoding.UTF8));
        });
    }

    private record ClassModel(
        string Namespace,
        string Name,
        ImmutableArray<ITypeParameterSymbol> TypeParams,
        IReadOnlyList<PropertyModel> Properties);

    private record PropertyModel(
        string Type,
        string Name,
        Accessibility PropertyAccessors,
        Accessibility GetAccessors,
        Accessibility? SetAccessors);

    private static string? ReadOnlyCollectionInitializer(string fullyQualifiedTypeName)
    {
        if (fullyQualifiedTypeName.IndexOf('<') is not -1 and var indexOfTypeParameterList)
        {
            if (fullyQualifiedTypeName.StartsWith("global::System.Collections.Generic.IReadOnlyList<"))
                return " = []";

            if (fullyQualifiedTypeName.StartsWith("global::System.Collections.Generic.IReadOnlyDictionary<"))
                return $" =\n        new global::System.Collections.Generic.Dictionary{fullyQualifiedTypeName.Substring(indexOfTypeParameterList)}()";

            if (fullyQualifiedTypeName.StartsWith("global::System.Collections.Generic.IReadOnlySet<"))
                return $" =\n        new global::System.Collections.Generic.HashSet{fullyQualifiedTypeName.Substring(indexOfTypeParameterList)}()";
        }

        return null;
    }
}

internal static class Extensions
{
    internal static string Canonical(this Accessibility accessors) => accessors switch
    {
        Accessibility.NotApplicable => "",
        Accessibility.Private => "private ",
        Accessibility.ProtectedAndInternal => "private protected ",
        Accessibility.Protected => "protected ",
        Accessibility.Internal => "internal ",
        Accessibility.ProtectedOrInternal => "protected internal ",
        Accessibility.Public => "public ",
        _ => throw new ArgumentOutOfRangeException(nameof(accessors), accessors, null)
    };
}
