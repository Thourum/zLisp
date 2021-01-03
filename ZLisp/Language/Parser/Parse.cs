using System;
using System.Collections.Generic;
using ZLisp.Language.Syntax;
using static ZLisp.Language.Syntax.Types;

namespace ZLisp.Language.Parser
{
    public sealed partial class Parser
    {
        internal SyntaxNode ParseInternal()
        {
            switch (_current.Kind)
            {
                case TokenKind.LeftParenthesis:
                    return ParseExpression();
                case TokenKind.LeftBrace:
                    return ParseVector();
                case TokenKind.LeftBracket:
                    return ParseHashMap();

                case TokenKind.Ampersand:
                    var ampersand = Take();
                    return new Symbol(ampersand.Kind.ToString().ToLower(), CreateSpan(ampersand));

                case TokenKind.Quote:
                case TokenKind.Quasiquote:
                case TokenKind.Unquote:
                case TokenKind.SpliceUnquote:
                case TokenKind.Deref:
                    var symbol = Take();
                    var nextSyntaxNode = ParseInternal();
                    var list = new List<Value>() 
                    { 
                        new Symbol(symbol.Kind.ToString().ToLower(), symbol.Span),
                        nextSyntaxNode
                    };
                    return new Expression(list, CreateSpan(symbol));

                case TokenKind.RightParenthesis:
                case TokenKind.RightBrace:
                case TokenKind.RightBracket:
                    throw new SyntaxException($"unexpected '{_current.Kind}'");
                default:
                    return ParseAtom();
            }
        }

        public SyntaxNode ParseAtom()
        {
            switch (_current.Kind)
            {
                case TokenKind.StringLiteral:
                    var str = Take();
                    return new Types.String(str.Value, CreateSpan(str));
                case TokenKind.IntegerLiteral:
                    var num = Take();
                    return new Integer(Int64.Parse(num.Value), CreateSpan(num));
                case TokenKind.FloatLiteral:
                    throw new NotImplementedException();
                //var flt = Take();
                //return new Float(float.Parse(flt.Value), flt.Span);

                case TokenKind.Keyword:
                    if (_current == "true" || _current == "false" || _current == "nil")
                    {
                        var Const = Take();
                        return new Constant(Const.Value, CreateSpan(Const));
                    }
                    else
                    {
                        var keyword = Take();
                        return new Symbol(keyword.Value, CreateSpan(keyword));
                    }

                case TokenKind.Colon:
                    var colon = Take();
                    var identifier = Take(TokenKind.Identifier);
                    return new Types.String(colon.Value + identifier.Value, CreateSpan(colon));

                case TokenKind.Identifier:
                case TokenKind.GreaterThanOrEqual:
                case TokenKind.GreaterThan:
                case TokenKind.LessThan:
                case TokenKind.LessThanOrEqual:
                case TokenKind.Plus:
                case TokenKind.Minus:
                case TokenKind.Mul:
                case TokenKind.Not:
                case TokenKind.Div:
                case TokenKind.NotEqual:
                case TokenKind.Equal:
                    goto symbol;

                case TokenKind.LineComment:
                case TokenKind.InLineComment:
                    var atom = Take();
                    return new Comment(atom.Value, CreateSpan(atom));

                symbol:
                    var symbol = Take();
                    return new Symbol(symbol.Value, CreateSpan(symbol));

                default:
                    throw UnexpectedToken("Atom");
            }
        }

        private HashMap ParseHashMap()
        {
            var l = ParseList(TokenKind.LeftBracket, TokenKind.RightBracket);
            var dic = new Dictionary<string, Value>();
            for (int i = 0; i < l.Content.Count; i += 2)
            {
                dic[((Types.String)l.Content[i]).Value] = l.Content[i + 1];
            }
            return new HashMap(dic, l.Span);
        }
        private Vector ParseVector()
        {
            var l = ParseList(TokenKind.LeftBrace, TokenKind.RightBrace);
            return new Vector(l.Content, l.Span);
        }
        private Expression ParseExpression()
        {
            var l = ParseList(TokenKind.LeftParenthesis, TokenKind.RightParenthesis);
            return new Expression(l.Content, l.Span);
        }

        private (List<Value> Content, SourceSpan Span) ParseList(TokenKind openKind, TokenKind closeKind)
        {
            List<Value> contents = new List<Value>();
            var start = _current;
            TakeScope(() => contents.Add(ParseInternal()), openKind, closeKind);
            return (contents, CreateSpan(start));
        }
    }
}
