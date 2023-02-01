using System;
using System.Collections.Generic;
using System.Text;

namespace AkoSharp;

public class Serializer
{
    public static string Serialize(AkoVar var)
    {
        if (var.Type is not (AkoVar.VarType.TABLE or AkoVar.VarType.ARRAY))
            throw new ArgumentException("Root variable must be a table or an array.", nameof(var));

        switch (var.Type)
        {
            case AkoVar.VarType.TABLE:
                return VisitTable(var, false);
            case AkoVar.VarType.ARRAY:
                return VisitArray(var, false);
            default:
                throw new Exception("This should never happen.");
        }
    }

    private static string Visit(AkoVar var)
    {
        switch (var.Type)
        {
            
            case AkoVar.VarType.TABLE:
                return VisitTable(var);
            case AkoVar.VarType.ARRAY:
                return VisitArray(var);
            case AkoVar.VarType.NULL:
                return VisitNull(var);
            case AkoVar.VarType.STRING:
                return VisitString(var); 
            case AkoVar.VarType.INT:
                return VisitInt(var);
            case AkoVar.VarType.FLOAT:
                return VisitFloat(var);
            case AkoVar.VarType.SHORT_TYPE:
                return VisitShortType(var);
            case AkoVar.VarType.BOOL:
                return VisitBool(var);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static string VisitNull(AkoVar var)
    {
        return ";";
    }
    
    private static string VisitString(AkoVar var)
    {
        return $"\"{var.Value}\"";
    }
    
    private static string VisitInt(AkoVar var)
    {
        return var.Value.ToString();
    }
    
    private static string VisitFloat(AkoVar var)
    {
        return var.Value.ToString();
    }
    
    private static string VisitShortType(AkoVar var)
    {
        return $"&{ShortTypeRegistry.GetShortTypeFromType(var.Value)}";
    }
    
    private static string VisitBool(AkoVar var)
    {
        if ((bool)var.Value)
            return "+";
        return "-";
    }
    
    private static string VisitTable(AkoVar table, bool includeBrackets = true)
    {
        StringBuilder sb = new StringBuilder();

        if(includeBrackets)
            sb.Append("[\n");

        foreach (var (key, value) in table.TableValue)
        {
            if(includeBrackets)
                sb.Append('\t');
            sb.Append(key);
            sb.Append(' ');
            sb.Append(Visit(value));
            sb.Append('\n');
        }

        if(includeBrackets)
            sb.Append(']');
        return sb.ToString();
    }

    private static string VisitArray(AkoVar array, bool includeBrackets = true)
    {
        StringBuilder sb = new StringBuilder();

        bool isAllNumbers = true;
        foreach (var var in array.ArrayValue)
        {
            switch (var.Type)
            {
                case AkoVar.VarType.INT:
                    case AkoVar.VarType.FLOAT:
                    break;
                default:
                    isAllNumbers = false;
                    break;
            }
        }

        if (isAllNumbers && array.ArrayValue.Count <= 4)
        {
            //Treat as a vector
            //append the value then 'x' but not for the last element
            var last = array.ArrayValue.Count;
            for (int i = 0; i < last; i++)
            {
                sb.Append(array.ArrayValue[i].Value);
                if (i != last - 1)
                    sb.Append('x');
            }

            return sb.ToString();
        }
        
        if(includeBrackets)
            sb.Append("[[\n");
        
        foreach (var var in array.ArrayValue)
        {
            if(includeBrackets)
                sb.Append('\t');
            sb.Append(Visit(var));
            sb.Append(' ');
        }

        if(includeBrackets)
            sb.Append("\n]]\n");
        
        return sb.ToString();
    }
}