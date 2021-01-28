using Microsoft.CodeAnalysis;

namespace Avalonia.PropertyGenerator.CSharp
{
    internal sealed record Types(
        INamedTypeSymbol IAvaloniaObject,
        INamedTypeSymbol StyledProperty,
        INamedTypeSymbol DirectProperty,
        INamedTypeSymbol AttachedProperty)
    {
        public static Types? FromCompilation(Compilation compilation)
        {
            INamedTypeSymbol? GetType(string name) => compilation.GetTypeByMetadataName(name);

            var types = new Types(
                GetType("Avalonia.IAvaloniaObject")!,
                GetType("Avalonia.StyledProperty`1")!,
                GetType("Avalonia.DirectProperty`2")!,
                GetType("Avalonia.AttachedProperty`1")!);

            if (types.IAvaloniaObject is null
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
