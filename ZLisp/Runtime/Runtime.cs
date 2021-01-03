using System;
using System.Collections.Generic;
using System.Linq;
using ZLisp.Language;
using ZLisp.Language.Parser;
using static ZLisp.Language.Syntax.Types;

namespace ZLisp.Runtime
{
    public partial class Runtime
    {
        public static Environment Env { get => EnvironmentSingleton.Instance.Env; }

        public static Value Eval(string src) => Eval(Parser.Parse(src));
        public static Value Eval(SourceDocument srcDoc) => Eval(srcDoc.Children);
        public static Value Eval(IEnumerable<Value> ast) => Eval(ast, Env);

        public static Value Eval(IEnumerable<Value> ast, Environment env)
        {
            if (!ast.Any())
            {
                throw new ArgumentNullException(nameof(ast));
            }

            if (ast.Count() == 1)
            {
                return Eval(ast.First(), env);
            }

            var result = new Expression();
            foreach (var node in ast)
            {
                result.ConjBANG(Eval(node, env));
            }
            return result;
        }

        public static Value Eval(Value node, Environment env)
        {
            while (true)
            {
                if (!node.IsList())
                {
                    return EvalAst(node, env);
                }

                var expanded = ExpandMacro(node, env);
                if (!expanded.IsList())
                {
                    return EvalAst(expanded, env);
                }

                var ast = (Expression)expanded;
                if (ast.Size() == 0)
                {
                    return node;
                }

                var a0sym = ast[0] is Symbol symbol ? symbol.GetName() : "__<*fn*>__";

                switch (a0sym)
                {
                    case "def":
                        var result = Eval(ast[2], env);
                        env.Set((Symbol)ast[1], result);
                        return result;
                    case "let":
                        var arg1 = (Expression)ast[1];
                        var let_env = new Environment(env);
                        for (int i = 0; i < arg1.Size(); i += 2) // Every other
                        {
                            var key = (Symbol)arg1[i];
                            var val = arg1[i + 1];
                            let_env.Set(key, Eval(val, let_env));
                        }
                        node = ast[2];
                        env = let_env;
                        continue;
                    case "fn":
                        var fnParam = (Expression)ast[1];
                        var fnBody = ast[2];
                        var cur_env = env;
                        return new Func(fnBody, env, fnParam, args => Eval(fnBody, new Environment(cur_env, fnParam, args)));


                    case "quote":
                        return ast[1];
                    case "quasiquote":
                        node = Quasiquote(ast[1]);
                        continue;
                    case "quasiquoteexpand":
                        return Quasiquote(ast[1]);


                    case "defmacro":
                        var res = (Func)Eval(ast[2], env);
                        res.SetMacro();
                        env.Set(((Symbol)ast[1]), res);
                        return res;
                    case "macroexpand":
                        return ExpandMacro(ast[1], env);


                    case "do":
                        EvalAst(ast.Slice(1, ast.Size() - 1), env);
                        node = ast[ast.Size() - 1];
                        continue;
                    case "if":
                        var cond = Eval(ast[1], env);
                        var isTrue = !(cond == Nil || cond == False);
                        if (!isTrue && ast.Size() < 3)
                        {
                            return Nil;
                        }
                        node = isTrue ? ast[2] : ast[3];
                        continue;


                    default:
                        var el = (Expression)EvalAst(ast, env);
                        var fn = (Func)el[0];
                        var fnAst = fn.GetAst();
                        if (fnAst != null)
                        {
                            node = fnAst;
                            env = fn.GetEnv(el.Rest());
                        }
                        else
                        {
                            return fn.Apply(el.Rest());
                        }
                        continue;
                }
            }
        }

        private static Value EvalAst(Value ast, Environment env)
        {
            switch (ast)
            {
                case Symbol sym:
                    return env.Get(sym);

                case Expression exp:
                    var list = exp.IsList() ? new Expression() : new Vector();
                    exp.Contents.ForEach(x => list.ConjBANG(Eval(x, env)));
                    return list;

                case HashMap map:
                    var dict = new Dictionary<string, Value>();
                    foreach (var entry in map.Contents)
                    {
                        dict.Add(entry.Key, Eval(entry.Value, env));
                    }
                    return new HashMap(dict);

                default:
                    return ast;
            }
        }
        private static Value Quasiquote(Value ast)
        {
            switch (ast)
            {
                case Vector _:
                    return new Expression(new Symbol("vec"), LoopQQ(((Expression)ast)));
                case Expression list:
                    return StartsWith(list, "unquote") ? list[1] : LoopQQ(list);
                case HashMap _:
                case Symbol _:
                    return new Expression(new Symbol("quote"), ast);
                default:
                    return ast;
            };
        }

        private static bool StartsWith(Value ast, string sym)
        {
            if (ast is Expression list && !(ast is Vector))
            {
                if (list.Size() == 2 && list[0] is Symbol symbol)
                {
                    return symbol.GetName() == sym;
                }
            }
            return false;
        }

        private static Value LoopQQ(Expression ast)
        {
            var acc = new Expression();
            for (var i = ast.Size() - 1; 0 <= i; i--) // Right to left
            {
                Value elt = ast[i];
                if (StartsWith(elt, "splice-unquote"))
                {
                    acc = new Expression(new Symbol("concat"), ((Expression)elt)[1], acc);
                }
                else
                {
                    acc = new Expression(new Symbol("cons"), Quasiquote(elt), acc);
                }
            }
            return acc;
        }

        public static bool IsMacroCall(Value ast, Environment env)
        {
            if (ast is Expression list)
            {
                if (list[0] is Symbol sym && env.Find(sym) != null)
                {
                    if (env.Get(sym) is Func fn && fn.IsMacro())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static Value ExpandMacro(Value ast, Environment env)
        {
            while (IsMacroCall(ast, env))
            {
                var list = (Expression)ast;
                var mac = (Func)env.Get((Symbol)list[0]);
                ast = mac.Apply(list.Rest());
            }
            return ast;
        }
    }
}
