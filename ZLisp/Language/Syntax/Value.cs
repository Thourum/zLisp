using ZLisp.Language.Syntax;

namespace ZLisp.Language
{
    public abstract class Value
    {
        private Value meta = Types.Nil;
        public virtual Value Copy() => (Value)this.MemberwiseClone();

        public Value GetMeta() => meta;
        public Value SetMeta(Value m) { meta = m; return this; }
        public virtual bool IsList() => false;
    }
}
