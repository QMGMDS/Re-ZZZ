using System;
using System.Collections.Generic;

namespace SPFramework
{
    /// <summary>
    /// 服务标记接口 - 统一约束服务，供服务中心按契约类型存储
    /// </summary>
    public interface IService { }

    /// <summary>
    /// 服务中心 - 以契约接口类型为键，注册与获取服务
    /// </summary>
    public static class ServiceHub
    {
        private static readonly Dictionary<Type, IService> ServiceDict = new();

        /// <summary>
        /// 注册服务
        /// </summary>
        public static void Register<T>(T service) where T : class, IService
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            Type serviceType = typeof(T);

            if (ServiceDict.ContainsKey(serviceType))
            {
                throw new InvalidOperationException(
                    $"[ServiceHub] {serviceType.Name} 已注册服务 不能重复注册");
            }

            ServiceDict.Add(serviceType, service);
        }

        /// <summary>
        /// 注销服务
        /// </summary>
        public static void Unregister<T>(T service) where T : class, IService
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            Type serviceType = typeof(T);

            if (!ServiceDict.TryGetValue(serviceType, out IService current))
            {
                throw new InvalidOperationException(
                    $"[ServiceHub] {serviceType.Name} 注销失败 - 该契约未注册服务，请检查注册与注销是否成对且契约类型一致");
            }

            if (!ReferenceEquals(current, service))
            {
                throw new InvalidOperationException(
                    $"[ServiceHub] {serviceType.Name} 注销失败 - 当前注册 {current} 不是传入实例 {service}");
            }

            ServiceDict.Remove(serviceType);
        }

        /// <summary>
        /// 尝试获取服务
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class, IService
        {
            service = null;

            Type serviceType = typeof(T);

            if (!ServiceDict.TryGetValue(serviceType, out IService raw))
                return false;

            if (IsDestroyed(raw))
            {
                ServiceDict.Remove(serviceType);
                return false;
            }

            service = (T)raw;
            return true;
        }

        /// <summary>
        /// 清空全部已注册服务
        /// </summary>
        public static void Clear()
        {
            ServiceDict.Clear();
        }

        private static bool IsDestroyed(IService service)
        {
            return service is UnityEngine.Object unityObject && unityObject == null;
        }
    }
}
