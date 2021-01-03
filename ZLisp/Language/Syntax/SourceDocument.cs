using System.Collections.Generic;

namespace ZLisp.Language
{
    public class SourceDocument : SyntaxNode
    {
        public override SyntaxKind Kind => SyntaxKind.Invalid;

        public List<Value> Children { get; }

        public SourceCode SourceCode { get; }

        public SourceDocument(SourceSpan span, SourceCode sourceCode, List<Value> children)
            : base(span)
        {
            SourceCode = sourceCode;
            Children = children;
        }
    }
}
