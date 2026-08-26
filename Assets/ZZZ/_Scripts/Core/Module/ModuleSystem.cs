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

        // 链表排序 列表遍历
        private static readonly LinkedList<Module> s_updateModules = new LinkedList<Module>();
        private static readonly List<IUpdateModule> s_updateExecuteList = new List<IUpdateModule>(DEFAULT_MODULE_COUNT);

        // 脏标记 - 模块 Update 执行链表更新时刷新
        private static bool s_isExecuteListDirty;

        #region Update 模块轮询

        /// <summary>
        /// 模块轮询
        /// </summary>
        /// <param name="elapsedTime">逻辑时间间隔 秒为单位</param>
        /// <param name="realElapsedTime">真实时间间隔 秒为单位</param>
        public static void Update(float elapsedTime, float realElapsedTime)
        {
            if (s_isExecuteListDirty)
            {
                s_isExecuteListDirty = false;
                BuildUpdateExecuteList();
            }

            for (int i = 0; i < s_updateExecuteList.Count; i++)
            {
                s_updateExecuteList[i].Update(elapsedTime, realElapsedTime);
            }
        }

        private static void BuildUpdateExecuteList()
        {
            s_updateExecuteList.Clear();

            foreach (var updateModule in s_updateModules)
            {
                s_updateExecuteList.Add(updateModule as IUpdateModule);
            }
        }

        #endregion

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
            RegisterUpdateModule(module);
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

            s_moduleMaps[type] = module;
            RegisterUpdateModule(module);
            return module as T;
        }

        /// <summary>
        /// 注册可轮询模块
        /// </summary>
        private static void RegisterUpdateModule(Module module)
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

            Type interfaceType = typeof(IUpdateModule);
            bool implementsIUpdateModule = interfaceType.IsInstanceOfType(module);

            if (implementsIUpdateModule)
            {
                LinkedListNode<Module> currentUpdate = s_updateModules.First;
                while (currentUpdate != null)
                {
                    if (module.Priority > currentUpdate.Value.Priority)
                    {
                        break;
                    }
                    currentUpdate = currentUpdate.Next;
                }

                if (currentUpdate != null)
                {
                    s_updateModules.AddBefore(currentUpdate, module);
                }
                else
                {
                    s_updateModules.AddLast(module);
                }
                s_isExecuteListDirty = true;
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
            s_updateModules.Clear();
            s_updateExecuteList.Clear();
        }
    }
}
