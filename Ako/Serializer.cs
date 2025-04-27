using System;
using System.Collections.Generic;
using System.Text;

namespace AkoSharp;

public class Serializer
{
    public static string Serialize(AVar var)
    {
        if (var is ATable table)
            return VisitTable(table, false);
        
        if(var is AArray array)
            return VisitArray(array, false);
        
        throw new ArgumentException("Root variable must be a table or an array.", nameof(var));
    }

    private static string Visit(AVar var)
    {
        return var switch 
        {
            ATable table => VisitTable(table),
            AArray array => VisitArray(array),
            AkoNull _ => VisitNull(),
            AString str => VisitString(str),
            AInt i => VisitInt(i),
            AFloat f => VisitFloat(f),
            AShortType st => VisitShortType(st),
            ABool b => VisitBool(b),
            _ => throw new ArgumentException("Unknown variable type.", nameof(var))
        };
    }

    private static string VisitNull()
    {
        return ";";
    }
    
    private static string VisitString(AString var)
    {
        return $"\"{var.Value}\"";
    }
    
    private static string VisitInt(AInt var)
    {
        return var.Value.ToString();
    }
    
    private static string VisitFloat(AFloat var)
    {
        return var.Value.ToString();
    }
    
    private static string VisitShortType(AShortType var)
    {
        return $"&{ShortTypeRegistry.GetShortTypeFromType(var.Value)}";
    }
    
    private static string VisitBool(ABool var)
    {
        if ((bool)var.Value)
            return "+";
        return "-";
    }
    
    private static string VisitTable(ATable table, bool includeBrackets = true)
    {
        StringBuilder sb = new StringBuilder();

        if(includeBrackets)
            sb.Append("[\n");

        foreach (var (key, value) in table)
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

    private static string VisitArray(AArray array, bool includeBrackets = true)
    {
        StringBuilder sb = new StringBuilder();

        bool isAllNumbers = true;
        foreach (var var in array)
        {
            if (var is not AInt or AFloat)
            {
                isAllNumbers = false;
                break;
            }
        }

        if (isAllNumbers && array.Count <= 4)
        {
            //Treat as a vector
            //append the value then 'x' but not for the last element
            var last = array.Count;
            for (int i = 0; i < last; i++)
            {
                sb.Append(array[i].ToString());
                if (i != last - 1)
                    sb.Append('x');
            }

            return sb.ToString();
        }
        
        if(includeBrackets)
            sb.Append("[[\n");
        
        foreach (var var in array)
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