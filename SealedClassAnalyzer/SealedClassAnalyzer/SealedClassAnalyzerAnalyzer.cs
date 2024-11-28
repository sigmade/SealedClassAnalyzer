using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace SealedClassAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SealedClassAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SCA001";
        private static readonly LocalizableString Title = "Класс не является sealed";
        private static readonly LocalizableString MessageFormat = "Класс '{0}' не является sealed";
        private static readonly LocalizableString Description = "Рассмотрите возможность сделать класс sealed для безопасности и производительности";
        private const string Category = "Design";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            // Разрешить анализ только не сгенерированного кода
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // Регистрация действия для анализа символов
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // Получаем символ класса
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            // Проверяем, является ли символ классом
            if (namedTypeSymbol.TypeKind != TypeKind.Class)
                return;

            // Игнорируем абстрактные, статические и сеaled классы
            if (namedTypeSymbol.IsAbstract || namedTypeSymbol.IsStatic || namedTypeSymbol.IsSealed)
                return;

            // Создаем диагностическое сообщение
            var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

            // Отправляем диагностическое сообщение
            context.ReportDiagnostic(diagnostic);
        }
    }
}
