using System;
using System.Collections.Generic;
using System.Globalization;
using Ako.Gen;
using Antlr4.Runtime.Tree;
using KeyVarPair = System.Collections.Generic.KeyValuePair<string[], Ako.AkoVar>;
namespace Ako;

internal class AkoDocumentVisitor : AkoBaseVisitor<object?>
{
    public override object? VisitDocumentTable(AkoParser.DocumentTableContext context)
    {
        return Visit(context.table_expr_list());
    }

    public override object? VisitDocumentArray(AkoParser.DocumentArrayContext context)
    {
        return Visit(context.array_expr());
    }

    public override object? VisitTable_expr(AkoParser.Table_exprContext context)
    {
        return Visit(context.table_expr_list());
    }

    public override object? VisitArray_expr(AkoParser.Array_exprContext context)
    {
        return Visit(context.array_expr_list());
    }

    public override object? VisitValueBasicString(AkoParser.ValueBasicStringContext context)
    {
        var str = context.BASIC_STRING().GetText()[1..^1];
        return new AkoVar(AkoVar.VarType.STRING){Value = ParseEscapedString(str)};
    }

    public override object? VisitTable_expr_list(AkoParser.Table_expr_listContext context)
    {
        var table = new AkoVar(AkoVar.VarType.TABLE);
        foreach (var pairContext in context.key_value_pair())
        {
            var result = Visit(pairContext);
            if(result is not KeyVarPair keyValue)
                throw new Exception("Invalid key value pair");
            
            var keys = keyValue.Key;
            //so we need to go though the keys and add them to the table
            //so the last key is the value
            //and the rest are tables.
            var currentTable = table;
            for (int i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                if (i == keys.Length - 1)
                {
                    currentTable[key] = keyValue.Value;
                }
                else
                {
                    if (!currentTable.ContainsKey(key))
                    {
                        currentTable[key] = new AkoVar(AkoVar.VarType.TABLE);
                    }
                    currentTable = currentTable[key];
                }
            }
        }

        return table;
    }

    public override object? VisitArray_expr_list(AkoParser.Array_expr_listContext context)
    {
        var array = new AkoVar(AkoVar.VarType.ARRAY);
        foreach (var valueContext in context.value())
        {
            var result = Visit(valueContext);
            if(result is not AkoVar value)
                throw new Exception("Invalid value");
            
            array.Add(value);
        }

        return array;
    }

    public override object? VisitKey_value_lefthand_pair(AkoParser.Key_value_lefthand_pairContext context)
    {
        var value = Visit(context.lefthand_expr());
        if(value is not AkoVar visitVar)
            throw new Exception("Invalid lefthand expression");

        var key = Visit(context.key());
        if(key is not string[] visitKey)
            throw new Exception("Invalid key");

        return new KeyVarPair(visitKey, visitVar);
    }

    public override object? VisitKey_value_righthand_pair(AkoParser.Key_value_righthand_pairContext context)
    {
        var key = Visit(context.key());
        if(key is not string[] visitKey)
            throw new Exception("Invalid key");
        
        var value = Visit(context.value());
        if(value is not AkoVar visitVar)
            throw new Exception("Invalid righthand expression");
        
        return new KeyVarPair(visitKey, visitVar);
    }

    public override object? VisitBoolConstant(AkoParser.BoolConstantContext context)
    {
        var strValue = context.BOOL().GetText();
        switch (strValue)
        {
            case "true":
            case "+":
                return new AkoVar(AkoVar.VarType.BOOL){Value = true};
            case "false":
            case "-":
                return new AkoVar(AkoVar.VarType.BOOL){Value = false};
            default:
                throw new Exception("Invalid bool value");
        }
    }

    public override object? VisitNullConstant(AkoParser.NullConstantContext context)
    {
        return new AkoVar(AkoVar.VarType.NULL);
    }

    public override object? VisitSimple_key(AkoParser.Simple_keyContext context)
    {
        if(context.quoted_key() is not null)
            return Visit(context.quoted_key());

        if (context.unquoted_key() is not null)
            return Visit(context.unquoted_key());
        
        throw new Exception("Invalid key");
    }

    public override object? VisitDotted_key(AkoParser.Dotted_keyContext context)
    {
        List<string> keys = new();
        foreach (var keyContext in context.simple_key())
        {
            var keyResult = Visit(keyContext);
            if (keyResult is string key)
            {
                keys.Add(key);    
            }else if (keyResult is string[] keyArray)
            {
                keys.AddRange(keyArray);
            }
            else
            {
                throw new Exception("Invalid key");
            }
            
        }

        return keys.ToArray();
    }

