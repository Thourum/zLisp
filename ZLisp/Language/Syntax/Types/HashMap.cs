using System.Collections.Generic;

namespace ZLisp.Language.Syntax
{
    public sealed partial class Types
    {
        public class HashMap : SyntaxNode
        {
            public override SyntaxKind Kind => SyntaxKind.HashMap;

            public Dictionary<string, Value> Contents { get; set; }

            public HashMap(Dictionary<string, Value> val, SourceSpan span = null) : base(span)
            {
                Contents = val;
            }

            public HashMap(Expression lst, SourceSpan span = null) : base(span)
            {
                Contents = new Dictionary<string, Value>();
                AssocBANG(lst);
            }

            public new HashMap Copy()
            {
                var new_self = (HashMap)this.MemberwiseClone();
                new_self.Contents = new Dictionary<string, Value>(Contents);
                return new_self;
            }

            public Dictionary<string, Value> GetValue() => Contents;

            public override string ToString()
            {
                List<string> strs = new List<string>();
                foreach (KeyValuePair<string, Value> entry in Contents)
                {
                    if (entry.Key.Length > 0 && entry.Key[0] == '\u029e')
                    {
                        strs.Add(":" + entry.Key[1..]);
                    }
                    else
                    {
                        strs.Add("\"" + entry.Key.ToString() + "\"");
                    }
                    strs.Add(entry.Value.ToString());
                }
                return string.Join("", strs.ToArray());
            }

            public HashMap AssocBANG(Expression lst)
            {
                for (int i = 0; i < lst.Size(); i += 2)
                {
                    Contents[((String)lst[i]).Value] = lst[i + 1];
                }
                return this;
            }

            public HashMap DissocBANG(Expression lst)
            {
                for (int i = 0; i < lst.Size(); i++)
                {
                    Contents.Remove(((String)lst[i]).Value);
                }
                return this;
            }
        }
    }
}