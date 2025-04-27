
using System;
using System.Collections.Generic;
using System.Diagnostics;
using AkoSharp.Gen;

namespace AkoSharp
{
    public static class Deserializer
    {
        public static AVar FromString(string input)
        {
            return new Parser(new Tokenizer(input).Tokenize()).Parse();
        }
    }
}