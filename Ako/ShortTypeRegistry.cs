using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ako
{
    public static class ShortTypeRegistry
    {
        private static Dictionary<string, Type> _registeredShortTypes = new();
        
        public static void Init()
        {
            var configs = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(e => e.GetTypes())
                .Where(e => e.IsClass && e.GetCustomAttribute<ShortType>() != null)
                .ToDictionary(mc => mc, mc => mc.GetCustomAttribute<ShortType>());
            
            foreach (var stName in configs)
            {
                if (stName.Value == null)
                    continue;
                //Log.Verbose($"Found Config: {stName.Value.Name} -> {stName.Key}");
                _registeredShortTypes.Add(stName.Value.Name, stName.Key);
            }
            
            _registeredShortTypes.Add("int", typeof(int));
            _registeredShortTypes.Add("float", typeof(float));
            _registeredShortTypes.Add("double", typeof(double));
            _registeredShortTypes.Add("string", typeof(string));
            _registeredShortTypes.Add("bool", typeof(bool));
            _registeredShortTypes.Add("byte", typeof(byte));
            _registeredShortTypes.Add("char", typeof(char));
            _registeredShortTypes.Add("decimal", typeof(decimal));
            _registeredShortTypes.Add("long", typeof(long));
            _registeredShortTypes.Add("short", typeof(short));
            _registeredShortTypes.Add("uint", typeof(uint));
            _registeredShortTypes.Add("ulong", typeof(ulong));
            _registeredShortTypes.Add("ushort", typeof(ushort));
            _registeredShortTypes.Add("sbyte", typeof(sbyte));
            _registeredShortTypes.Add("object", typeof(object));
            _registeredShortTypes.Add("void", typeof(void));
        }

        public static Type? GetTypeFromShortType(string shortTypeName)
        {
            if (!_registeredShortTypes.ContainsKey(shortTypeName))
                throw new Exception($"Failed to find registered type \"{shortTypeName}\"");
            return _registeredShortTypes[shortTypeName];
        }

        public static void Shutdown()
        {
            _registeredShortTypes.Clear();
        }
    }
}