using System;
using ZLisp.Language;
using ZLisp.Language.Syntax;

namespace ZLisp.Runtime
{
    public class Func : Value
    {
        private readonly Func<Types.Expression, Value> fn = null;
        private readonly Value ast = null;
        private readonly Types.Expression fparams;
        private readonly Environment env = null;
        private bool macro = false;

        public Func(Func<Types.Expression, Value> fn)
        {
            this.fn = fn;
        }

        public Func(Value ast,
            Environment env,
            Types.Expression fparams,
            Func<Types.Expression, Value> fn)
        {
            this.fn = fn;
            this.ast = ast;
            this.env = env;
            this.fparams = fparams;
        }

        public override string ToString()
        {
            if (ast != null)
            {
                return "<fn* " + fparams.ToString() + " " + ast.ToString() + ">";
            }
            else
            {
                return "<builtin_function " + fn.ToString() + ">";
            }
        }

        public Value Apply(Types.Expression args) => fn(args);

        public Value GetAst() => ast;
        public Environment GetEnv() => env;
        public Environment GetEnv(Types.Expression args) => new Environment(env, fparams, args);

        public Types.Expression GetFParams() { return fparams; }
        public bool IsMacro() => macro;
        public void SetMacro() { macro = true; }

    }
}
