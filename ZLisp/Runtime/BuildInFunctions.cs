using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZLisp.Language;
using ZLisp.Language.Parser;
using ZLisp.Language.Syntax;
using static ZLisp.Language.Syntax.Types;

namespace ZLisp.Runtime
{
    public partial class Runtime
    {
        // Scalar functions
        private static readonly Func IsNil = new Func(a => a[0] == Nil ? True : False);
        private static readonly Func IsTrue = new Func(a => a[0] == True ? True : False);
        private static readonly Func IsFalse = new Func(a => a[0] == False ? True : False);
        private static readonly Func IsSymbol = new Func(a => a[0] is Symbol ? True : False);

        private static readonly Func IsString = new Func(a =>
            {
                if (a[0] is Types.String str)
                {
                    var s = str.GetValue();
                    return (s.Length == 0 || s[0] != '\u029e') ? True : False;
                }
                else
                {
                    return False;
                }
            });

        private static readonly Func Keyword = new Func(a =>
            {
                if (a[0] is Types.String str && str.GetValue()[0] == '\u029e')
                {
                    return str;
                }
                else
                {
                    return new Types.String("\u029e" + ((Types.String)a[0]).GetValue());
                }
            });

        private static readonly Func IsKeyword = new Func(a =>
            {
                if (a[0] is Types.String str)
                {
                    var s = str.GetValue();
                    return (s.Length > 0 && s[0] == '\u029e') ? True : False;
                }

                return False;
            });

        private static readonly Func IsNumber = new Func(a => a[0] is Integer ? True : False);
        private static readonly Func IsFunction = new Func(a => a[0] is Func func && !func.IsMacro() ? True : False);
        private static readonly Func IsMacro = new Func(a => a[0] is Func func && func.IsMacro() ? True : False);


        // Number functions
        private static readonly Func TimeMs = new Func(a => new Integer(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond));

        // String functions
        public static readonly Func PrStr = new Func(a => new Types.String(a.ToString()));
        public static readonly Func Str = new Func(a => new Types.String(a.ToString(false)));
        public static readonly Func Prn = new Func(a =>
            {
                Console.Write(a.ToString(false));
                return Nil;
            });

        public static readonly Func Println = new Func(a =>
            {
                Console.WriteLine(a.ToString(false));
                return Nil;
            });

        public static readonly Func Readline = new Func(a =>
            {
                Console.Write((((Types.String)a[0]).GetValue()));
                Console.Out.Flush();
                var line = Console.ReadLine();
                if (line == null)
                {
                    return Nil;
                }
                return new Types.String(line);
            });

        public static readonly Func EvalSrc = new Func(ast =>
        {
            if (ast[0] is Types.String str)
            {
                var srcDoc = Parser.Parse(str.GetValue());
                return Eval(srcDoc.Children.First(), Env, srcDoc.SourceCode);
            }
            else
            {
                throw new RuntimeException($"eval argument is not a string");
            }
        });


        public static readonly Func ReadString = new Func(
            a => Parser.Parse(a[0] is Types.String str ? 
                str.GetValue() : 
                throw new RuntimeException($"read-string argument {a[0]} is not a string"))
            .Children.First());
        public static readonly Func Slurp = new Func(a => new Types.String(File.ReadAllText(((Types.String)a[0]).GetValue())));


        // List/Vector functions
        public static readonly Func IsList = new Func(a => a[0].GetType() == typeof(Expression) ? True : False);
        public static readonly Func IsVector = new Func(a => a[0].GetType() == typeof(Vector) ? True : False);

        // HashMap functions
        public static readonly Func IsHashMap = new Func(a => a[0].GetType() == typeof(HashMap) ? True : False);
        private static readonly Func IsContains = new Func(a =>
            {
                string key = ((Types.String)a[1]).GetValue();
                var dict = ((HashMap)a[0]).GetValue();
                return dict.ContainsKey(key) ? True : False;
            });

        private static readonly Func Assoc = new Func(a =>
            {
                var new_hm = ((HashMap)a[0]).Copy();
                return new_hm.AssocBANG(a.Slice(1));
            });

        private static readonly Func Dissoc = new Func(a =>
            {
                var new_hm = ((HashMap)a[0]).Copy();
                return new_hm.DissocBANG(a.Slice(1));
            });

        private static readonly Func Get = new Func(a =>
            {
                string key = ((Types.String)a[1]).GetValue();
                if (a[0] == Nil)
                {
                    return Nil;
                }
                else
                {
                    var dict = ((HashMap)a[0]).GetValue();
                    return dict.ContainsKey(key) ? dict[key] : Nil;
                }
            });

