namespace ZLisp.Language.Syntax
{
    public sealed partial class Types
    {
        public class Comment : SyntaxNode
        {
            public override SyntaxKind Kind => SyntaxKind.Comment;

            public string Value { get; }

            public Comment(string value, SourceSpan span = null) : base(span)
            {
                Value = value;
            }
        }
    }
}
