namespace ZLisp.Language.Syntax
{
    public sealed partial class Types
    {
        public class String : SyntaxNode
        {
            public override SyntaxKind Kind => SyntaxKind.String;

            public string Value { get; }

            public String(string value, SourceSpan span = null) : base(span)
            {
                Value = value;
            }

            public override string ToString() => "\"" + Value + "\"";

            public string ToString(bool printReadably) => printReadably ? "\"" + Value + "\"" : Value;

            public string GetValue() => Value;
        }
    }
}