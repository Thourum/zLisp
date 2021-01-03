using System.Collections.Generic;
using ZLisp.Language;
using ZLisp.Language.Parser;
using ZLisp.Language.Syntax;

namespace ZLisp.Runtime
{
    public class Environment
    {
        private readonly Environment _outer = null;
        private readonly Dictionary<string, Value> _data = new Dictionary<string, Value>();

        public Environment(Environment outer)
        {
            _outer = outer;
        }

        public Environment(Environment outer, Types.Expression binds, Types.Expression exprs)
        {
            _outer = outer;
            for (var i = 0; i < binds.Size(); i++)
            {
                var sym = ((Types.Symbol)binds.Nth(i)).GetName();
                if (sym == "ampersand")
                {
                    _data[((Types.Symbol)binds.Nth(i + 1)).GetName()] = exprs.Slice(i);
                    break;
                }
                else
                {
                    _data[sym] = exprs.Nth(i);
                }
            }
        }
        public Environment Find(Types.Symbol key)
        {
            if (_data.ContainsKey(key.GetName()))
            {
                return this;
            }
            else if (_outer != null)
            {
                return _outer.Find(key);
            }
            else
            {
                return null;
            }
        }

        public Value Get(Types.Symbol key)
        {
            Environment e = Find(key);
            if (e == null)
            {
                throw new SyntaxException(
                        "'" + key.GetName() + "' not found");
            }
            else
            {
                return e._data[key.GetName()];
            }
        }

        public Environment Set(Types.Symbol key, Value value)
        {
            _data[key.GetName()] = value;
            return this;
        }
    }
}
