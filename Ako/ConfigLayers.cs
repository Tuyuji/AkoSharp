using System;
using System.Collections.Generic;
using System.Linq;

namespace AkoSharp;

//T needs to be an enum with int
public class ConfigLayers<T> where T: struct, Enum
{
    static ConfigLayers()
    {
        if (!typeof(T).IsEnum || Enum.GetUnderlyingType(typeof(T)) != typeof(int))
        {
            throw new InvalidOperationException(
                $"The generic type parameter {nameof(T)} must be an enum with int underlying type.");
        }
    }
    
    public Dictionary<T, AkoVar> Layers = new();
    
    public ConfigLayers()
    {
        foreach (var value in Enum.GetValues<T>())
        {
            if(!Layers.ContainsKey(value))
                Layers.Add(value, new AkoVar(AkoVar.VarType.TABLE));
        }
    }

    public AkoVar GetLayer(T layer)
    {
        return Layers[layer];
    }
    
    public void SetLayer(T layer, AkoVar value)
    {
        Layers[layer] = value;
    }

    public AkoVar? Get(params string[] path)
    {
        var enumValues = Enum.GetValues<T>();

        for (int i = enumValues.Length-1; i >= 0; i--)
        {
            var table = GetLayer(enumValues[i]);
            if(table.Count == 0)
                continue;
            
            var currentVar = table;
            bool found = false;

            for (int j = 0; j < path.Length; j++)
            {
                var key = path[j];
                if (currentVar.Type == AkoVar.VarType.TABLE)
                {
                    if (currentVar.ContainsKey(key))
                    {
                        currentVar = currentVar[key];
                        found = true;
                    }
                    else
                    {
                        found = false;
                        break;
                    }
                }
                //if we are at the last part of the path then we can return the value
                else if (j == path.Length - 1)
                {
                    return currentVar;
                }
                else
                {
                    found = false;
                    break;
                }
            }
            
            if (found)
            {
                return currentVar;
            }
        }

        return null;
    }
}