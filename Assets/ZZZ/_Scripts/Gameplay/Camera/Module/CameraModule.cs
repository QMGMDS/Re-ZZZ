using System;

using GamePlay.Camera.Contract;

namespace GamePlay.Camera
{
    /// <summary>
    /// 统一驱动当前激活摄像机实例
    /// </summary>
    public sealed class CameraModule : ICameraModule
    {
        private ICameraUpdateTarget _target;

        /// <inheritdoc/>
        public void Register(ICameraUpdateTarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (_target != null)
            {
                throw new InvalidOperationException("摄像机更新目标不能重复注册");
            }

            _target = target;
        }

        /// <inheritdoc/>
        public void Unregister(ICameraUpdateTarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (_target != target)
            {
                throw new InvalidOperationException("摄像机更新目标尚未注册");
            }

            _target = null;
        }

        /// <inheritdoc/>
        public void RenderUpdate(float deltaTimeSeconds)
        {
            if (_target == null)
            {
                return;
            }

            _target.RenderUpdate(deltaTimeSeconds);
        }
    }
}
