namespace ZLisp.Language.Syntax
{
    public sealed partial class Types
    {
        public class Constant : SyntaxNode
        {
            public override SyntaxKind Kind => SyntaxKind.Constant;

            public string Value { get; }

            public Constant(string value, SourceSpan span = null) : base(span)
            {
                Value = value;
            }

            public override string ToString() => Value.ToString();

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
                if (!(obj is Constant)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return Value == ((Constant)obj).Value;
            }
            public override int GetHashCode() => Value.GetHashCode();
            public static bool operator ==(Constant a, Constant b) =>
                ReferenceEquals(a, b) || (!(a is null) && a.Equals(b));

            public static bool operator !=(Constant a, Constant b) => !(a == b);
            public static bool operator !=(object a, Constant b) => a is Constant aConst && !(aConst == b);
            public static bool operator ==(object a, Constant b) => a is Constant aConst && aConst == b;
        }

        public static Constant Nil = new Constant("nil");
        public static Constant True = new Constant("true");
        public static Constant False = new Constant("false");
    }
}
