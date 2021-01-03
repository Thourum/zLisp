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
            if (errorSink.HasErrors) { errorSink.ConsolePrint(); }

            var sourceDoc = parser.ParseFile(src, tokens);
            if (errorSink.HasErrors) { errorSink.ConsolePrint(); }

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

                if (_current != TokenKind.EndOfFile)
                {
                    AddError(Severity.Error, "Top-level statements are not permitted within the current options.", CreateSpan(_current.Span.Start, _tokens.Last().Span.End));
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
