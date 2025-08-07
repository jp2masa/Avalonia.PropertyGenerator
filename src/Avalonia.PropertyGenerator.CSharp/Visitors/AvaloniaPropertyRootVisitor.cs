using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Avalonia.PropertyGenerator.CSharp.Visitors
{
    internal sealed class AvaloniaPropertyRootVisitor(Types types)
        : SymbolVisitor<ImmutableArray<DeclaringType>?>
    {
        private readonly Types _types = types;

        public override ImmutableArray<DeclaringType>? VisitNamespace(INamespaceSymbol symbol)
        {
            var builder = ImmutableArray.CreateBuilder<DeclaringType>();
            var visitor = new AvaloniaPropertyDeclaringTypeVisitor(_types);

            foreach (var member in symbol.GetMembers())
            {
                var ns = member.Accept(this);

                if (ns is not null)
                {
                    builder.AddRange(ns);
                }

                var type = member.Accept(visitor);

                if (type is not null)
                {
                    builder.Add(type);
                }
            }

            return builder.Count > 0 ? builder.ToImmutable() : null;
        }
    }
}
