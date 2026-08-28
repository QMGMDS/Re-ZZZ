using System;
using System.Collections.Generic;

namespace SPFramework
{
    /// <summary>
    /// 游戏模块管理系统
    /// </summary>
    public static class ModuleSystem
    {
        /// <summary>
        /// 默认模块数量
        /// </summary>
        private const int DEFAULT_MODULE_COUNT = 16;

        private static readonly Dictionary<Type, Module> s_moduleMaps = new Dictionary<Type, Module>(DEFAULT_MODULE_COUNT);
        private static readonly LinkedList<Module> s_modules = new LinkedList<Module>();

        #region 模块获取

        public static T GetModule<T>() where T : class
        {
            Type type = typeof(T);

            if (!type.IsInterface)
            {
                throw new ArgumentException($"类型 {type.FullName} 必须是一个接口", nameof(T));
            }

            if (s_moduleMaps.TryGetValue(type, out var module))
            {
                return module as T;
            }

            throw new InvalidOperationException($"游戏模块 {type.FullName} 尚未注册");
        }

        public static Module GetModule(Type type)
        {
            return s_moduleMaps.TryGetValue(type, out Module module) ? module : CreateModule(type);
        }

        #endregion

        #region 模块创建

        /// <summary>
        /// 创建游戏模块
        /// </summary>
        private static Module CreateModule(Type moduleType)
        {
            Module module = Activator.CreateInstance(moduleType) as Module;

            if (module == null)
            {
                throw new ArgumentException("创建游戏模块失败");
            }

            s_moduleMaps[moduleType] = module;
            RegisterModule(module);
            return module;
        }

        /// <summary>
        /// 注册自定义模块
        /// </summary>
        public static T RegisterModule<T>(Module module) where T : class
        {
            Type type = typeof(T);

            if (!type.IsInterface)
            {
                throw new ArgumentException($"类型 {type.FullName} 必须是一个接口", nameof(T));
            }

            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            if (!type.IsInstanceOfType(module))
            {
                throw new ArgumentException(
                    $"模块 {module.GetType().FullName} 未实现契约 {type.FullName}",
                    nameof(module));
            }

            s_moduleMaps[type] = module;
            RegisterModule(module);
            return module as T;
        }

        private static void RegisterModule(Module module)
        {
            LinkedListNode<Module> current = s_modules.First;

            while (current != null)
            {
                if (module.Priority > current.Value.Priority)
                {
                    break;
                }
                current = current.Next;
            }

            if (current != null)
            {
                s_modules.AddBefore(current, module);
            }
            else
            {
                s_modules.AddLast(module);
            }

            module.OnCreate();
        }

        #endregion

        /// <summary>
        /// 销毁并清理所有的模块
        /// </summary>
        public static void Destroy()
        {
            // 按优先级从低往高执行销毁处理（从后往前）
            LinkedListNode<Module> current = s_modules.Last;
            while (current != null)
            {
                current.Value.OnDestroy();
                current = current.Previous;
            }

            s_modules.Clear();
            s_moduleMaps.Clear();
        }
    }
}
