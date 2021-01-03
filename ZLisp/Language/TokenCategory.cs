namespace ZLisp.Language
{
    public enum TokenCatagory
    {
        Unknown,
        WhiteSpace,
        Comment,

        Constant,
        Identifier,
        Grouping,
        Punctuation,
        Operator,

        EOF,
        Invalid,
    }
}
