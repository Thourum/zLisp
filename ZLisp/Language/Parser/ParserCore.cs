using System;
using System.Collections.Generic;
using System.Linq;
using ZLisp.Language.Error;

namespace ZLisp.Language.Parser
{
    public sealed partial class Parser
    {
        private bool _error;
        private ErrorSink _errorSink;
        private int _index;
        private SourceCode _sourceCode;
        private IEnumerable<Token> _tokens;

        private Token _current => _tokens.ElementAtOrDefault(_index) ?? _tokens.Last();

        private Token _last => Peek(-1);

        private Token _next => Peek(1);

        public Parser()
            : this(new ErrorSink())
        {
        }

        public Parser(ErrorSink errorSink)
        {
            _errorSink = errorSink;
        }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        private void AddError(Severity severity, string message, SourceSpan? span = null)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            _errorSink.AddError(message, _sourceCode, severity, span ?? CreateSpan(_current));
        }

        private void Advance()
        {
            _index++;
        }

        private static SourceSpan CreateSpan(SourceLocation start, SourceLocation end) => new SourceSpan(start, end);

        private SourceSpan CreateSpan(Token start) => CreateSpan(start.Span.Start, _current.Span.End);

        private SourceSpan CreateSpan(SourceLocation start) => CreateSpan(start, _current.Span.End);

        private void InitializeParser(SourceCode sourceCode, IEnumerable<Token> tokens)
        {
            _sourceCode = sourceCode;
            _tokens = tokens.Where(g => !g.IsTrivia()).ToArray();
            _index = 0;
        }

        private void TakeScope(Action action, TokenKind openKind = TokenKind.LeftParenthesis, TokenKind closeKind = TokenKind.RightParenthesis)
        {
            try
            {
                Take(openKind);
                while (_current != closeKind && _current != TokenKind.EndOfFile)
                {
                    action();
                }
                Take(closeKind);
            }
            catch (SyntaxException)
            {
                while (_current != closeKind && _current != TokenKind.EndOfFile)
                {
                    Take();
                }
            }
            finally
            {
                if (_error)
                {
                    if (_last == closeKind)
                    {
                        _index--;
                    }
                    if (_current != closeKind)
                    {
                        while (_current != closeKind && _current != TokenKind.EndOfFile)
                        {
                            Take();
                        }
                    }
                    _error = false;
                }
            }
        }

        private Token Peek(int ahead)
        {
            return _tokens.ElementAtOrDefault(_index + ahead) ?? _tokens.Last();
        }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        private SyntaxException SyntaxError(Severity severity, string message, SourceSpan? span = null)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            _error = true;
            AddError(severity, message, span);
            return new SyntaxException(message);
        }

        private Token Take()
        {
            var token = _current;
            Advance();
            return token;
        }

        private Token Take(TokenKind kind)
        {
            if (_current != kind)
            {
                throw UnexpectedToken(kind);
            }
            return Take();
        }

        private SyntaxException UnexpectedToken(TokenKind expected)
        {
            return UnexpectedToken(expected.ToString());
        }

        private SyntaxException UnexpectedToken(string expected)
        {
            Advance();
            var value = string.IsNullOrEmpty(_last?.Value) ? _last?.Kind.ToString() : _last?.Value;
            string message = $"Unexpected '{value}'.  Expected '{expected}'";

            return SyntaxError(Severity.Error, message, _last.Span);
        }

    }
}
