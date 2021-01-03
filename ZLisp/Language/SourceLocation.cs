namespace ZLisp.Language
{
    public class SourceLocation
    {
        private readonly int _column;
        private readonly int _index;
        private readonly int _line;

        public int Column => _column;

        public int Index => _index;

        public int Line => _line;

        public SourceLocation(int index, int line, int column)
        {
            _index = index;
            _line = line;
            _column = column;
        }
    }
}
