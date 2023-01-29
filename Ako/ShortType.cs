using System;

namespace AkoSharp
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ShortType : Attribute
    {
        public readonly string Name;

        public ShortType(string name)
        {
            Name = name;
        }
    }
}