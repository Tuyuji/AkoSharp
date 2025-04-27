using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AkoSharp.Gen;

public ref struct Parser
{
    private readonly ReadOnlySpan<Token> _span;
    private int _index = 0;

    public Parser(ReadOnlySpan<Token> span)
    {
        _span = span;
    }

    private Token Consume()
    {
        if (_index >= _span.Length)
        {
            return Token.Null;
        }
        
        return _span[_index++];
    }

    private Token Peek(int offset = 0)
    {
        if (_index + offset >= _span.Length)
        {
            return Token.Null;
        }
        
        return _span[_index + offset];
    }

    private bool CheckPeekType(TokenType type, int offset = 0)
    {
        var peeked = Peek(offset);
        if (peeked.IsNull)
        {
            return false;
        }
        
        return peeked.Type == type;
    }

    private AVar ParseValue()
    {
        var peeked = Peek();
        if (peeked.IsNull)
        {
            return null;
        }
        
        switch (peeked.Type)
        {
            case TokenType.OpenDoubleBrace:
                return ParseArray();
            case TokenType.OpenBrace:
                return ParseTable();
            case TokenType.Semicolon:
                Consume();
                return new AkoNull();
            case TokenType.Bool:
                return new ABool(Consume().ValueBool);
            case TokenType.String:
                return new AString(Consume().ValueString);
            case TokenType.And:
                if (Peek(1) is not { Type: TokenType.Identifier })
                {
                    throw new Exception($"Expected identifier after & at {Peek().Meta.ToString(false)}");
                }

                Consume();
                StringBuilder sb = new();

                while (Peek() is { Type: TokenType.Identifier })
                {
                    sb.Append(Consume().ValueString);
                    if (Peek() is not { Type: TokenType.Dot })
                    {
                        break;
                    }

                    Consume();
                    sb.Append('.');
                }

                return new AShortType(ShortTypeRegistry.GetTypeFromShortType(sb.ToString()));
            case TokenType.Int:
            case TokenType.Float:
            {
                var startLoc = peeked.Meta;
                if (Peek(1) is {Type: TokenType.VectorSeparator})
                {
                    List<float> numbers = new List<float>();
                    while (!Peek().IsNull)
                    {
                        if (!(Peek().Type == TokenType.Int || Peek().Type == TokenType.Float))
                        {
                            throw new FormatException("Expected int or float");
                        }

                        var value = Consume();
                        if(value.Type == TokenType.Int)
                            numbers.Add(value.ValueInt);
                        else
                            numbers.Add(value.ValueFloat);
                        
                        if (Peek() is { Type: TokenType.VectorSeparator })
                        {
                            Consume();
                        }
                        else
                        {
                            //no continue to vector, return what we have
                            if (numbers.Count > 4)
                            {
                                throw new FormatException(
                                    $"Vector size is greater than 4 at " +
                                    $"{startLoc.ToString(false)}-{Peek(-1).Meta.ToString(false)}");
                            }

                            switch (numbers.Count)
                            {
                                case 2:
                                    return new AVector(new Vector2(numbers[0], numbers[1]));
                                case 3:
                                    return new AVector(new Vector3(numbers[0], numbers[1], numbers[2]));
                                case 4:
                                    return new AVector(new Vector4(numbers[0], numbers[1], numbers[2], numbers[3]));
                                default:
                                    throw new Exception("Unexpected count");
                            }
                        }
                    }
                }

                if (peeked.Type == TokenType.Int)
                {
                    return new AInt(Consume().ValueInt);
                }
                else
                {
                    return new AFloat(Consume().ValueFloat);
                }
                
                throw new Exception("Unexpected token type");
            }
            default:
                throw new InvalidOperationException($"Unexpected token type: {peeked.Type} at {peeked.Meta.ToString()}");
        }
    }

    private AArray ParseArray()
    {
        if (Peek() is { Type: TokenType.OpenDoubleBrace })
        {
            Consume();
        }
        else
        {
            throw new Exception($"Expected open double brace at {Peek().Meta.ToString(false)}" );
        }

        var array = new AArray();

        while (Peek() is not { Type: TokenType.CloseDoubleBrace })
        {
            array.Add(ParseValue());
        }
        
        if (Peek() is { Type: TokenType.CloseDoubleBrace })
        {
            Consume();
            return array;
        }
        
        throw new Exception($"Expected close double brace at {Peek().Meta.ToString(false)}");
    }

    private ATable ParseTable(bool ignoreBraces = false)
    {
        if (!ignoreBraces)
        {
            if (Peek() is { Type: TokenType.OpenBrace })
            {
                Consume();
            }
            else
            {
                throw new Exception("Expected open brace.");
            }
        }

        var table = new ATable();

        while (!Peek().IsNull && Peek() is not { Type: TokenType.CloseBrace })
        {
            if (Peek().IsNull || Peek(1).IsNull)
            {
                throw new Exception("Expected two tokens, got only one or non");
            }

            var firstPeekType = Peek().Type;
            if (
                !(
                    firstPeekType == TokenType.Identifier ||
                    firstPeekType == TokenType.String ||
                    firstPeekType == TokenType.Bool ||
                    firstPeekType == TokenType.Semicolon
                ))
            {
                throw new Exception("Expected an identifier, bool or null");
            }

            ParseTableElement(table);
        }

        if (!ignoreBraces)
        {
            if (Peek() is { Type: TokenType.CloseBrace })
            {
                Consume();
                return table;
            }

            throw new Exception("Expected a close brace.");
        }

        return table;
    }

    private void ParseTableElement(ATable table)
    {
        if (Peek().IsNull)
        {
            throw new Exception("Expected a token, got non");
        }

        Token? valueFirst = null;
        if (Peek() is { Type: TokenType.Bool } || Peek() is { Type: TokenType.Semicolon })
        {
            valueFirst = Consume();
        }

        if (
            !(Peek().Type == TokenType.Identifier ||
              Peek().Type == TokenType.String)
        )
        {
            throw new Exception("Expected an identifier or string");
        }
        
        var currentTable = table;
        var CTValueId = (string)null;

        while (!Peek().IsNull && (Peek().Type == TokenType.Identifier || Peek().Type == TokenType.String))
        {
            var id = Consume().ValueString;
            var stillMore = CheckPeekType(TokenType.Dot);

            if (!stillMore)
            {
                if (!(currentTable.ContainsKey(id)))
                {
                    currentTable[id] = null;
                }

                CTValueId = id;
                break;
            }
            else
            {
                if (!(currentTable.ContainsKey(id)))
                {
                    currentTable[id] = new ATable();
                }
                currentTable = currentTable[id] as ATable;
            }

            if (Peek() is { Type: TokenType.Dot })
            {
                Consume();
            }
        }

        if (CTValueId == null)
        {
            throw new Exception("Failed to get table id");
        }

        if (valueFirst != null)
        {
            //value is first
            currentTable[CTValueId] = valueFirst.Value.Type switch
            {
                TokenType.Bool => new ABool(valueFirst.Value.ValueBool),
                TokenType.Semicolon => new AkoNull(),
                _ => throw new Exception("Expected a bool or null")
            };
        }
        else
        {
            currentTable[CTValueId] = ParseValue();
        }
    }

    public AVar Parse()
    {
        if (Peek().IsNull)
        {
            //Nothing
            return null;
        }

        if (Peek() is { Type: TokenType.OpenDoubleBrace })
        {
            return ParseArray();
        }
        else
        {
            bool shouldIgnoreBraces = !(Peek() is { Type: TokenType.OpenBrace });
            return ParseTable(shouldIgnoreBraces);
        }
    }
}