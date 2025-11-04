namespace Markdown;

public class Token
{
    public string Value { get; }
    public TokenType Type { get;  }
    public int Position { get;  }

    public Token(string value, TokenType type, int length, int position)
    {
        Value = value;
        Type = type;
        Position = position;
    }
}