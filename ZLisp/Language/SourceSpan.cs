namespace ZLisp.Language
{
    public class SourceSpan
    {
        private readonly SourceLocation _end;
        private readonly SourceLocation _start;

        public SourceLocation End => _end;

        public int Length => _end.Index - _start.Index;

        public SourceLocation Start => _start;

        public SourceSpan(SourceLocation start, SourceLocation end)
        {
            _start = start;
            _end = end;
        }
    }
}