        private static readonly Func Keys = new Func(a =>
            {
                var dict = ((HashMap)a[0]).GetValue();
                Expression key_lst = new Expression();
                foreach (var key in dict.Keys)
                {
                    key_lst.ConjBANG(new Types.String(key));
                }
                return key_lst;
            });

        private static readonly Func Vals = new Func(a =>
            {
                var dict = ((HashMap)a[0]).GetValue();
                Expression val_lst = new Expression();
                foreach (var val in dict.Values)
                {
                    val_lst.ConjBANG(val);
                }
                return val_lst;
            });

        // Sequence functions
        public static readonly Func IsSequential = new Func(a => a[0] is Expression ? True : False);

        private static readonly Func Cons = new Func(a =>
            {
                var lst = new List<Value> { a[0] };
                lst.AddRange(((Expression)a[1]).GetValue());
                return new Expression(lst);
            });

        private static readonly Func Concat = new Func(a =>
            {
                if (a.Size() == 0) { return new Expression(); }
                var lst = new List<Value>();
                for (int i = 0; i < a.Size(); i++)
                {
                    if (a[i] is Expression e)
                    {
                        lst.AddRange(e.GetValue());
                    }
                    else
                    {
                        lst.Add(a[i]);
                    }
                }
                return new Expression(lst);
            });        

        private static readonly Func Nth = new Func(a =>
            {
                var idx = (int)((Integer)a[1]).GetValue();
                if (idx > ((Expression)a[0]).Size())
                {
                    throw new RuntimeException("nth: index out of range");
                }

                return ((Expression)a[0])[idx];
            });

        private static readonly Func First = new Func(a => a[0] == Nil ? Nil : ((Expression)a[0])[0]);
        private static readonly Func Rest = new Func(a => a[0] == Nil ? new Expression() : ((Expression)a[0]).Rest());
        private static readonly Func IsEmpty = new Func(a => ((Expression)a[0]).Size() == 0 ? True : False);
        private static readonly Func Count = new Func(a => (a[0] == Nil) ? new Integer(0) : new Integer(((Expression)a[0]).Size()));
        private static readonly Func Conj = new Func(a =>
            {
                var src_lst = ((Expression)a[0]).GetValue();
                var new_lst = new List<Value>();
                new_lst.AddRange(src_lst);
                if (a[0] is Vector)
                {
                    for (int i = 1; i < a.Size(); i++)
                    {
                        new_lst.Add(a[i]);
                    }
                    return new Vector(new_lst);
                }
                else
                {
                    for (int i = 1; i < a.Size(); i++)
                    {
                        new_lst.Insert(0, a[i]);
                    }
                    return new Expression(new_lst);
                }
            });


        private static readonly Func Seq = new Func(a =>
            {
                if (a[0] == Nil)
                {
                    return Nil;
                }
                else if (a[0] is Vector v)
                {
                    if (v.Size() == 0)
                    {
                        return Nil;
                    }
                    else
                    {
                        return new Expression(v.GetValue());
                    }
                }
                else if (a[0] is Expression exp)
                {
                    if (exp.Size() == 0)
                    {
                        return Nil;
                    }
                    else
                    {
                        return exp;
                    }
                }
                else if (a[0] is Types.String str)
                {
                    var strVal = str.GetValue();
                    if (strVal.Length == 0)
                    {
                        return Nil;
                    }
                    var chars_list = new List<Value>();
                    foreach (var c in strVal)
                    {
                        chars_list.Add(new Types.String(c.ToString()));
                    }
                    return new Expression(chars_list);
                }
                return Nil;
            });

        // General list related functions
        private static readonly Func Apply = new Func(a =>
            {
                var f = (Func)a[0];
                var lst = new List<Value>();
                lst.AddRange(a.Slice(1, a.Size() - 1).GetValue());
                lst.AddRange(((Expression)a[a.Size() - 1]).GetValue());
                return f.Apply(new Expression(lst));
            });

        private static readonly Func Map = new Func(a =>
            {
                Func f = (Func)a[0];
                var src_lst = ((Expression)a[1]).GetValue();
                var new_lst = new List<Value>();
                for (int i = 0; i < src_lst.Count; i++)
                {
                    new_lst.Add(f.Apply(new Expression(src_lst[i])));
                }
                return new Expression(new_lst);
            });


        // Metadata functions
        private static readonly Func Meta = new Func(a => a[0].GetMeta());
        private static readonly Func WithMeta = new Func(a => a[0].Copy().SetMeta(a[1]));


