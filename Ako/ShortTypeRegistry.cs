using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AkoSharp
{
    public static class ShortTypeRegistry
    {
        private static Dictionary<string, Type> _registeredShortTypes = new();
        
        public static void Register(Assembly[] assemblies)
        {
            var configs = assemblies
                .SelectMany(e => e.GetTypes())
                .Where(e => e.IsClass && e.GetCustomAttribute<ShortType>() != null)
                .ToDictionary(mc => mc, mc => mc.GetCustomAttribute<ShortType>());
            
            foreach (var stName in configs)
            {
                if (stName.Value == null)
                    continue;
                
                Register(stName.Value.Name, stName.Key);
            }
        }
        
        public static void Register(Assembly assembly)
        {
            Register(new[] {assembly});
        }

        public static void Register(string shortName, Type type)
        {
            _registeredShortTypes.Add(shortName, type);
        }
        
        public static void Register<T>(string shortName)
        {
            _registeredShortTypes.Add(shortName, typeof(T));
        }

        public static void Clear()
        {
            _registeredShortTypes.Clear();
        }

        public static void RegisterCTypes()
        {
            Register<int>("int");
            Register<float>("float");
            Register<double>("double");
            Register<string>("string");
            Register<bool>("bool");
            Register<byte>("byte");
            Register<char>("char");
            Register<decimal>("decimal");
            Register<long>("long");
            Register<short>("short");
            Register<uint>("uint");
            Register<ulong>("ulong");
            Register<ushort>("ushort");
            Register<sbyte>("sbyte");
            Register<object>("object");
            Register("void", typeof(void));
        }
        
        public static void Remove(string shortName)
        {
            _registeredShortTypes.Remove(shortName);
        }
        
        public static Type? GetTypeFromShortType(string shortTypeName)
        {
            if (!_registeredShortTypes.ContainsKey(shortTypeName))
                throw new Exception($"Failed to find registered type \"{shortTypeName}\"");
            return _registeredShortTypes[shortTypeName];
        }

        public static void AutoRegister(bool registerCTypes = true)
        {
            Register(AppDomain.CurrentDomain.GetAssemblies());
            if (registerCTypes)
                RegisterCTypes();
        }
        
        [Obsolete("Use AutoRegister instead. " +
                  "Will be removed in 3.0.0", false)]
        public static void Init()
        {
            AutoRegister();
        }
        
        [Obsolete("Use Clear instead. " +
                  "Will be removed in 3.0.0", false)]
        public static void Shutdown()
        {
            _registeredShortTypes.Clear();
        }

        public static string GetShortTypeFromType(Type type)
        {
            var shortType = _registeredShortTypes.FirstOrDefault(e => e.Value == type);
            if (shortType.Key == null)
                throw new Exception($"Failed to find registered type \"{type}\"");
            return shortType.Key;
        }
    }
}