using System.Collections.Generic;
using System.Linq;
using ZLisp.Language.Error;

namespace ZLisp.Language.Parser
{
    public sealed partial class Parser
    {
        public static SourceDocument Parse(string sourceCode)
        {
            var errorSink = new ErrorSink();
            var lexer = new Lexer.Lexer(errorSink);
            var parser = new Parser(errorSink);
            var src = new SourceCode(sourceCode);

            var tokens = lexer.LexFile(src).ToArray();
            if (errorSink.HasErrors) { errorSink.ConsolePrint(); return null; }

            var sourceDoc = parser.ParseFile(src, tokens);
            if (errorSink.HasErrors) { errorSink.ConsolePrint(); return null; }

            return sourceDoc;
        }

        public SourceDocument ParseSource(string sourceCode, IEnumerable<Token> tokens) => ParseFile(new SourceCode(sourceCode), tokens);

        public SourceDocument ParseFile(SourceCode sourceCode, IEnumerable<Token> tokens)
        {
            InitializeParser(sourceCode, tokens);
            try
            {
                List<Value> contents = new List<Value>();
                var start = _current.Span.Start;

                while (_current != TokenKind.EndOfFile)
                {
                    contents.Add(ParseInternal());
                }

                return new SourceDocument(CreateSpan(start), _sourceCode, contents);
            }
            catch (SyntaxException)
            {
                return null;
            }
        }
    }
}
