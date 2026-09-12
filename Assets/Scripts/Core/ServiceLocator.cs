using System;
using System.Collections.Generic;

namespace Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new();

        public static void Register<T>(T service) where T : class
        {
            Type type = typeof(T);
            if (services.ContainsKey(type))
            {
                return;
            }
            services[type] = service;
        }

        public static T Get<T>() where T : class
        {
            Type type = typeof(T);
            if (services.TryGetValue(type, out var service))
            {
                return (T)service;
            }
            UnityEngine.Debug.LogWarning($"ServiceLocator: Service {type.Name} not found.");
            return null;
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);
            if (services.TryGetValue(type, out var obj))
            {
                service = (T)obj;
                return true;
            }
            service = null;
            return false;
        }

        public static void Unregister<T>() where T : class
        {
            Type type = typeof(T);
            if (services.ContainsKey(type))
            {
                services.Remove(type);
            }
        }

        public static void Clear()
        {
            services.Clear();
        }
    }
}
