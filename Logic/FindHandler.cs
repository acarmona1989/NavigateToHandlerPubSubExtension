using Microsoft.Build.Framework.XamlTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace NavigateToHandlerPubSub.Logic
{
    public class FindHandler
    {
        public async Task<List<IdentifiedHandler>> FindHandlersAsync(Microsoft.CodeAnalysis.Solution solution, Document workingDocument, int linePosition, string projectName)
        {
            var syntaxRoot = await workingDocument.GetSyntaxRootAsync();
            var syntaxNode = syntaxRoot?.FindNode(new TextSpan(linePosition, 0), true, true);

            // Get type information

            var semanticModel = await workingDocument.GetSemanticModelAsync();
            await VS.StatusBar.ShowProgressAsync("Get document model", 2, 3);

            var symbol = GetTypeInfo(semanticModel, syntaxNode);

            var handlers = await GetEventHandlerReferencesAsync(symbol, solution, projectName);
            await VS.StatusBar.ShowProgressAsync("Getting references", 3, 3);

            return handlers;
        }

        private ITypeSymbol GetTypeInfo(SemanticModel semanticModel, SyntaxNode syntaxNode)
        {
            if (syntaxNode is VariableDeclaratorSyntax variableDeclarator)
            {
                return semanticModel.GetTypeInfo(((VariableDeclarationSyntax)variableDeclarator.Parent).Type).Type;
            }
            if (syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax)
            {
                return semanticModel.GetDeclaredSymbol(typeDeclarationSyntax);
            }
            if (syntaxNode is IdentifierNameSyntax identifierName)
            {
                var accessSymbol = semanticModel.GetSymbolInfo(identifierName.Parent);
                IMethodSymbol methodSymbol = (IMethodSymbol)accessSymbol.Symbol;
                if (methodSymbol == null) return null;

                var returnType = (INamedTypeSymbol)methodSymbol.ReceiverType;
                return returnType;
            }

            return semanticModel.GetTypeInfo(syntaxNode).Type;
        }

        private async Task<List<IdentifiedHandler>> GetEventHandlerReferencesAsync(ITypeSymbol symbol, Microsoft.CodeAnalysis.Solution solution, string projectName)
        {
            if (symbol == null)
                return default;

            List<IdentifiedHandler> handlers = new();

            IEnumerable<ReferencedSymbol> references = default;
            ImmutableArray<ITypeSymbol> arguments = default;

            if (symbol is INamedTypeSymbol named)
            {
                arguments = named.TypeArguments;

                if (arguments.Any())
                {
                    symbol = arguments.First();
                }
            }
            references = await SymbolFinder.FindReferencesAsync(symbol, solution);

            foreach (var reference in references)
            {
                var locations = string.IsNullOrWhiteSpace(projectName) ?
                    reference.Locations
                    : reference.Locations.Where(l => l.Document.Project.AssemblyName == projectName);

                foreach (var location in locations)
                {
                    var tree = await location.Document.GetSyntaxTreeAsync();
                    var root = await tree.GetRootAsync();
                    var allMethods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
                    var publicMethods = default(IEnumerable<MethodDeclarationSyntax>);
                    publicMethods = allMethods.Where(publicMethod =>
                       publicMethod.Modifiers.Any(modifier => modifier.Text.Equals("public")) &&
                       publicMethod.ToFullString().Contains("HandleAsync") &&
                       publicMethod.ParameterList.Parameters.Any(parameter => parameter.ToFullString().Contains(symbol.Name)));

                    if (!publicMethods.Any())
                        continue;

                    if (arguments != default && arguments.Count() > 1)
                    {
                        var argumentName = arguments.Last().ToString();
                        var argumentParsed = argumentName.Split('.');

                        if (argumentParsed.Count() > 1)
                            argumentName = argumentParsed.Last();
                        publicMethods = publicMethods.Where(publicMethod => publicMethod.ReturnType.ToFullString().Contains(argumentName));

                        if (!publicMethods.Any())
                            continue;
                    }

                    if (!handlers.Any(x => x.SourceFile == location.Document.FilePath))
                        handlers.Add(new IdentifiedHandler
                        {
                            FileName = location.Document.Name,
                            SourceFile = location.Document.FilePath,
                            LineNumber = publicMethods.First().SpanStart
                        });
                }
            }

            return handlers;
        }
    }
}