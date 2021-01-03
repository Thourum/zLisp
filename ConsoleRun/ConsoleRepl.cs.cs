using System;
using ZLisp.Language.Error;
using ZLisp.Language.Lexer;
using ZLisp.Language.Parser;
using ZLisp.Runtime;
using static ZLisp.Language.Syntax.Types;

namespace ConsoleRepl
{
    class ConsoleRepl
    {
        static void Main(string[] args)
        {
            Runtime.Eval("(def not (fn (a) (if a false true)))");
            Runtime.Eval("(def load-file (fn (f) (eval (str \"(do\" (slurp f) \"\nnil)\"))))");
            Runtime.Eval("(defmacro cond (fn (& xs) (if (> (count xs) 0) (list 'if (first xs) (if (> (count xs) 1) (nth xs 1) (throw \"odd number of forms to cond\")) (cons 'cond (rest (rest xs)))))))");
            Runtime.Eval("(defmacro unless (fn (pred a b) `(if ~pred ~b ~a)))");
            Runtime.Eval("(defmacro defun (fn (n p b) `(def ~n (fn ~p ~b))))");

            if (args.Length > 0)
            {
                Expression _argv = new Expression();
                for (int i = 0; i < args.Length; i++)
                {
                    _argv.ConjBANG(new ZLisp.Language.Syntax.Types.String(args[i]));
                }
                Runtime.Env.Set(new Symbol("ARGV"), _argv);
                Runtime.Eval("(load-file \"" + args[0] + "\")");
                return;
            }

            PrintIntro();

            while (true)
            {
                string line;
                try
                {
                    Console.Write("zLisp > ");
                    line = Console.ReadLine();
                    line = System.Text.RegularExpressions.Regex.Unescape(line);
                    Runtime.Eval(line);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                    Console.WriteLine(e.StackTrace);
                    continue;
                }
            }
        }
        private static void PrintIntro()
        {
            const string introStr = @"
                   █████       █████  █████████  ███████████ 
                  ░░███       ░░███  ███░░░░░███░░███░░░░░███
      █████████    ░███        ░███ ░███    ░░░  ░███    ░███
     ░█░░░░███     ░███        ░███ ░░█████████  ░██████████ 
     ░   ███░      ░███        ░███  ░░░░░░░░███ ░███░░░░░░  
       ███░   █    ░███      █ ░███  ███    ░███ ░███        
      █████████    ███████████ █████░░█████████  █████       
     ░░░░░░░░░    ░░░░░░░░░░░ ░░░░░  ░░░░░░░░░  ░░░░░        
                                                         
    Welcome to Zealand LISP, a crude lisp implementation done in C#
    By Samuel Asvanyi for a school project @ Zealand 2021
";
            Console.WriteLine(introStr);
        }
    }
}
