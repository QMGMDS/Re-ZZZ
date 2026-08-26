using UnityEngine;

using SPFramework;

namespace GamePlay.Contract
{
    /// <summary>
    /// 摄像机服务契约
    /// </summary>
    public interface ICameraService : IService
    {
        /// <summary>
        /// 将输入方向转换为世界坐标中的方向
        /// </summary>
        Vector2 ConvertToWorldCoordinate(Vector2 input);

        /// <summary>
        /// 设置跟随目标物体
        /// </summary>
        void SetTargetObject(Transform targetObject);
    }
}
