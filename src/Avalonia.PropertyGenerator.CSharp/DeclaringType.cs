using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Avalonia.PropertyGenerator.CSharp
{
    internal sealed record DeclaringType(
        INamedTypeSymbol Type,
        ImmutableArray<Property> StyledProperties,
        ImmutableArray<Property> DirectProperties,
        ImmutableArray<Property> AttachedProperties);
}
