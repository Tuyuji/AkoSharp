using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AkoSharp
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

    public abstract class AVar
    {
        //Some nice conversions to ATable or AArray if this instance is one of them
        public ATable GetTable()
        {
            if (this is ATable table)
                return table;
            else
                throw new Exception("This instance is not a table");
        }
        
        public AArray GetArray()
        {
            if (this is AArray array)
                return array;
            else
                throw new Exception("This instance is not an array");
        }

        public AVar this[string key]
        {
            get => GetTable()[key];
            set => GetTable()[key] = value;
        } 
        
        public AVar this[int index]
        {
            get => GetArray()[index];
            set => GetArray()[index] = value;
        }
        
        public bool GetBool()
        {
            if (this is ABool b)
                return b.Value;
            else
                throw new Exception("This instance is not a bool");
        }
        
        public int GetInt()
        {
            if (this is AInt i)
                return i.Value;
            else
                throw new Exception("This instance is not an int");
        }
        
        public float GetFloat()
        {
            if (this is AFloat f)
                return f.Value;
            else
                throw new Exception("This instance is not a float");
        }
        
        public string GetString()
        {
            if (this is AString s)
                return s.Value;
            else
                throw new Exception("This instance is not a string");
        }
        
        public Type GetType()
        {
            if (this is AShortType t)
                return t.Value;
            else
                throw new Exception("This instance is not a type");
        }
        
        public Vector2 GetVector2()
        {
            if (this is AVector v)
                return new Vector2(v.Value.X, v.Value.Y);
            else
                throw new Exception("This instance is not a vector");
        }
        
        public Vector3 GetVector3()
        {
            if (this is AVector v)
                return new Vector3(v.Value.X, v.Value.Y, v.Value.Z);
            else
                throw new Exception("This instance is not a vector");
        }
        
        public Vector4 GetVector4()
        {
            if (this is AVector v)
                return v.Value;
            else
                throw new Exception("This instance is not a vector");
        }
        
        public bool IsNull()
        {
            return this is AkoNull;
        }
    }
    
    public class ATable : AVar, IEnumerable<KeyValuePair<string, AVar>>
    {
        private readonly Dictionary<string, AVar> _table = new();
        
        public void Add(string key, AVar value)
        {
            _table.Add(key, value);
        }
        
        //implicit conversion from ATable to Dictionary<string, AVar>
        public static implicit operator Dictionary<string, AVar>(ATable table)
        {
            return table._table;
        }
        
        public bool ContainsKey(string key)
        {
            return _table.ContainsKey(key);
        }
        
        public bool TryGet(string key, out AVar value)
        {
            return _table.TryGetValue(key, out value);
        }
        
        public AVar this[string key]
        {
            get => _table[key];
            set => _table[key] = value;
        }
        
        //For each

        public IEnumerator<KeyValuePair<string, AVar>> GetEnumerator()
        {
            foreach (var kvp in _table)
            {
                yield return kvp;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    public class AArray : AVar, IEnumerable<AVar>
    {
        private readonly List<AVar> _array = new();
        
        public int Count => _array.Count;
        
        public void Add(AVar value)
        {
            _array.Add(value);
        }

        public AVar this[int index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        public IEnumerator<AVar> GetEnumerator()
        {
            foreach (var value in _array)
            {
                yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    public class AString : AVar
    {
        public string Value;
        
        public AString(string value)
        {
            Value = value;
        }
        
        public static implicit operator string(AString s)
        {
            return s.Value;
        }
        
        public override string ToString()
        {
            return Value;
        }
    }
    
    public class AInt : AVar
    {
        public int Value;
        
        public AInt(int value)
        {
            Value = value;
        }
        
        public static implicit operator int(AInt i)
        {
            return i.Value;
        }
        
        public override string ToString()
        {
            return Value.ToString();
        }
    }
    
    public class AFloat : AVar
    {
        public float Value;
        
        public AFloat(float value)
        {
            Value = value;
        }
        
        public static implicit operator float(AFloat f)
        {
            return f.Value;
        }
        
        public override string ToString()
        {
            return Value.ToString();
        }
    }
    
    public class AShortType : AVar
    {
        public Type Value;
        
        public AShortType(Type value)
        {
            Value = value;
        }
        
        public static implicit operator Type(AShortType s)
        {
            return s.Value;
        }

        public override string ToString()
        {
            return ShortTypeRegistry.GetShortTypeFromType(Value);
        }
    }
    
    public class ABool : AVar
    {
        public bool Value;
        
        public ABool(bool value)
        {
            Value = value;
        }
        
        public static implicit operator bool(ABool b)
        {
            return b.Value;
        }

        public override string ToString()
        {
            return Value ? "+" : "-";
        }
    }
    
    public class AkoNull : AVar
    {
    }
    
    public class AVector : AVar
    {
        public Vector4 Value;
        public int Count = 0;
        
        public AVector(Vector4 value)
        {
            Value = value;
            Count = 4;
        }
        
        public AVector(Vector3 value)
        {
            Value = new Vector4(value, 0);
            Count = 3;
        }
        
        public AVector(Vector2 value)
        {
            Value = new Vector4(value, 0, 0);
            Count = 2;
        }
        
        public static implicit operator Vector4(AVector v)
        {
            return v.Value;
        }
        
        public static implicit operator Vector3(AVector v)
        {
            return new Vector3(v.Value.X, v.Value.Y, v.Value.Z);
        }
        
        public static implicit operator Vector2(AVector v)
        {
            return new Vector2(v.Value.X, v.Value.Y);
        }
    }
    
    /*
    [Obsolete("Use AVar instead")]
    public class AkoVar
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
                    case AkoVar s:
                        this.Type = s.Type;
                        this._value = s._value;
                        break;
                    default:
                        throw new Exception("Unsupported type.");
                        break;
                }
            }
        }
        
        protected dynamic _value;

        public int Count
        {
            get
            {
                switch (Type)
                {
                    case VarType.TABLE:
                        return TableValue.Count;
                    case VarType.ARRAY:
                        return ArrayValue.Count;
                    default:
                        throw new Exception("Invalid type, expected bool got " + Enum.GetName(typeof(VarType), Type));
                }
            }
        }

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
                    sb.Append(TableValue.Count);
                    break;
                case VarType.ARRAY:
                    sb.Append("Count: ");
                    sb.Append(ArrayValue.Count);
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

        public float GetFloat()
        {
            return Type switch
            {
                VarType.INT => _value,
                VarType.FLOAT => _value,
                _ => throw new Exception("Can't convert to float from " + Enum.GetName(typeof(VarType), Type))
            };
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

        private static void CheckCount(AkoVar var, int expected)
        {
            if(var.Count != expected)
                throw new Exception($"Array size mismatch, expected {expected} got {var.Count}");
        }
        
        public static implicit operator Vector2(AkoVar array)
        {
            if(array.Type != VarType.ARRAY)
                throw new Exception("Invalid type, expected bool got " + Enum.GetName(typeof(VarType), array.Type));

            CheckCount(array, 2);
            
            return new Vector2(array[0].GetFloat(), array[1].GetFloat());
        }
        
        public static implicit operator Vector3(AkoVar array)
        {
            if(array.Type != VarType.ARRAY)
                throw new Exception("Invalid type, expected bool got " + Enum.GetName(typeof(VarType), array.Type));
            
            CheckCount(array, 3);

            return new Vector3(array[0].GetFloat(), array[1].GetFloat(), array[2].GetFloat());
        }

        public static implicit operator Vector4(AkoVar array)
        {
            if(array.Type != VarType.ARRAY)
                throw new Exception("Invalid type, expected bool got " + Enum.GetName(typeof(VarType), array.Type));
            
            CheckCount(array, 4);

            return new Vector4(array[0].GetFloat(), array[1].GetFloat(), array[2].GetFloat(), array[3].GetFloat());
        }

        #endregion

        #region Table Functions

        public ConcurrentDictionary<string, AkoVar> TableValue => _value;

        public bool ContainsKey(string key)
        {
            return TableValue.ContainsKey(key);
        }

        public bool TryGet(string key, out AkoVar value)
        {
            return TableValue.TryGetValue(key, out value);
        }
        
        public void Get(string key, out AkoVar value)
        {
            if(!TableValue.ContainsKey(key))
                TableValue.TryAdd(key, new AkoVar());
                
            TableValue.TryGetValue(key, out value);
        }

        public void GetTable(string key, out AkoVar value)
        {
            if(!TableValue.ContainsKey(key))
                TableValue.TryAdd(key, new AkoVar(VarType.TABLE));
                
            TableValue.TryGetValue(key, out var tempValue);
            value = tempValue;
        }

        public bool TrySet(string key, AkoVar value)
        {
            if (TableValue.ContainsKey(key))
                return false;
            
            return TableValue.TryAdd(key, value);
        }
        
        public dynamic this[string key]
        {
            get => TableValue[key];
            set
            {
                if (value is AkoVar)
                    TableValue[key] = value;
                else
                    throw new Exception("AkoTable can only contain AkoVar");
            }
        }

        #endregion

        #region Array Functions

        public List<AkoVar> ArrayValue => _value;

        public void Add(AkoVar value)
        {
            ArrayValue.Add(value);
        }
        
        public void Remove(AkoVar value)
        {
            ArrayValue.Remove(value);
        }
        
        public AkoVar this[int key]
        {
            get => ArrayValue[key];
            set => ArrayValue[key] = value;
        }

        #endregion

        public void Merge(AkoVar newVar)
        {
            if (this.Type != newVar.Type)
                throw new Exception("Can't merge two different types");

            switch (this.Type)
            {
                case VarType.ARRAY:
                    foreach (var value in newVar.ArrayValue)
                    {
                        this.ArrayValue.Add(value);
                    }
                    break;
                case VarType.TABLE:
                    //For tables we will replace the old value with the new one
                    //unless its an array or table in which case we will merge them
                    foreach (var (key, value) in newVar.TableValue)
                    {
                        if (this.TableValue.ContainsKey(key))
                        {
                            if (value.Type == VarType.ARRAY || value.Type == VarType.TABLE)
                            {
                                this.TableValue[key].Merge(value);
                            }
                            else
                            {
                                this.TableValue[key] = value;
                            }
                        }
                        else
                        {
                            this.TableValue.TryAdd(key, value);
                        }
                    }
                    break;
                default:
                    throw new Exception("Can't merge a " + Enum.GetName(typeof(VarType), this.Type));
                
            }
        }
    }*/
}