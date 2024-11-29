using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SealedClassAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SealedClassAnalyzerCodeFixProvider)), Shared]
    public class SealedClassAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Add modifier 'sealed'";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(SealedClassAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            // Получаем первое диагностическое сообщение
            var diagnostic = context.Diagnostics[0];
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Регистрируем кодовое действие для предоставления исправления
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => AddSealedModifierAsync(context.Document, diagnosticSpan, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> AddSealedModifierAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
        {
            // Получаем синтаксическое дерево и корневой узел
            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            // Находим узел класса по позиции диагностического сообщения
            var classDeclaration = syntaxRoot.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();

            // Проверяем, есть ли уже модификатор 'sealed' (на всякий случай)
            if (classDeclaration.Modifiers.Any(SyntaxKind.SealedKeyword))
                return document;

            // Добавляем модификатор 'sealed'
            var sealedModifier = SyntaxFactory.Token(SyntaxKind.SealedKeyword).WithTrailingTrivia(SyntaxFactory.Space);
            var newModifiers = classDeclaration.Modifiers.Add(sealedModifier);

            // Создаем новый узел класса с обновленными модификаторами
            var newClassDeclaration = classDeclaration.WithModifiers(newModifiers);

            // Обновляем синтаксическое дерево
            var newSyntaxRoot = syntaxRoot.ReplaceNode(classDeclaration, newClassDeclaration);

            // Возвращаем обновленный документ
            return document.WithSyntaxRoot(newSyntaxRoot);
        }
    }
}
