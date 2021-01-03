namespace ZLisp.Language
{
    public enum TokenKind
    {
        EndOfFile,
        Error,

        #region WhiteSpace

        WhiteSpace,
        NewLine,

        #endregion WhiteSpace

        #region Comments

        LineComment,
        InLineComment,

        #endregion Comments

        #region Constants

        IntegerLiteral,
        StringLiteral,
        FloatLiteral,

        #endregion Constants

        #region Identifiers

        Identifier,
        Keyword,

        #endregion Identifiers

        #region Groupings

        LeftBracket, // {
        RightBracket, // }
        RightBrace, // ]
        LeftBrace, // [
        LeftParenthesis, // (
        RightParenthesis, // )

        #endregion Groupings

        #region Operators

        GreaterThanOrEqual, // >=
        GreaterThan, // >
        LessThan, // <
        LessThanOrEqual, // <=
        Plus, // +
        Minus, // -
        Mul, // *
        Not, // !
        Div, // /
        NotEqual, // /=
        Equal, // ==

        Deref, // @
        Meta,  // ^
        Unquote, // ~
        SpliceUnquote, // ~@
        Quasiquote, // `
        Quote, // '
        Ampersand, // &

        #endregion Operators


        #region Punctuation
        Dot,
        Semicolon,
        Colon,

        #endregion Punctuation
    }
}
