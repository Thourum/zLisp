using System.Collections.Generic;

namespace ZLisp.Language.Syntax
{
    public sealed partial class Types
    {
        public class Vector : Expression
        {
            public override SyntaxKind Kind => SyntaxKind.Vector;

            public Vector() : base()
            {
                start = "[";
                end = "]";
            }

            public Vector(List<Value> val)
                    : base(val)
            {
                start = "[";
                end = "]";
            }

            public Vector(List<Value> contents, SourceSpan span = null) : base(contents, span)
            {
                start = "[";
                end = "]";
            }

            public override bool IsList() => false;

            public override Expression Slice(int start, int end)
            {
                var val = Contents;
                return new Vector(val.GetRange(start, val.Count - start));
            }
        }
    }
}