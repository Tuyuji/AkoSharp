using System;
using System.Collections.Generic;

namespace Ako
{
    enum TokenType
    {
        None,
        Identifier,
        IntLitereral,
        FloatLiteral,
        BooleanLiteral,
        StringLitereral,
        Operator,
    }
    
    class Token
    {
        public TokenType Type = TokenType.None;
        public string Text = String.Empty;
        public UInt32 LineNumber = 0;
    }

    class AkoTokenizer
    {
        private enum State
        {
            None,
            Comment,
            Number,
            String,
            GetIdentifier,
            PotentialArray,
            StringEscape
        };

        private class Pair<T, U>
        {
            public Pair()
            {
            }
            
            public Pair(T first, U second)
            {
                First = first;
                Second = second;
            }
            
            public T First { get; set; }
            public U Second { get; set; }
        }
        
        private static void EndToken(ref Pair<State, Token> token, ref List<Token> tokens, bool ignoreState = false)
        {
            if (!ignoreState && token.First != State.None && token.Second.Text != String.Empty)
            {
                tokens.Add(token.Second);
                uint lineNumber = token.Second.LineNumber;
                token.Second = new Token
                {
                    LineNumber = lineNumber
                };
            }

            token.First = State.None;
            token.Second.Type = TokenType.None;
            token.Second.Text = string.Empty;
        }

        public static Token[] Parse(string text)
        {
            List<Token> tokens = new List<Token>();
            Pair<State, Token> currentState = new Pair<State, Token>(State.None, new Token());

            currentState.Second.LineNumber = 1;

            foreach (char currentChar in text)
            {
                switch (currentState.First)
                {
                    case State.String:
                        switch (currentChar)
                        {
                            // String literal
                            // Handle escape sequences and end of string
                            case '\\':
                                // Skip next character
                                currentState.First = State.StringEscape;
                                break;
                            case '"':
                                EndToken(ref currentState, ref tokens);
                                break;
                            default:
                                currentState.Second.Text += currentChar;
                                break;
                        }
                        continue;
                    case State.StringEscape:
                        //Done with escape sequence, go back to string state
                        currentState.Second.Text += currentChar;
                        currentState.First = State.String;
                        continue;
                    case State.Comment:
                        switch (currentChar)
                        {
                            case '\n':
                            case '\r':
                            case '\t':
                                EndToken(ref currentState, ref tokens);
                                currentState.Second.LineNumber++;
                                break;
                            default:
                                currentState.Second.LineNumber++;
                                break;
                        }
                        continue;
                }

                if (char.IsDigit(currentChar))
                {
                    if (currentState.First == State.None)
                    {
                        currentState.First = State.Number;
                        currentState.Second.Type = TokenType.IntLitereral;
                        currentState.Second.Text += currentChar;
                    }
                    else
                    {
                        currentState.Second.Text += currentChar;
                    }
                    continue;
                }

                switch (currentChar)
                {
                    case '#':
                        currentState.First = State.Comment;
                        currentState.Second.Text = string.Empty;
                        break;
                    
                    case '[':
                    case  ']':
                        if (currentState.First != State.PotentialArray)
                        {
                            EndToken(ref currentState, ref tokens);
                            currentState.First = State.PotentialArray;
                            currentState.Second.Type = TokenType.Operator;
                            currentState.Second.Text += currentChar;
                        }
                        else if (currentState.First == State.PotentialArray)
                        {
                            currentState.Second.Text += currentChar;
                            EndToken(ref currentState, ref tokens);
                        }
                        break;
                    
                    case '-':
                    case '+':
                    case '&':
                    case ';':
                        if (currentState.First != State.String)
                        {
                            EndToken(ref currentState, ref tokens);
                            //Dummy state to force EndToken to accept it.
                            currentState.First = State.GetIdentifier;
                            currentState.Second.Type = TokenType.Operator;
                            currentState.Second.Text += currentChar;
                            EndToken(ref currentState, ref tokens);
                            currentState.First = State.GetIdentifier; // If it a +lol, -lol, ;lol then we need to get "lol"
                            currentState.Second.Type = TokenType.Identifier;
                        }
                        else
                        {
                            currentState.Second.Text += currentChar;
                        }
                        break;
                    
                    case '.':
                        if (currentState.First == State.None)
                        {
                            currentState.First = State.GetIdentifier;
                            currentState.Second.Type = TokenType.Operator;
                            currentState.Second.Text += currentChar;
                            EndToken(ref currentState, ref tokens);
                        }
                        else if (currentState.First == State.Number &&
                                 currentState.Second.Type == TokenType.IntLitereral)
                        {
                            //It's a float literal, so lets switch
                            currentState.Second.Text += currentChar;
                            currentState.Second.Type = TokenType.FloatLiteral;
                        }
                        else if (currentState.First == State.GetIdentifier)
                        {
                            //Working on an identifier and got a .
                            EndToken(ref currentState, ref tokens);
                            currentState.First = State.GetIdentifier; // Hacky
                            currentState.Second.Type = TokenType.Operator;
                            currentState.Second.Text += currentChar;
                            EndToken(ref currentState, ref tokens);
                        }
                        else
                        {
                            currentState.Second.Text += currentChar;
                        }
                        break;
                    
                    case 'f':
                        if (currentState.First == State.Number &&
                            currentState.Second.Type == TokenType.FloatLiteral)
                        {
                            EndToken(ref currentState, ref tokens);
                        }
                        else
                        {
                            currentState.Second.Text += currentChar;
                        }
                        break;
                    
                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                        if (currentState.First == State.String)
                        {
                            currentState.Second.Text += currentChar;
                        }
                        else
                        {
                            EndToken(ref currentState, ref tokens);
                            if(currentChar != ' ')
                                currentState.Second.LineNumber++;
                        }
                        break;
                    
                    case '"':
                        if (currentState.First != State.String)
                        {
                            //Start of string.
                            EndToken(ref currentState, ref tokens);
                            currentState.First = State.String;
                            currentState.Second.Type = TokenType.StringLitereral;
                        }
                        break;
                    
                    default:
                        if (currentState.First == State.None || currentState.First == State.Number)
                        {
                            EndToken(ref currentState, ref tokens);
                            currentState.First = State.GetIdentifier;
                            currentState.Second.Type = TokenType.Identifier;
                            currentState.Second.Text += currentChar;
                        }
                        else
                        {
                            currentState.Second.Text += currentChar;
                        }
                        break;
                }
            }
            
            EndToken(ref currentState, ref tokens);
            return tokens.ToArray();
        }
    }
}