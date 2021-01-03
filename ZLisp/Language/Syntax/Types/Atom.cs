namespace ZLisp.Language.Syntax
{
    public sealed partial class Types
    {
        public class Atom : SyntaxNode
        {
            public override SyntaxKind Kind => SyntaxKind.Atom;

            private Value _value { get; set; }
            public Value Value { get => _value; }

            public Atom(Value value, SourceSpan span = null) : base(span)
            {
                _value = value;
            }

            public Value GetValue() => _value;
            public Value SetValue(Value v) => _value = v;
            public override string ToString() => _value.ToString();
        }
    }
}
