using System.Collections;
using System.Collections.Generic;

namespace ZLisp.Language.Error
{
    public partial class ErrorSink : IEnumerable<ErrorEntry>
    {
        private List<ErrorEntry> _errors;

        public IEnumerable<ErrorEntry> Errors => _errors.AsReadOnly();

        public bool HasErrors => _errors.Count > 0;

        public ErrorSink()
        {
            _errors = new List<ErrorEntry>();
        }

        public void AddError(string message, SourceCode sourceCode, Severity severity, SourceSpan span) =>
            _errors.Add(new ErrorEntry(message, sourceCode.GetLines(span.Start.Line, span.End.Line), severity, span));

        public void Clear() => _errors.Clear();

        public IEnumerator<ErrorEntry> GetEnumerator() => _errors.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _errors.GetEnumerator();

        public void ConsolePrint() => _errors.ForEach(e => e.ConsolePrint());
    }

}
