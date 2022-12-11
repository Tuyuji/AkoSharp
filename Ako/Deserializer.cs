using System;
using System.Collections.Generic;
using System.Linq;

namespace Ako
{
    public static class Deserializer
    {
        private class Iterator<T>
        {
            private readonly T[] _array;
            private int _index;
            
            public T Current => _array[_index];
            public T Peek {
                get
                {
                    if(_index + 1 > _array.Length)
                        throw new IndexOutOfRangeException();
                    return _array[_index + 1];
                }
            }
            
            public Iterator(T[] array)
            {
                this._array = array;
                this._index = 0;
            }
            
            public bool HasNext()
            {
                return this._index < this._array.Length-1;
            }
            
            public void Next()
            {
                if (!HasNext()) return;
                this._index++;
            }
        }
        
        public static AkoVar FromString(string input)
        {
            return FromTokens(AkoTokenizer.Parse(input));
        }

        private static AkoVar FromTokens(Token[] tokens)
        {
            var it = new Iterator<Token>(tokens);
            //Just see if it starts with a identifier aka root being a table or 
            //an array.
            if (it.Current.Type == TokenType.Operator && it.Current.Text == "[[")
            {
                var array = new AkoVar(AkoVar.VarType.ARRAY);
                it.Next();
                HandleArray(array, ref it);
                return array;
            }
            else
            {
                var root = new AkoVar();
                root.ConvertToTable();
                while(it.HasNext())
                    HandleStatement(root, ref it);
                return root;
            }

            return null;
        }
        
        private static dynamic GetValue(ref Iterator<Token> it)
        {
            switch (it.Current.Type)
            {
                case TokenType.IntLitereral:
                    int intLiteral = int.Parse(it.Current.Text);
                    it.Next();
                    return intLiteral;
                case TokenType.FloatLiteral:
                    float floatLiteral = float.Parse(it.Current.Text);
                    it.Next();
                    return floatLiteral;
                case TokenType.BooleanLiteral:
                    bool booleanLiteral = it.Current.Text.ToLower()[0] == 't';
                    it.Next();
                    return booleanLiteral;
                case TokenType.StringLitereral:
                    string strLiteral = it.Current.Text;
                    it.Next();
                    return strLiteral;
                case TokenType.Operator:
                    switch (it.Current.Text[0])
                    {
                        case '+':
                            it.Next();
                            return true;
                        case '-':
                            it.Next();
                            return false;
                        case ';':
                            it.Next();
                            return null;
                        case '&':
                            it.Next();
                            var shortTypeHandle = new ShortTypeHandle(it.Current.Text,
                                ShortTypeRegistry.GetTypeFromShortType(it.Current.Text));
                            it.Next();
                            return shortTypeHandle;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        /// <summary>
        /// Consumes tokens in It and returns the AkoVar it represents.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="it"></param>
        private static AkoVar PopIdentifer(AkoVar table, ref Iterator<Token> it)
        {
            if(it.Current!.Type != TokenType.Identifier)
                throw new Exception("Expected identifier");
            
            //Tokens are setup like this: [Identifier, Operator, Identifier] for example: window.width

            AkoVar currentTable = table;

            if (it.HasNext() && it.Peek.Type == TokenType.Operator && it.Peek.Text == ".")
            {
                while (it.HasNext())
                {
                    //Its always gonna be a identifier.
                    //We'll break out of this loop when we notice we hit the end of the identifier.

                    if (it.Current.Type == TokenType.Identifier)
                    {
                        if (it.Peek.Type == TokenType.Operator && it.Peek.Text == ".")
                        {
                            //We know we're gonna be looking for a table with the name in the current identifier.
                            //So lets grab a table with currentTable
                            currentTable.GetTable(it.Current.Text, out currentTable);
                            it.Next(); // Current = '.' op
                            it.Next(); // Current = Identifier
                        }
                        else
                        {
                            //Done, lets get the var and return it
                            currentTable.Get(it.Current.Text, out AkoVar result);
                            it.Next();
                            return result;
                        }
                    }
                }
            }
            else
            {
                currentTable.Get(it.Current.Text, out AkoVar result);
                it.Next();
                return result;
            }
            
            

            return null;
        }

        /// <summary>
        /// This will handle cases like the following:
        ///    +window.show
        ///     window.res.width 100
        ///     render.size [width 100 height 200]
        /// but just one line.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="it"></param>
        private static void HandleStatement(AkoVar table, ref Iterator<Token> it)
        {
            AkoVar identifier = null;
            dynamic value = null;

            //Operators like +, - can be put in front of an identifier.
            if (it.Current.Type == TokenType.Operator)
            {
                value = GetValue(ref it);
                
                identifier = PopIdentifer(table, ref it);
                if (identifier == null)
                    throw new Exception("Expected identifier");
                
                identifier.Value = value;
                return;
            }
            
            identifier = PopIdentifer(table, ref it);
            if (identifier == null)
                throw new Exception("Expected identifier");

            if (it.Current.Type == TokenType.Operator)
            {
                switch (it.Current.Text)
                {
                    case "+":
                        identifier.Value = true;
                        it.Next();
                        break;
                    case "-":
                        identifier.Value = false;
                        it.Next();
                        break;
                    case ";":
                        identifier.Value = null;
                        it.Next();
                        break;
                        
                    case "[":
                        identifier.ConvertToTable();
                        //Handle block requires us to be inside 
                        it.Next();
                        HandleBlock(identifier, ref it);
                        break;
                        
                    case "[[":
                        identifier.ConvertToArray();
                        it.Next();
                        HandleArray(identifier, ref it);
                        break;
                }
            }
            else
            {
                identifier.Value = GetValue(ref it);
            }
        }

        private static void HandleArray(AkoVar var, ref Iterator<Token> it)
        {
            while (!(it.Current.Type == TokenType.Operator && it.Current.Text == "]]"))
            {
                if (it.Current.Type == TokenType.Operator && it.Current.Text == "[")
                {
                    it.Next();
                    var table = new AkoVar(AkoVar.VarType.TABLE);
                    HandleBlock(table, ref it);
                    var.Add(table);
                    continue;
                }
                
                var value = GetValue(ref it);
                var.Add(new AkoVar()
                {
                    Value = value
                });
            }

            it.Next();
        }

        private static void HandleBlock(AkoVar table, ref Iterator<Token> it)
        {
            while (!(it.Current.Type == TokenType.Operator && it.Current.Text == "]"))
            {
                HandleStatement(table, ref it);
            }

            it.Next();
        }
    }
}