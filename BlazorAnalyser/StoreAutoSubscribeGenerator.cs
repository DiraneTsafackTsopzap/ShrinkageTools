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
    [SuppressMessage("ReSharper", "RawStringCanBeSimplified")]
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1️⃣ Génération de l'attribut AutoSubscribe
        context.RegisterPostInitializationOutput(static postInitializationContext =>
        {
            postInitializationContext.AddSource(
                "BlazorLayout.StateManagement.AutoSubscribeAttribute.g.cs",
                """
                namespace BlazorLayout.StateManagement;

                /// <summary>
                /// Put this on StoreBase-derived store classes and its partial properties
                /// to generate their accessors and auto-subscription logic.
                /// </summary>
                [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Class)]
                public sealed class AutoSubscribeAttribute : System.Attribute
                {
                }
                """);
        });

        // 2️⃣ Pipeline : classes annotées avec [AutoSubscribe]
        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "BlazorLayout.StateManagement.AutoSubscribeAttribute",
            predicate: static (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax,
            transform: static (context, _) =>
            {
                var classSyntax = (ClassDeclarationSyntax)context.TargetNode;

                if (context.SemanticModel.GetDeclaredSymbol(classSyntax) is not INamedTypeSymbol classSymbol)
                    return null;

                var properties = new List<PropertyModel>();

                foreach (var member in classSyntax.Members)
                {
                    if (member is not PropertyDeclarationSyntax propertySyntax)
                        continue;

                    if (context.SemanticModel.GetDeclaredSymbol(propertySyntax) is not IPropertySymbol propertySymbol)
                        continue;

                    if (!propertySymbol.GetAttributes().Any(a =>
                        a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                        == "global::BlazorLayout.StateManagement.AutoSubscribeAttribute"))
                        continue;

                    if (!propertySymbol.IsPartialDefinition)
                        continue;

                    properties.Add(new PropertyModel(
                        Type: propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        Name: propertySyntax.Identifier.Text,
                        PropertyAccessors: propertySymbol.DeclaredAccessibility,
                        GetAccessors: propertySymbol.GetMethod?.DeclaredAccessibility ?? Accessibility.NotApplicable,
                        SetAccessors: propertySymbol.SetMethod?.DeclaredAccessibility
                    ));
                }

                return new ClassModel(
                    Namespace: classSymbol.ContainingNamespace.ToDisplayString(),
                    Name: classSymbol.Name,
                    TypeParams: classSymbol.TypeParameters,
                    Properties: properties
                );
            }
        ).Where(m => m is not null);

        // 3️⃣ Génération du code des propriétés
        context.RegisterSourceOutput(pipeline, static (context, model) =>
        {
            var sb = new StringBuilder();

            sb.AppendLine($"namespace {model!.Namespace};");
            sb.AppendLine();
            sb.AppendLine($"public partial class {model.Name}");
            sb.AppendLine("{");

            foreach (var prop in model.Properties)
            {
                sb.AppendLine($"    private {prop.Type} __{prop.Name};");
                sb.AppendLine();
                sb.AppendLine($"    public partial {prop.Type} {prop.Name}");
                sb.AppendLine("    {");
                sb.AppendLine("        get");
                sb.AppendLine("        {");
                sb.AppendLine("            SubscribeCaller();");
                sb.AppendLine($"            return __{prop.Name};");
                sb.AppendLine("        }");

                if (prop.SetAccessors is not null)
                {
                    sb.AppendLine("        private set");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            __{prop.Name} = value;");
                    sb.AppendLine("            NotifySubscribers();");
                    sb.AppendLine("        }");
                }

                sb.AppendLine("    }");
                sb.AppendLine();
            }

            sb.AppendLine("}");

            context.AddSource(
                $"{model.Namespace}.{model.Name}.g.cs",
                SourceText.From(sb.ToString(), Encoding.UTF8));
        });
    }

    private record ClassModel(
        string Namespace,
        string Name,
        ImmutableArray<ITypeParameterSymbol> TypeParams,
        IReadOnlyList<PropertyModel> Properties
    );

    private record PropertyModel(
        string Type,
        string Name,
        Accessibility PropertyAccessors,
        Accessibility GetAccessors,
        Accessibility? SetAccessors
    );
}
