using System;
using System.Linq;

namespace ZLisp.Language.Error
{
    public sealed class ErrorEntry
    {
        public string[] Lines { get; }

        public string Message { get; }

        public Severity Severity { get; }

        public SourceSpan Span { get; }

        public ErrorEntry(string message, string[] lines, Severity severity, SourceSpan span)
        {
            Message = message;
            Lines = lines;
            Span = span;
            Severity = severity;
        }

        public override string ToString() => $"{Severity}: {Message}{Environment.NewLine}Line: {Lines.First()}";

        public void ConsolePrint()
        {
            Console.ForegroundColor = Severity switch
            {
                Severity.Fatal => ConsoleColor.DarkRed,
                Severity.Error => ConsoleColor.Red,
                Severity.Warning => ConsoleColor.Yellow,
                _ => ConsoleColor.White,
            };
            if (Lines.Length > 1)
            {
                Console.WriteLine(Lines.First());
                Console.CursorLeft = Span.Start.Column;
                Console.WriteLine(new string('^', Lines[0].Length - Span.Start.Column));
                for (int i = 1; i < Lines.Length - 1; i++)
                {
                    Console.WriteLine(Lines[i]);
                    Console.WriteLine(new string('^', Lines[i].Length));
                }
                Console.WriteLine(Lines.Last());
                Console.WriteLine(new string('^', Lines.Last().Length - Span.End.Column));
            }
            else
            {
                Console.WriteLine(Lines.First());
                Console.CursorLeft = Span.Start.Column;
                Console.WriteLine(new string('^', Span.Length));
                Console.WriteLine($"{Severity} {Span}: {Message}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
