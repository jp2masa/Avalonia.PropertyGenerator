using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Avalonia.PropertyGenerator.CSharp
{
    internal sealed record DeclaringType(
        INamedTypeSymbol Type,
        ImmutableArray<StyledProperty> StyledProperties,
        ImmutableArray<DirectProperty> DirectProperties,
        ImmutableArray<AttachedProperty> AttachedProperties);
}
