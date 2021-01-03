namespace ZLisp.Language
{
    public abstract class SyntaxNode : Value
    {
        public abstract SyntaxKind Kind { get; }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public SourceSpan? Span { get; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

        protected SyntaxNode(SourceSpan span = null)
        {
            Span = span;
        }
    }

    public enum SyntaxKind
    {
        Invalid,
        Atom,
        Expression,
        Integer,
        Float,
        String,
        Constant,
        Boolean,
        Identifier,
        Function,
        Vector,
        Symbol,
        HashMap,
        Comment,
        Keyword
    }
}
