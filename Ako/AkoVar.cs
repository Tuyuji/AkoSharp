using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Ako
{
    public struct ShortTypeHandle
    {
        public string ShortName;
        public Type Type;
        
        public ShortTypeHandle(string shortName, Type type)
        {
            ShortName = shortName;
            Type = type;
        }
        
        //Implicit conversion from ShortTypeHandle to Type
        public static implicit operator Type(ShortTypeHandle handle)
        {
            return handle.Type;
        }
    }
    public partial class AkoVar
    {
        public enum VarType
        {
            NULL = 0,
            STRING = 1,
            INT = 2,
            FLOAT = 3,
            SHORT_TYPE = 4,
            BOOL = 5,
            TABLE = 6,
            ARRAY = 7,
        }

        public AkoVar(VarType type = VarType.NULL)
        {
            switch (type)
            {
                case VarType.TABLE:
                    ConvertToTable();
                    break;
                case VarType.ARRAY:
                    ConvertToArray();
                    break;
                default:
                    Type = type;
                    break;
            }
        }

        public VarType Type;

        public dynamic Value
        {
            get
            {
                return _value;
            }
            set
            {
                switch (value)
                {
                    case string s:
                        _value = s;
                        Type = VarType.STRING;
                        break;
                    case bool b:
                        _value = b;
                        Type = VarType.BOOL;
                        break;
                    case int i:
                        _value = i;
                        Type = VarType.INT;
                        break;
                    case float f:
                        _value = f;
                        Type = VarType.FLOAT;
                        break;
                    case ShortTypeHandle h:
                        _value = h;
                        Type = VarType.SHORT_TYPE;
                        break;
                    case null:
                        _value = null;
                        Type = VarType.NULL;
                        break;
                    default:
                        throw new Exception("Unsupported type.");
                        break;
                }
            }
        }
        
        protected dynamic _value;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
           
            sb.Append("(");
            sb.Append(Enum.GetName(typeof(VarType), Type));
            sb.Append(") ");
            switch (Type)
            {
                case VarType.STRING:
                    sb.Append('"');
                    sb.Append(Value as string);
                    sb.Append('"');
                    break;
                case VarType.TABLE:
                    sb.Append("Count: ");
                    sb.Append(_dic.Count);
                    break;
                case VarType.ARRAY:
                    sb.Append("Count: ");
                    sb.Append(_list.Count);
                    break;         
                default:
                    sb.Append(Value.ToString());
                    break;
            }
            

            return sb.ToString();
        }
        
        public void ConvertToTable()
        {
            if (Type == VarType.TABLE)
            {
                return;
            }
            else
            {
                this.Type = VarType.TABLE;
                _value = new ConcurrentDictionary<string, AkoVar>();
                return;
            }
        }

        public void ConvertToArray()
        {
            if (Type == VarType.ARRAY)
            {
                return;
            }
            Type = VarType.ARRAY;
            _value = new List<AkoVar>();
        }

        public void SetValue(string value)
        {
            Type = VarType.STRING;
            value = value;
        }
        
        public void SetValue(int value)
        {
            Type = VarType.INT;
            Value = value;
        }
        
        public void SetValue(float value)
        {
            Type = VarType.FLOAT;
            Value = value;
        }
        
        public void SetValue(bool value)
        {
            Type = VarType.BOOL;
            Value = value;
        }
        
        public void SetValue(Type value)
        {
            Type = VarType.SHORT_TYPE;
            Value = value;
        }

        #region Implicit operators
        
        public static implicit operator string(AkoVar sVar)
        {
            if(sVar.Type != VarType.STRING)
                throw new Exception("Invalid type, expected string got " + Enum.GetName(typeof(VarType), sVar.Type));
            return sVar.Value;
        }
        
        public static implicit operator int(AkoVar iVar)
        {
            if(iVar.Type != VarType.INT)
                throw new Exception("Invalid type, expected int got " + Enum.GetName(typeof(VarType), iVar.Type));
            return iVar.Value;
        }
        
        public static implicit operator float(AkoVar fVar)
        {
            if(fVar.Type != VarType.FLOAT)
                throw new Exception("Invalid type, expected float got " + Enum.GetName(typeof(VarType), fVar.Type));
            return fVar.Value;
        }
        
        public static implicit operator Type(AkoVar tVar)
        {
            if(tVar.Type != VarType.SHORT_TYPE)
                throw new Exception("Invalid type, expected short type got " + Enum.GetName(typeof(VarType), tVar.Type));
            return tVar.Value;
        }
        
        public static implicit operator bool(AkoVar bVar)
        {
            if(bVar.Type != VarType.BOOL)
                throw new Exception("Invalid type, expected bool got " + Enum.GetName(typeof(VarType), bVar.Type));
            return bVar.Value;
        }

        public static implicit operator VarType(AkoVar aVar)
        {
            return aVar.Type;
        }

        #endregion

        #region Table Functions

        private ConcurrentDictionary<string, AkoVar> _dic => _value;

        public bool TryGet(string key, out AkoVar value)
        {
            return _dic.TryGetValue(key, out value);
        }
        
        public void Get(string key, out AkoVar value)
        {
            if(!_dic.ContainsKey(key))
                _dic.TryAdd(key, new AkoVar());
                
            _dic.TryGetValue(key, out value);
        }

        public void GetTable(string key, out AkoVar value)
        {
            if(!_dic.ContainsKey(key))
                _dic.TryAdd(key, new AkoVar(VarType.TABLE));
                
            _dic.TryGetValue(key, out var tempValue);
            value = tempValue;
        }
        
        public dynamic this[string key]
        {
            get => _dic[key];
            set
            {
                if (value is AkoVar)
                    _dic[key] = value;
                else
                    throw new Exception("AkoTable can only contain AkoVar");
            }
        }

        #endregion

        #region Array Functions

        private List<AkoVar> _list => _value;

        public void Add(AkoVar value)
        {
            _list.Add(value);
        }
        
        public void Remove(AkoVar value)
        {
            _list.Remove(value);
        }
        
        public AkoVar this[int key]
        {
            get => _list[key];
            set => _list[key] = value;
        }

        #endregion
    }
}