﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace JsonCons.JsonPathLib
{
    enum ExprState
    {
        Start,
        ExpectFunctionExpr,
        PathLhs,
        PathRhs,
        FilterExpression,
        ExpressionRhs,
        RecursiveDescentOrPathLhs,
        PathOrLiteralOrFunction,
        JsonTextOrFunction,
        JsonTextOrFunctionName,
        JsonTextString,
        JsonValue,
        JsonString,
        IdentifierOrFunctionExpr,
        NameOrLeftBracket,
        UnquotedString,
        Number,
        FunctionExpression,
        Argument,
        ZeroOrOneArguments,
        OneOrMoreArguments,
        Identifier,
        SingleQuotedString,
        DoubleQuotedString,
        BracketedUnquotedNameOrUnion,
        UnionExpression,
        IdentifierOrUnion,
        BracketSpecifierOrUnion,
        Bracketed_wildcard,
        IndexOrSlice,
        WildcardOrUnion,
        UnionElement,
        IndexOrSliceOrUnion,
        Index,
        Integer,
        Digit,
        SliceExpressionStop,
        SliceExpressionStep,
        CommaOrRightBracket,
        ExpectRightBracket,
        QuotedStringEscapeChar,
        EscapeU1, 
        EscapeU2, 
        EscapeU3, 
        EscapeU4, 
        EscapeExpectSurrogatePair1, 
        EscapeExpectSurrogatePair2, 
        EscapeU5, 
        EscapeU6, 
        EscapeU7, 
        EscapeU8,
        Expression,
        ComparatorExpression,
        EqOrRegex,
        ExpectRegex,
        Regex,
        CmpLtOrLte,
        CmpGtOrGte,
        CmpNe,
        ExpectOr,
        ExpectAnd
    };

    class ExpressionCompiler 
    {
        string _input;
        int _index = 0;
        int _column = 1;
        int _line = 1;
        Stack<ExprState> _stateStack = new Stack<ExprState>();
        Stack<Token>_outputStack = new Stack<Token>();

        public ExpressionCompiler(string input)
        {
            _input = input;
        }

        public JsonPathExpression Compile()
        {
            _stateStack = new Stack<ExprState>();
            _index = 0;
            _column = 1;

            Stack<Int64> evalStack = new Stack<Int64>();
            evalStack.Push(0);

            _stateStack.Push(ExprState.Start);

            StringBuilder buffer = new StringBuilder();
            UInt32 cp = 0;
            UInt32 cp2 = 0;

            while (_index < _input.Length)
            {
                switch (_stateStack.Peek())
                {
                    case ExprState.Start: 
                    {
                        switch (_input[_index])
                        {
                            case ' ':case '\t':case '\r':case '\n':
                                SkipWhiteSpace();
                                break;
                            case '$':
                            {
                                PushToken(new Token(new RootSelector()));
                                _stateStack.Push(ExprState.PathRhs);
                                ++_index;
                                ++_column;
                                break;
                            }
                            default:
                            {
                                throw new JsonException("Invalid state");
                            }
                        }
                        break;
                    }
                    case ExprState.PathRhs: 
                    {
                        switch (_input[_index])
                        {
                            case ' ':case '\t':case '\r':case '\n':
                                SkipWhiteSpace();
                                break;
                            case '.':
                            {
                                _stateStack.Push(ExprState.RecursiveDescentOrPathLhs);
                                ++_index;
                                ++_column;
                                break;
                            }
                            default:
                            {
                                throw new JsonException("Invalid state");
                            }
                        }
                        break;
                    }
                    case ExprState.RecursiveDescentOrPathLhs:
                        switch (_input[_index])
                        {
                            case '.':
                                PushToken(new Token(new RecursiveDescentSelector()));
                                ++_index;
                                ++_column;
                                _stateStack.Pop();
                                _stateStack.Push(ExprState.NameOrLeftBracket);
                                break;
                            default:
                                _stateStack.Pop();
                                _stateStack.Push(ExprState.PathLhs);
                                break;
                        }
                        break;
                    case ExprState.PathLhs: 
                        switch (_input[_index])
                        {
                            case ' ':case '\t':case '\r':case '\n':
                                SkipWhiteSpace();
                                break;
                            case '*':
                                PushToken(new Token(new WildcardSelector()));
                                _stateStack.Pop();
                                ++_index;
                                ++_column;
                                break;
                            case '\'':
                                _stateStack.Pop(); _stateStack.Push(ExprState.Identifier);
                                _stateStack.Push(ExprState.SingleQuotedString);
                                ++_index;
                                ++_column;
                                break;
                            case '\"':
                                _stateStack.Pop(); _stateStack.Push(ExprState.Identifier);
                                _stateStack.Push(ExprState.DoubleQuotedString);
                                ++_index;
                                ++_column;
                                break;
                            case '[':
                                _stateStack.Push(ExprState.BracketSpecifierOrUnion);
                                ++_index;
                                ++_column;
                                break;
                            case '.':
                                throw new JsonException("Expected identifier");
                            default:
                                buffer.Clear();
                                _stateStack.Pop(); _stateStack.Push(ExprState.IdentifierOrFunctionExpr);
                                _stateStack.Push(ExprState.UnquotedString);
                                break;
                        }
                        break;
                    case ExprState.UnquotedString: 
                        switch (_input[_index])
                        {
                            case 'a':case 'b':case 'c':case 'd':case 'e':case 'f':case 'g':case 'h':case 'i':case 'j':case 'k':case 'l':case 'm':case 'n':case 'o':case 'p':case 'q':case 'r':case 's':case 't':case 'u':case 'v':case 'w':case 'x':case 'y':case 'z':
                            case 'A':case 'B':case 'C':case 'D':case 'E':case 'F':case 'G':case 'H':case 'I':case 'J':case 'K':case 'L':case 'M':case 'N':case 'O':case 'P':case 'Q':case 'R':case 'S':case 'T':case 'U':case 'V':case 'W':case 'X':case 'Y':case 'Z':
                            case '0':case '1':case '2':case '3':case '4':case '5':case '6':case '7':case '8':case '9':
                            case '_':
                                buffer.Append (_input[_index]);
                                ++_index;
                                ++_column;
                                break;
                            default:
                                _stateStack.Pop(); // UnquotedString
                                break;
                        };
                        break;                    
                    case ExprState.IdentifierOrFunctionExpr:
                    {
                        switch (_input[_index])
                        {
                            case ' ':case '\t':case '\r':case '\n':
                                SkipWhiteSpace();
                                break;
                            case '(':
                            {
                                /*
                                evalStack.Push(0);
                                auto f = resources.get_function(buffer);
                                if (ec)
                                {
                                    return pathExpression_type();
                                }
                                buffer.Clear();
                                PushToken(current_node_arg);
                                PushToken(new Token(f));
                                if (ec) {return pathExpression_type();}
                                _stateStack.Pop(); _stateStack.Push(ExprState.FunctionExpression);
                                _stateStack.Push(ExprState.ZeroOrOneArguments);
                                ++_index;
                                ++_column;
                                */
                                break;
                            }
                            default:
                            {
                                PushToken(new Token(new IdentifierSelector(buffer.ToString())));
                                buffer.Clear();
                                _stateStack.Pop(); 
                                break;
                            }
                        }
                        break;
                    }
                    case ExprState.SingleQuotedString:
                        switch (_input[_index])
                        {
                            case '\'':
                                _stateStack.Pop();
                                ++_index;
                                ++_column;
                                break;
                            case '\\':
                                _stateStack.Push(ExprState.QuotedStringEscapeChar);
                                ++_index;
                                ++_column;
                                break;
                            default:
                                buffer.Append (_input[_index]);
                                ++_index;
                                ++_column;
                                break;
                        };
                        break;
                    case ExprState.DoubleQuotedString: 
                        switch (_input[_index])
                        {
                            case '\"':
                                _stateStack.Pop();
                                ++_index;
                                ++_column;
                                break;
                            case '\\':
                                _stateStack.Push(ExprState.QuotedStringEscapeChar);
                                ++_index;
                                ++_column;
                                break;
                            default:
                                buffer.Append (_input[_index]);
                                ++_index;
                                ++_column;
                                break;
                        };
                        break;
                    case ExprState.QuotedStringEscapeChar:
                        switch (_input[_index])
                        {
                            case '\"':
                                buffer.Append('\"');
                                ++_index;
                                ++_column;
                                _stateStack.Pop();
                                break;
                            case '\'':
                                buffer.Append('\'');
                                ++_index;
                                ++_column;
                                _stateStack.Pop();
                                break;
                            case '\\': 
                                buffer.Append('\\');
                                ++_index;
                                ++_column;
                                _stateStack.Pop();
                                break;
                            case '/':
                                buffer.Append('/');
                                ++_index;
                                ++_column;
                                _stateStack.Pop();
                                break;
                            case 'b':
                                buffer.Append('\b');
                                ++_index;
                                ++_column;
                                _stateStack.Pop();
                                break;
                            case 'f':
                                buffer.Append('\f');
                                ++_index;
                                ++_column;
                                _stateStack.Pop();
                                break;
                            case 'n':
                                buffer.Append('\n');
                                ++_index;
                                ++_column;
                                _stateStack.Pop();
                                break;
                            case 'r':
                                buffer.Append('\r');
                                ++_index;
                                ++_column;
                                _stateStack.Pop();
                                break;
                            case 't':
                                buffer.Append('\t');
                                ++_index;
                                ++_column;
                                _stateStack.Pop();
                                break;
                            case 'u':
                                ++_index;
                                ++_column;
                                _stateStack.Pop(); _stateStack.Push(ExprState.EscapeU1);
                                break;
                            default:
                                throw new JsonException("Illegal escape character");
                        }
                        break;
                    case ExprState.EscapeU1:
                        cp = AppendToCodepoint(0, _input[_index]);
                        ++_index;
                        ++_column;
                        _stateStack.Pop(); _stateStack.Push(ExprState.EscapeU2);
                        break;
                    case ExprState.EscapeU2:
                        cp = AppendToCodepoint(cp, _input[_index]);
                        ++_index;
                        ++_column;
                        _stateStack.Pop(); _stateStack.Push(ExprState.EscapeU3);
                        break;
                    case ExprState.EscapeU3:
                        cp = AppendToCodepoint(cp, _input[_index]);
                        ++_index;
                        ++_column;
                        _stateStack.Pop(); _stateStack.Push(ExprState.EscapeU4);
                        break;
                    case ExprState.EscapeU4:
                        cp = AppendToCodepoint(cp, _input[_index]);
                        if (IsHighSurrogate(cp))
                        {
                            ++_index;
                            ++_column;
                            _stateStack.Pop(); _stateStack.Push(ExprState.EscapeExpectSurrogatePair1);
                        }
                        else
                        {
                            buffer.Append(Char.ConvertFromUtf32((int)cp));
                            ++_index;
                            ++_column;
                            _stateStack.Pop();
                        }
                        break;
                    case ExprState.EscapeExpectSurrogatePair1:
                        switch (_input[_index])
                        {
                            case '\\': 
                                ++_index;
                                ++_column;
                                _stateStack.Pop(); _stateStack.Push(ExprState.EscapeExpectSurrogatePair2);
                                break;
                            default:
                                throw new JsonException("Invalid codepoint");
                        }
                        break;
                    case ExprState.EscapeExpectSurrogatePair2:
                        switch (_input[_index])
                        {
                            case 'u': 
                                ++_index;
                                ++_column;
                                _stateStack.Pop(); _stateStack.Push(ExprState.EscapeU5);
                                break;
                            default:
                                throw new JsonException("Invalid codepoint");
                        }
                        break;
                    case ExprState.EscapeU5:
                        cp2 = AppendToCodepoint(0, _input[_index]);
                        ++_index;
                        ++_column;
                        _stateStack.Pop(); _stateStack.Push(ExprState.EscapeU6);
                        break;
                    case ExprState.EscapeU6:
                        cp2 = AppendToCodepoint(cp2, _input[_index]);
                        ++_index;
                        ++_column;
                        _stateStack.Pop(); _stateStack.Push(ExprState.EscapeU7);
                        break;
                    case ExprState.EscapeU7:
                        cp2 = AppendToCodepoint(cp2, _input[_index]);
                        ++_index;
                        ++_column;
                        _stateStack.Pop(); _stateStack.Push(ExprState.EscapeU8);
                        break;
                    case ExprState.EscapeU8:
                    {
                        cp2 = AppendToCodepoint(cp2, _input[_index]);
                        UInt32 codepoint = 0x10000 + ((cp & 0x3FF) << 10) + (cp2 & 0x3FF);
                        buffer.Append(Char.ConvertFromUtf32((int)codepoint));
                        _stateStack.Pop();
                        ++_index;
                        ++_column;
                        break;
                    }
                    default:
                        ++_index;
                        break;
                    }
            }

            if (_stateStack.Count == 0)
            {
                throw new JsonException("Syntax error");
            }
            if (_stateStack.Peek() == ExprState.Start)
            {
                throw new JsonException("Unexpected end of input");
            }
            if (_stateStack.Count >= 3)
            {
                if (_stateStack.Peek() == ExprState.UnquotedString || _stateStack.Peek() == ExprState.Identifier)
                {
                    PushToken(new Token(new IdentifierSelector(buffer.ToString())));
                    _stateStack.Pop(); // UnquotedString
                    buffer.Clear();
                    if (_stateStack.Peek() == ExprState.IdentifierOrFunctionExpr)
                    {
                        _stateStack.Pop(); // identifier
                    }
                }
                else if (_stateStack.Peek() == ExprState.Digit)
                {
                    if (buffer.Length == 0)
                    {
                        throw new JsonException("Invalid number");
                    }
                    Int64 n;
                    if (!Int64.TryParse(buffer.ToString(), out n))
                    {
                        throw new JsonException("Invalid number");
                    }
                    //PushToken(new Token(jsoncons::make_unique<index_selector>(n)));
                    buffer.Clear();
                    _stateStack.Pop(); // IndexOrSliceOrUnion
                    if (_stateStack.Peek() == ExprState.Index)
                    {
                        _stateStack.Pop(); // index
                    }
                }
            }


            Token token = _outputStack.Pop();
            if (_outputStack.Count != 0)
            {
                throw new JsonException("Invalid state");
            }
            if (token.Type != TokenType.Selector)
            {
                throw new JsonException("Invalid state");
            }
            return new JsonPathExpression(token.GetSelector());
        }

        private void PushToken(Token token)
        {
            switch (token.Type)
            {
                case TokenType.Selector:
                    if (_outputStack.Count != 0 && _outputStack.Peek().Type == TokenType.Selector)
                    {
                        _outputStack.Peek().GetSelector().AppendSelector(token.GetSelector());
                    }
                    else
                    {
                        _outputStack.Push(token);
                    }
                    break;
            }
        }

        bool IsHighSurrogate(UInt32 ch) 
        {
            return (ch >= 0xD800 && ch <= 0xDBFF);
        }

        private UInt32 AppendToCodepoint(UInt32 cp, uint c)
        {
            cp *= 16;
            if (c >= '0'  &&  c <= '9')
            {
                cp += c - '0';
            }
            else if (c >= 'a'  &&  c <= 'f')
            {
                cp += c - 'a' + 10;
            }
            else if (c >= 'A'  &&  c <= 'F')
            {
                cp += c - 'A' + 10;
            }
            else
            {
                throw new JsonException("Invalid codepoint");
            }
            return cp;
        }

        private void SkipWhiteSpace()
        {
            switch (_input[_index])
            {
                case ' ':case '\t':
                    ++_index;
                    ++_column;
                    break;
                case '\r':
                    if (_index+1 < _input.Length && _input[_index+1] == '\n')
                        ++_index;
                    ++_line;
                    _column = 1;
                    ++_index;
                    break;
                case '\n':
                    ++_line;
                    _column = 1;
                    ++_index;
                    break;
                default:
                    break;
            }
        }
    };

    public class JsonPathExpression
    {
        ISelector _selector;

        internal JsonPathExpression(ISelector selector)
        {
            _selector = selector;
        }

        public IReadOnlyList<JsonElement> Evaluate(JsonElement root)
        {
            var nodes = new List<JsonElement>();
            _selector.Select(root, root, nodes);
            return nodes;
        }

        public static JsonPathExpression Compile(string expr)
        {

            var compiler = new ExpressionCompiler(expr);
            return compiler.Compile();
        }
    }

} // namespace JsonCons.JsonPathLib
