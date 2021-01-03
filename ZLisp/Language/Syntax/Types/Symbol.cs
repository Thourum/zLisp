namespace ZLisp.Language.Syntax
{
    public sealed partial class Types
    {
        public class Symbol : SyntaxNode
        {
            public override SyntaxKind Kind => SyntaxKind.Symbol;

            public string Value { get; }

            public Symbol(String v, SourceSpan span = null) : base(span)
            {
                Value = v.GetValue();
            }

            public Symbol(string value, SourceSpan span = null) : base(span)
            {
                Value = value;
            }

            public string GetName() => Value;
            public override string ToString() => Value;
        }
    }
}