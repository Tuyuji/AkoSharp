namespace AkoSharp.Gen;

public enum TokenType
{
    None,
    Bool,
    Int,
    Float,
    String,
    Identifier,
    Dot,
    VectorSeparator,
    Semicolon,
    And,
    Plus,
    Minus,
    OpenBrace,
    CloseBrace,
    OpenDoubleBrace,
    CloseDoubleBrace
}

public struct TokenMeta
{
    public int StartLine;
    public int StartColumn;
    public int EndLine;
    public int EndColumn;
    public int StartIndex;
    public int EndIndex;
    public override string ToString()
    {
        return ToString(true);
    }

    public string ToString(bool includeEnd)
    {
        if (includeEnd)
        {
            return $"{StartLine}-{StartColumn}:{EndLine}-{EndColumn}";
        }
        else
        {
            return $"{StartLine}-{StartColumn}";
        }
    }

    public TokenMeta Copy()
    {
        return new TokenMeta()
        {
            StartLine = StartLine,
            StartColumn = StartColumn,
            StartIndex = StartIndex,
            EndLine = EndLine,
            EndColumn = EndColumn,
            EndIndex = EndIndex
        };
    }

    public static TokenMeta NullMeta = new TokenMeta()
    {
        StartLine = 0 //StartLine cant be 0, so anyone trying to find a null meta, just look for this.
    };
}

public struct Token
{
    public static Token Null = new Token(TokenType.None, TokenMeta.NullMeta);
    
    public TokenType Type;
    public TokenMeta Meta;

    public int ValueInt;
    public float ValueFloat;
    public string ValueString;
    public bool ValueBool;
    
    public bool IsNull => Type == TokenType.None;

    public Token(TokenType type, TokenMeta meta)
    {
        Type = type;
        Meta = meta;
        ValueInt = 0;
        ValueFloat = 0;
        ValueString = string.Empty;
        ValueBool = false;
    }
    
    public override string ToString()
    {
        return $"{Type}: {Meta.StartLine}:{Meta.StartColumn} - {Meta.StartIndex}";
    }
};