using System;

namespace ZLisp.Language.Syntax
{
    public sealed partial class Types
    {
        public class Integer : SyntaxNode
        {
            public override SyntaxKind Kind => SyntaxKind.Integer;

            public Int64 Value { get; }

            public Integer(Int64 value, SourceSpan span = null) : base(span)
            {
                Value = value;
            }

            public override string ToString() => Value.ToString();
            public Int64 GetValue() => Value;

            public static Constant operator <(Integer a, Integer b) => a.Value < b.Value ? True : False;
            public static Constant operator <=(Integer a, Integer b) => a.Value <= b.Value ? True : False;
            public static Constant operator >(Integer a, Integer b) => a.Value > b.Value ? True : False;
            public static Constant operator >=(Integer a, Integer b) => a.Value >= b.Value ? True : False;
            public static Integer operator +(Integer a, Integer b) => new Integer(a.Value + b.Value);
            public static Integer operator -(Integer a, Integer b) => new Integer(a.Value - b.Value);
            public static Integer operator *(Integer a, Integer b) => new Integer(a.Value * b.Value);
            public static Integer operator /(Integer a, Integer b) => new Integer(a.Value / b.Value);
        }
    }
}