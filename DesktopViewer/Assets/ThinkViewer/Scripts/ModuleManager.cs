using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Think.Viewer.Manager
{
    public interface IModule
    {
        void Initialize();
    }
    public class ModuleManager : MonoBehaviour
    {
        private static Dictionary<Type, IModule> _Modules = new Dictionary<Type, IModule>();
        public static void Add(IModule module)
        {
            if (!_Modules.ContainsValue(module))
            {
                module.Initialize();
                _Modules[module.GetType()] = (module);
            }
        }
        public static bool Remove(IModule module)
        {
            return _Modules.Remove(module.GetType());
        }
        public static M GetModule<M>() where M : IModule
        {
            Type moduleType = typeof(M);
            if (_Modules.ContainsKey(moduleType))
                return (M)_Modules[moduleType];
            return default;
        }
    }
}