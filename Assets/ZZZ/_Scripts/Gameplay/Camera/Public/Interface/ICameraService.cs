using UnityEngine;

using SPFramework;

namespace GamePlay.Camera.Public
{
    /// <summary>
    /// 摄像机服务契约
    /// </summary>
    public interface ICameraService : IService
    {
        /// <summary>将输入方向转换为世界坐标中的方向</summary>
        Vector2 ConvertToWorldCoordinate(Vector2 input);

        /// <summary>执行一次摄像机更新</summary>
        void CameraUpdate();
    }
}
