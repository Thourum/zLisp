using System;

namespace ZLisp.Language
{
    public sealed class Token : IEquatable<Token>
    {
        private Lazy<TokenCatagory> _catagory;

        public TokenCatagory Catagory => _catagory.Value;

        public TokenKind Kind { get; }

        public SourceSpan Span { get; }

        public string Value { get; }

        public Token(TokenKind kind, string contents, SourceLocation start, SourceLocation end)
        {
            Kind = kind;
            Value = contents;
            Span = new SourceSpan(start, end);

            _catagory = new Lazy<TokenCatagory>(GetTokenCatagory);
        }

        public static bool operator !=(Token left, string right) => left?.Value != right;

        public static bool operator !=(string left, Token right) => right?.Value != left;

        public static bool operator !=(Token left, TokenKind right) => left?.Kind != right;

        public static bool operator !=(TokenKind left, Token right) => right?.Kind != left;

        public static bool operator ==(Token left, string right) => left?.Value == right;

        public static bool operator ==(string left, Token right) => right?.Value == left;

        public static bool operator ==(Token left, TokenKind right) => left?.Kind == right;

        public static bool operator ==(TokenKind left, Token right) => right?.Kind == left;

        public override bool Equals(object obj)
        {
            if (obj is Token token)
            {
                return Equals(token);
            }
            return base.Equals(obj);
        }

        public bool Equals(Token other)
        {
            if (other == null)
            {
                return false;
            }
            return other.Value == Value &&
                   other.Span == Span &&
                   other.Kind == Kind;
        }
        public override int GetHashCode() => Value.GetHashCode() ^ Span.GetHashCode() ^ Kind.GetHashCode();

        public override string ToString() => $"K:{Kind,-18}C:{Catagory,-12}Value:'{Value}'";

        public bool IsTrivia() => Catagory == TokenCatagory.WhiteSpace || Catagory == TokenCatagory.Comment;

        private TokenCatagory GetTokenCatagory()
        {
            switch (Kind)
            {
                case TokenKind.Colon:
                case TokenKind.Semicolon:
                case TokenKind.Dot:
                    return TokenCatagory.Punctuation;

                case TokenKind.GreaterThanOrEqual:
                case TokenKind.LessThanOrEqual:
                case TokenKind.NotEqual:
                case TokenKind.Deref:
                case TokenKind.Meta:
                case TokenKind.Unquote:
                case TokenKind.Quasiquote:
                case TokenKind.Quote:
                case TokenKind.Equal:
                case TokenKind.Not:
                case TokenKind.LessThan:
                case TokenKind.GreaterThan:
                case TokenKind.Minus:
                case TokenKind.Mul:
                case TokenKind.Plus:
                case TokenKind.Div:
                    return TokenCatagory.Operator;

                case TokenKind.InLineComment:
                case TokenKind.LineComment:
                    return TokenCatagory.Comment;

                case TokenKind.NewLine:
                case TokenKind.WhiteSpace:
                    return TokenCatagory.WhiteSpace;

                case TokenKind.LeftBrace:
                case TokenKind.LeftBracket:
                case TokenKind.LeftParenthesis:
                case TokenKind.RightBrace:
                case TokenKind.RightBracket:
                case TokenKind.RightParenthesis:
                    return TokenCatagory.Grouping;

                case TokenKind.Identifier:
                case TokenKind.Keyword:
                    return TokenCatagory.Identifier;

                case TokenKind.StringLiteral:
                case TokenKind.IntegerLiteral:
                case TokenKind.FloatLiteral:
                    return TokenCatagory.Constant;

                case TokenKind.EndOfFile:
                    return TokenCatagory.EOF;

                case TokenKind.Error:
                    return TokenCatagory.Invalid;

                default: return TokenCatagory.Unknown;
            }
        }
    }
}
