using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZLisp.Language.Error;

namespace ZLisp.Language.Lexer
{
    public class Lexer
    {
        private static readonly string[] _Keywords = { "if", "nil", "true", "false", "and", "or" };

        private StringBuilder _builder;
        private int _column;
        private ErrorSink _errorSink;
        private int _index;
        private int _line;
        private SourceCode _sourceCode;
        private SourceLocation _tokenStart;

        public ErrorSink ErrorSink => _errorSink;

        private char _ch => _sourceCode[_index];

        private char _last => Peek(-1);

        private char _next => Peek(1);
        public Lexer()
                : this(new ErrorSink())
        {
        }
        public Lexer(ErrorSink errorSink)
        {
            _errorSink = errorSink;
            _builder = new StringBuilder();
            _sourceCode = null;
        }

        public IEnumerable<Token> LexFile(string sourceCode) => LexFile(new SourceCode(sourceCode));

        public IEnumerable<Token> LexFile(SourceCode sourceCode)
        {
            _sourceCode = sourceCode;
            _builder.Clear();
            _line = 1;
            _index = 0;
            _column = 0;
            _tokenStart = new SourceLocation(_index, _line, _column);

            return LexContents();
        }

        private void AddError(string message, Severity severity)
        {
            var span = new SourceSpan(_tokenStart, new SourceLocation(_index, _line, _column));
            _errorSink.AddError(message, _sourceCode, severity, span);
        }

        private void Advance()
        {
            _index++;
            _column++;
        }

        private void Consume()
        {
            _builder.Append(_ch);
            Advance();
        }

        private Token CreateToken(TokenKind kind)
        {
            string contents = _builder.ToString();
            SourceLocation end = new SourceLocation(_index, _line, _column);
            SourceLocation start = _tokenStart;

            _tokenStart = end;
            _builder.Clear();

            return new Token(kind, contents, start, end);
        }
        private char Peek(int ahead)
        {
            return _sourceCode[_index + ahead];
        }

        private void DoNewLine()
        {
            _line++;
            _column = 0;
        }

        private bool IsEOF() => _ch == '\0';

        private bool IsNewLine() => _ch == '\n';

        private bool IsWhiteSpace() => char.IsWhiteSpace(_ch) && !IsNewLine();

        private bool IsDigit() => char.IsDigit(_ch) || (_ch == '-' && char.IsDigit(_next));

        private bool IsLetter() => char.IsLetter(_ch);

        private bool IsLetterOrDigit() => char.IsLetterOrDigit(_ch);

        private bool IsIdentifier() => IsLetterOrDigit() || _ch == '_' || _ch == '-' || _ch == '?';

        private bool IsKeyword() => _Keywords.Contains(_builder.ToString());

        private bool IsPunctuation() => "<>{}()[]!*+-=/.;:\'`@~^&".Contains(_ch);


        private IEnumerable<Token> LexContents()
        {
            while (!IsEOF())
            {
                yield return LexToken();
            }

            yield return CreateToken(TokenKind.EndOfFile);
        }

        private Token LexToken()
        {
            if (IsEOF())
            {
                return CreateToken(TokenKind.EndOfFile);
            }
            else if (IsNewLine())
            {
                return ScanNewLine();
            }
            else if (IsWhiteSpace())
            {
                return ScanWhiteSpace();
            }
            else if (IsDigit())
            {
                return ScanInteger();
            }
            else if (_ch == ';') // Comment
            {
                return ScanComment();
            }
            else if (IsLetter() || _ch == '_') // Identifier
            {
                return ScanIdentifier();
            }
            else if (_ch == '"') // String
            {
                return ScanStringLiteral();
            }
            else if (_ch == '.' && char.IsDigit(_next)) // Float TODO: finish floats
            {
                return ScanFloat();
            }
            else if (IsPunctuation())
            {
                return ScanPunctuation();
            }
            else
            {
                return ScanWord();
            }
        }

        private Token ScanComment()
        {
            Consume();
            TokenKind commentTokenKind = TokenKind.InLineComment;
            if (_ch == ';')
            {
                commentTokenKind = TokenKind.LineComment; // Is Line comments, For debugging purpose 
            }

            while (!IsNewLine() && !IsEOF())
            {
                Consume();
            }

            return CreateToken(commentTokenKind);
        }

        private Token ScanFloat()
        {
            throw new NotImplementedException();
        }

