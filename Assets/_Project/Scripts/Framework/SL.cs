using System;
using System.Collections.Generic;

namespace VTBeat {
    public static class SL {
        private static readonly Dictionary<Type, object> s_Services = new();
        
        public static void Register<T>(T service) where T : class {
            s_Services[typeof(T)] = service;
        }
        public static void Unregister<T>(T service) where T : class {
            s_Services.Remove(typeof(T));
        }
        public static bool TryGet<T>(out T service) where T : class {
            service = null;
            if (s_Services.TryGetValue(typeof(T), out object value)) {
                service = value as T;
                return service != null;
            }
            
            return false;
        }
    }
}