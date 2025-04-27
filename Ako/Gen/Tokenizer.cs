using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkoSharp.Gen;

public ref struct Tokenizer
{
    private readonly ReadOnlySpan<char> _span;
    private int _index = 0;
    private int _line = 1;
    private int _column = 1;
    private TokenMeta _meta = new();
    
    private List<Token> _tokens = new();
    private StringBuilder _builder = new();

    public Tokenizer(ReadOnlySpan<char> span)
    {
        _span = span;
    }

    private char Consume()
    {
        if (_index >= _span.Length)
            return '\0';

        var next = _span[_index++];
        if (next == '\n')
        {
            _line++;
            _column = 1;
        }else if (next == '\t')
        {
            _column++;
        }
        else
        {
            _column++;
        }

        return next;
    }

    private char? Peek(int offset = 0)
    {
        if (_index + offset >= _span.Length)
            return null;
        
        return _span[_index + offset];
    }
    
    private void StartMeta()
    {
        _meta.StartLine = _line;
        _meta.StartColumn = _column;
        _meta.StartIndex = _index;
    }

    private void AddToken(TokenType type)
    {
        _tokens.Add(new Token(type, _meta.Copy()));
    }
    
    private void AddToken(TokenType type, int value)
    {
        _tokens.Add(new Token(type, _meta.Copy()){ValueInt = value});
    }
    
    private void AddToken(TokenType type, float value)
    {
        _tokens.Add(new Token(type, _meta.Copy()){ValueFloat = value});
    }
    
    private void AddToken(TokenType type, string value)
    {
        _tokens.Add(new Token(type, _meta.Copy()){ValueString = value});
    }
    
    private void AddToken(TokenType type, bool value)
    {
        _tokens.Add(new Token(type, _meta.Copy()){ValueBool = value});
    }

    private string GetPositionString()
    {
        return $"{_line}:{_column}";
    }

    private bool ParseDigit()
    {
        var c = Peek();
        _builder.Clear();
        
        if (c.HasValue && (c == '+' || c == '-'))
        {
            _builder.Append(Consume());
            c = Peek();
        }
        
        if (!c.HasValue)
        {
            throw new Exception("No more characters in position");
            return false;
        }

        if (char.IsDigit(c.Value))
        {
            while (Peek().HasValue && char.IsDigit(Peek().Value))
            {
                _builder.Append(Consume());
            }

            if (Peek().HasValue && Peek() == '.')
            {
                _builder.Append(Consume());
                while (Peek().HasValue && char.IsDigit(Peek().Value))
                {
                    _builder.Append(Consume());
                }
                AddToken(TokenType.Float, float.Parse(_builder.ToString()));
                return true;
            }
            else
            {
                AddToken(TokenType.Int, int.Parse(_builder.ToString()));
                return true;
            }
        }

        return false;
    }
    
    public Token[] Tokenize()
    {
        if(_span.IsEmpty)
            return Array.Empty<Token>();
        
        _tokens.Clear();
        _builder.Clear();
        
        char? lookahead = null;

        while (Peek().HasValue)
        {
            lookahead = null;
            var c = Peek().Value;
            if (c == ' ' || c == '\n' || c == '\t')
            {
                Consume();
                continue;
            }

            if (c == '#')
            {
                //Comment skip until new line
                Consume();
                var commentLine = _line;
                while (_line == commentLine)
                {
                    if (Consume() == '\0')
                    {
                        //no more text
                        break;
                    }
                }
                continue;
            }
            
            StartMeta();

            switch (c)
            {
                case '+':
                case '-':
                    lookahead = Peek(1);
                    if (lookahead.HasValue && char.IsDigit(lookahead.Value))
                    {
                        break;
                    }

                    Consume();
                    AddToken(TokenType.Bool, c == '+');
                    continue;
                case ';':
                    Consume();
                    AddToken(TokenType.Semicolon);
                    continue;
                case '.':
                    Consume();
                    AddToken(TokenType.Dot);
                    continue;
                case '&':
                    Consume();
                    AddToken(TokenType.And);
                    continue;
                case '[':
                    Consume();
                    lookahead = Peek();
                    if (lookahead.HasValue && lookahead.Value == '[')
                    {
                        Consume();
                        AddToken(TokenType.OpenDoubleBrace);
                        continue;
                    }
                    AddToken(TokenType.OpenBrace);
                    continue;
                case ']':
                    Consume();
                    lookahead = Peek();
                    if (lookahead.HasValue && lookahead.Value == ']')
                    {
                        Consume();
                        AddToken(TokenType.CloseDoubleBrace);
                        continue;
                    }
                    AddToken(TokenType.CloseBrace);
                    continue;
            }

            if (char.IsLetter(c) || c == '_')
            {
                _builder.Clear();
                lookahead = Peek();
                while (lookahead.HasValue && lookahead != '.' && (char.IsLetterOrDigit(lookahead.Value) || lookahead == '_'))
                {
                    _builder.Append(Consume());
                    lookahead = Peek();
                }
                AddToken(TokenType.Identifier, _builder.ToString());
                continue;
            }

            if (ParseDigit())
            {
                lookahead = Peek();
                if (lookahead is 'x')
                {
                    //vector mode
                    while (lookahead is 'x')
                    {
                        //Consume the x
                        var vectorDelimiterLine = _line;
                        var vectorDelimiterColumn = _column;
                        Consume();
                        AddToken(TokenType.VectorSeparator);

                        if (!ParseDigit())
                        {
                            //Failed to parse digit and we did have an X before, invalid Ako
                            throw new Exception($"Expected a number after vector delimiter at {GetPositionString()}");
                        }

                        lookahead = Peek();
                    }
                    continue;
                }
                else
                {
                    //normal digit
                    continue;
                }
            }

            if (c == '"')
            {
                Consume();
                _builder.Clear();
                lookahead = Peek();
                while (lookahead.HasValue && lookahead.Value != '"')
                {
                    if (lookahead.Value == '\\')
                    {
                        Consume();
                        if (!Peek().HasValue)
                        {
                            //nothing
                            _builder.Append("\\");
                        }
                        else
                        {
                            switch (Peek())
                            {
                                case 'n':
                                    _builder.Append("\n");
                                    break;
                                default:
                                    _builder.Append(Peek());
                                    break;
                            }

                            Consume();
                            lookahead = Peek();
                        }
                    }
                    else
                    {
                        _builder.Append(Consume());
                        lookahead = Peek();
                    }
                }

                Consume();
                AddToken(TokenType.String, _builder.ToString());
                continue;
            }
            
            throw new Exception($"Unexpected character '{c}' at position {GetPositionString()}");
        }
        
        return _tokens.ToArray();
    }
}