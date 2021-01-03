using System.Collections.Generic;
using System.Linq;

namespace ZLisp.Language.Syntax
{
    public sealed partial class Types
    {
        public class Expression : SyntaxNode
        {
            private List<Value> _contents { get; set; }
            public List<Value> Contents { get => _contents; }

            public string start = "(", end = ")";
            public override SyntaxKind Kind => SyntaxKind.Expression;
            public override bool IsList() => true;



            public Expression(SourceSpan span = null) : base(span)
            {
                _contents = new List<Value>();
            }

            public Expression(List<Value> contents, SourceSpan span = null) : base(span)
            {
                _contents = contents;
            }

            public Expression(params Value[] mvs) : base()
            {
                _contents = new List<Value>();
                ConjBANG(mvs);
            }

            public Expression ConjBANG(params Value[] mvs)
            {
                for (int i = 0; i < mvs.Length; i++)
                {
                    _contents.Add(mvs[i]);
                }
                return this;
            }

            public override string ToString() => start + string.Join(" ", _contents) + end;
            public string ToString(bool printReadably) => printReadably ?
                start + string.Join(" ", _contents) + end :
                string.Join(" ", _contents.Select(x => x is String str ? str.GetValue() : x.ToString()));
            public int Size() => _contents.Count;
            public Value Nth(int idx) => _contents.Count > idx ? _contents[idx] : Nil;
            public Value this[int idx] { get => _contents.Count > idx ? _contents[idx] : Nil; }
            public virtual Expression Slice(int start) => new Expression(_contents.GetRange(start, _contents.Count - start));
            public virtual Expression Slice(int start, int end) => new Expression(_contents.GetRange(start, end - start));
            public Expression Rest() => Size() > 0 ? new Expression(_contents.GetRange(1, _contents.Count - 1)) : new Expression();
            public List<Value> GetValue() => _contents;

        }
    }
}