        private Token ScanIdentifier()
        {
            while (IsIdentifier())
            {
                Consume();
            }

            if (!IsWhiteSpace() && !IsPunctuation() && !IsEOF())
            {
                return ScanWord();
            }

            if (IsKeyword())
            {
                return CreateToken(TokenKind.Keyword);
            }

            return CreateToken(TokenKind.Identifier);
        }
        private Token ScanInteger()
        {
            while (IsDigit())
            {
                Consume();
            }

            if (_ch == 'f' || _ch == '.' || _ch == 'e')
            {
                return ScanFloat();
            }

            if (!IsWhiteSpace() && !IsPunctuation() && !IsEOF())
            {
                return ScanWord();
            }

            return CreateToken(TokenKind.IntegerLiteral);
        }

        private Token ScanNewLine()
        {
            Consume();

            DoNewLine();

            return CreateToken(TokenKind.NewLine);
        }

        private Token ScanPunctuation()
        {
            switch (_ch)
            {
                case '\'':
                    Consume();
                    return CreateToken(TokenKind.Quote);

                case '`':
                    Consume();
                    return CreateToken(TokenKind.Quasiquote);

                case '~':
                    Consume();
                    if (_ch == '@')
                    {
                        Consume();
                        return CreateToken(TokenKind.SpliceUnquote);
                    }
                    return CreateToken(TokenKind.Unquote);

                case '^':
                    Consume();
                    return CreateToken(TokenKind.Meta);

                case '@':
                    Consume();
                    return CreateToken(TokenKind.Deref);

                case '(':
                    Consume();
                    return CreateToken(TokenKind.LeftParenthesis);

                case ')':
                    Consume();
                    return CreateToken(TokenKind.RightParenthesis);

                case '{':
                    Consume();
                    return CreateToken(TokenKind.LeftBracket);

                case '}':
                    Consume();
                    return CreateToken(TokenKind.RightBracket);

                case '[':
                    Consume();
                    return CreateToken(TokenKind.LeftBrace);

                case ']':
                    Consume();
                    return CreateToken(TokenKind.RightBrace);


                case '>':
                    Consume();
                    if (_ch == '=')
                    {
                        Consume();
                        return CreateToken(TokenKind.GreaterThanOrEqual);
                    }
                    return CreateToken(TokenKind.GreaterThan);

                case '<':
                    Consume();
                    if (_ch == '=')
                    {
                        Consume();
                        return CreateToken(TokenKind.LessThanOrEqual);
                    }
                    return CreateToken(TokenKind.LessThan);

                case '+':
                    Consume();
                    return CreateToken(TokenKind.Plus);

                case '-':
                    Consume();
                    return CreateToken(TokenKind.Minus);

                case '=':
                    Consume();
                    return CreateToken(TokenKind.Equal);

                case '!':
                    Consume();
                    return CreateToken(TokenKind.Not);

                case '*':
                    Consume();
                    return CreateToken(TokenKind.Mul);

                case '&':
                    Consume();
                    return CreateToken(TokenKind.Ampersand);

                case '/':
                    Consume();
                    if (_ch == '=')
                    {
                        Consume();
                        return CreateToken(TokenKind.NotEqual);
                    }
                    return CreateToken(TokenKind.Div);

                case '.':
                    Consume();
                    return CreateToken(TokenKind.Dot);

                case ':':
                    Consume();
                    return CreateToken(TokenKind.Colon);

                default: return ScanWord();
            }
        }

        private Token ScanStringLiteral()
        {
            Advance();

            while (_ch != '"')
            {
                if (IsEOF())
                {
                    AddError("Unexpected End Of File", Severity.Fatal);
                    return CreateToken(TokenKind.Error);
                }

                if (_ch == '\\' && _next == '"')
                {
                    Advance();
                }

                Consume();
            }

            Advance();

            return CreateToken(TokenKind.StringLiteral);
        }

        private Token ScanWhiteSpace()
        {
            while (IsWhiteSpace())
            {
                Consume();
            }
            return CreateToken(TokenKind.WhiteSpace);
        }

        private Token ScanWord(Severity severity = Severity.Error, string message = "Unexpected Token '{0}'")
        {
            while (!IsWhiteSpace() && !IsEOF() && !IsPunctuation())
            {
                Consume();
            }
            AddError(string.Format(message, _builder.ToString()), severity);
            return CreateToken(TokenKind.Error);
        }
    }
}