        // Atom functions
        private static readonly Func IsAtom = new Func(a => a[0] is Atom ? True : False);
        private static readonly Func Deref = new Func(a => ((Atom)a[0]).GetValue());
        private static readonly Func ResetBANG = new Func(a => ((Atom)a[0]).SetValue(a[1]));
        private static readonly Func SwapBANG = new Func(a =>
            {
                Atom atm = (Atom)a[0];
                Func f = (Func)a[1];
                var new_lst = new List<Value> { atm.GetValue() };
                new_lst.AddRange(a.Slice(2).GetValue());
                return atm.SetValue(f.Apply(new Expression(new_lst)));
            });

        // General Functions
        public static bool IsEqual(Value a, Value b)
        {
            if (!((a is SyntaxNode snA && b is SyntaxNode snB && snA.Kind == snB.Kind)
                || (a is Expression && b is Expression)))
            {
                return false;
            }

            if (a is Integer num)
            {
                return num.GetValue() == ((Integer)b).GetValue();
            }
            else if (a is Symbol symbol)
            {
                return symbol.GetName() == ((Symbol)b).GetName();
            }
            else if (a is Types.String str)
            {
                return str.GetValue() == ((Types.String)b).GetValue();
            }
            else if (a is Expression expA)
            {
                var expB = (Expression)b;
                if (expA.Size() != expB.Size())
                {
                    return false;
                }
                for (int i = 0; i < expA.Size(); i++)
                {
                    if (!IsEqual(expA[i], expB[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (a is HashMap)
            {
                var akeys = ((HashMap)a).GetValue().Keys;
                var bkeys = ((HashMap)b).GetValue().Keys;
                if (akeys.Count != bkeys.Count)
                {
                    return false;
                }
                foreach (var k in akeys)
                {
                    if (!IsEqual(((HashMap)a).GetValue()[k],
                                  ((HashMap)b).GetValue()[k]))
                    {
                        return false;
                    }
                }
                return true;
            }

            return a == b;
        }

        public static Dictionary<string, Value> ns = new Dictionary<string, Value> {
            {"=",  new Func(a => IsEqual(a[0], a[1]) ? True : False)},
            {"nil?", IsNil},
            {"true?", IsTrue},
            {"false?", IsFalse},
            {"symbol", new Func(a => new Symbol(((Types.String)a[0]).ToString()))},
            {"symbol?", IsSymbol},
            {"string?", IsString},
            {"keyword", Keyword},
            {"keyword?", IsKeyword},
            {"number?", IsNumber},
            {"fn?", IsFunction},
            {"macro?", IsMacro},

            {"pr-str", PrStr},
            {"str", Str},
            {"prn", Prn},
            {"println", Println},
            {"readline", Readline},
            {"read-string", ReadString},
            {"eval", EvalSrc },
            {"slurp", Slurp},
            {"time-ms", TimeMs},

            {"<",  new Func(a => (Integer)a[0] <  (Integer)a[1])},
            {"<=", new Func(a => (Integer)a[0] <= (Integer)a[1])},
            {">",  new Func(a => (Integer)a[0] >  (Integer)a[1])},
            {">=", new Func(a => (Integer)a[0] >= (Integer)a[1])},
            {"+",  new Func(a => (Integer)a[0] +  (Integer)a[1])},
            {"-",  new Func(a => (Integer)a[0] -  (Integer)a[1])},
            {"*",  new Func(a => (Integer)a[0] *  (Integer)a[1])},
            {"/",  new Func(a => (Integer)a[0] /  (Integer)a[1])},

            {"list",  new Func(a => new Expression(a.GetValue()))},
            {"list?", IsList},
            {"vector",  new Func(a => new Vector(a.GetValue()))},
            {"vector?", IsVector},
            {"hash-map",  new Func(a => new HashMap(a))},
            {"map?", IsHashMap},
            {"contains?", IsContains},
            {"assoc", Assoc},
            {"dissoc", Dissoc},
            {"get", Get},
            {"keys", Keys},
            {"vals", Vals},

            {"sequential?", IsSequential},
            {"cons", Cons},
            {"concat", Concat},
            {"vec",  new Func(a => new Vector(((Expression)a[0]).GetValue()))},
            {"nth", Nth},
            {"first", First},
            {"rest",  Rest},
            {"empty?", IsEmpty},
            {"count", Count},
            {"conj", Conj},
            {"seq", Seq},
            {"Apply", Apply},
            {"map", Map},

            {"with-meta", WithMeta},
            {"meta", Meta},
            {"atom", new Func(a => new Atom(a[0]))},
            {"atom?", IsAtom},
            {"deref", Deref},
            {"reset!", ResetBANG},
            {"swap!", SwapBANG},
        };
    }
}
