using System;
using Microsoft.CodeAnalysis;

namespace Avalonia.PropertyGenerator.CSharp
{
    internal sealed record Types(
        INamedTypeSymbol AvaloniaObject,
        INamedTypeSymbol StyledProperty,
        INamedTypeSymbol DirectProperty,
        INamedTypeSymbol AttachedProperty)
    {
        private const string GetTypeWarningId = "AVPROP01";
        private const string GetTypeWarningTitle = "AVPROP01";
        private const string GetTypeWarningMessage = "Failed to load '{0}'";
        private const string GetTypeWarningCategory = "Avalonia.PropertyGenerator.CSharp";
        private const string GetTypeWarningDescription = "The Avalonia property generator was unable to find a property type from the Avalonia assemblies. Make sure Avalonia is correctly referenced. If you believe this is a bug, please report it: https://github.com/jp2masa/Avalonia.PropertyGenerator/issues/new.";

        private static readonly DiagnosticDescriptor GetTypeWarning =
            new DiagnosticDescriptor(
                GetTypeWarningId,
                GetTypeWarningTitle,
                GetTypeWarningMessage,
                GetTypeWarningCategory,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                GetTypeWarningDescription);

        public static Types? FromCompilation(Compilation compilation, Action<Diagnostic>? reportDiagnostic)
        {
            INamedTypeSymbol? GetType(string name)
            {
                var type = compilation.GetTypeByMetadataName(name);

                if (type is null)
                {
                    reportDiagnostic?.Invoke(Diagnostic.Create(GetTypeWarning, null, name));
                    return null;
                }

                if (type.IsGenericType && !type.IsUnboundGenericType)
                {
                    type = type.ConstructUnboundGenericType();
                }

                return type;
            }

            var types = new Types(
                GetType("Avalonia.AvaloniaObject")!,
                GetType("Avalonia.StyledProperty`1")!,
                GetType("Avalonia.DirectProperty`2")!,
                GetType("Avalonia.AttachedProperty`1")!);

            if (types.AvaloniaObject is null
                || types.StyledProperty is null
                || types.DirectProperty is null
                || types.AttachedProperty is null)
            {
                return null;
            }

            return types;
        }
    }
}