    public override object? VisitQuoted_key(AkoParser.Quoted_keyContext context)
    {
        if (context.LITERAL_STRING() != null)
            return new[]{context.LITERAL_STRING().GetText()[1..^1]};

        if (context.BASIC_STRING() != null)
            return new[]{context.BASIC_STRING().GetText()[1..^1]};
        
        throw new Exception("Invalid key");
    }

    public override object? VisitUnquoted_key(AkoParser.Unquoted_keyContext context)
    {
        return new[]{context.UNQUOTED_KEY().GetText()};
    }

    public override object? VisitValueNum(AkoParser.ValueNumContext context)
    {
        return Visit(context.num_value());
    }

    public override object? VisitIntConstant(AkoParser.IntConstantContext context)
    {
        if (context.DEC_INT() != null)
            return ParseDecInt(context.DEC_INT());
        
        if (context.HEX_INT() != null)
            return ParseHexInt(context.HEX_INT());

        if (context.BIN_INT() != null)
            return ParseBinInt(context.BIN_INT());

        throw new Exception("Invalid int value");
    }

    public override object VisitFloatConstant(AkoParser.FloatConstantContext context)
    {
        return ParseFloat(context.FLOAT());
    }

    public override object? VisitVectorConstant(AkoParser.VectorConstantContext context)
    {
        //Vectors are just arrays of numbers, eg. 900x600x32 or 1x2x3x4 etc.
        //So we just parse them as arrays.
        //now due to how vectors are done in the grammer we need to remove the x's
        //and then parse.
        var vecText = context.VECTOR().GetText();
        var elements = vecText.Split('x');
        var array = new AkoVar(AkoVar.VarType.ARRAY);
        foreach (var element in elements)
        {
            //Elements can be DEC_INT, HEX_INT, BIN_INT, FLOAT
            //So we need to check which one it is.
            if (int.TryParse(element, out var intVal))
            {
                array.Add(new AkoVar(AkoVar.VarType.INT){Value = intVal});
            }
            else if (float.TryParse(element, out var floatVal))
            {
                array.Add(new AkoVar(AkoVar.VarType.FLOAT){Value = floatVal});
            }
            else
            {
                throw new Exception("Invalid vector value");
            }
        }
        
        return array;
    }

    public override object VisitValueShortType(AkoParser.ValueShortTypeContext context)
    {
        //Short type eg. &int or &float
        var str = context.SHORT_TYPE().GetText()[1..];
        var type = ShortTypeRegistry.GetTypeFromShortType(str);
       
        return new AkoVar(AkoVar.VarType.SHORT_TYPE) { Value = new ShortTypeHandle(str, type) };

    }

    private AkoVar ParseDecInt(ITerminalNode node)
    {
        var str = node.GetText();
        bool isPositive = true;
        if (str.StartsWith('-') || str.StartsWith('+'))
        {
            isPositive = str.StartsWith('+');
            str = str[1..];
        }

        return new AkoVar(AkoVar.VarType.INT)
        {
            Value = isPositive ? int.Parse(str) : -int.Parse(str)
        };
    }

    private AkoVar ParseHexInt(ITerminalNode node)
    {
        var str = node.GetText();
        //Start is `x
        str = str[2..];
        int value = int.Parse(str, NumberStyles.HexNumber);
        return new AkoVar(AkoVar.VarType.INT){Value = value};
    }

    private AkoVar ParseBinInt(ITerminalNode node)
    {
        var str = node.GetText();
        //Start is `b
        str = str[2..];
        int value = Convert.ToInt32(str, 2);
        return new AkoVar(AkoVar.VarType.INT){Value = value};
    }

    private AkoVar ParseFloat(ITerminalNode node)
    {
        var str = node.GetText();
        //check if str ends with f or F and remove it
        if (str.EndsWith('f') || str.EndsWith('F'))
            str = str[..^1];
        
        float value = Convert.ToSingle(str);
        return new AkoVar(AkoVar.VarType.FLOAT){Value = value};
    }

    private string ParseEscapedString(string input)
    {
        input = input.Replace("\\\"", "\"");
        input = input.Replace("\\n", "\n");
        input = input.Replace("\\", "\\");
        return input;
    }
}