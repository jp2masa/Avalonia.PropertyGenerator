using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Avalonia.PropertyGenerator.CSharp.Visitors
{
    internal sealed class AvaloniaPropertyRootVisitor(Types types)
        : SymbolVisitor<ImmutableArray<DeclaringType>?>
    {
        private readonly AvaloniaPropertyDeclaringTypeVisitor _visitor =
            new AvaloniaPropertyDeclaringTypeVisitor(types);

        public override ImmutableArray<DeclaringType>? VisitNamespace(INamespaceSymbol symbol)
        {
            ImmutableArray<DeclaringType>.Builder? builder = null;

            foreach (var member in symbol.GetMembers())
            {
                var ns = member.Accept(this);

                if (ns is not null)
                {
                    (builder ??= ImmutableArray.CreateBuilder<DeclaringType>())
                        .AddRange(ns);
                }

                var type = member.Accept(_visitor);

                if (type is not null)
                {
                    (builder ??= ImmutableArray.CreateBuilder<DeclaringType>())
                        .Add(type);
                }
            }

            return builder?.ToImmutable();
        }
    }
}
