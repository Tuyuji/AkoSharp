using System;
using System.Collections.Generic;
using System.Linq;
using Ako.Gen;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace Ako
{
    public static class Deserializer
    {
        public class ThrowingErrorListener : BaseErrorListener
        {
            public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg,
                RecognitionException e)
            {
                throw new ParseCanceledException($"Line: {line}, Char: {charPositionInLine}, Message: {msg}");
            }
        }
        
        public static AkoVar FromString(string input)
        {
            var inputStream = new AntlrInputStream(input);
            var lexer = new AkoLexer(inputStream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new AkoParser(tokens);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new ThrowingErrorListener());
            IParseTree tree = parser.document();
            AkoDocumentVisitor eval = new AkoDocumentVisitor();
            return eval.Visit(tree) as AkoVar;
        }
    }
}