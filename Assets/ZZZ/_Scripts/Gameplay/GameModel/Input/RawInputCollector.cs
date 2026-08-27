using System;

using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.GameModel
{
    /// <summary>
    /// 原始输入数据采集器
    /// </summary>
    public sealed class RawInputCollector
    {
        /// <summary>
        /// 采集按键单次按下
        /// </summary>
        public bool CollectButton(InputActionReference inputReference)
        {
            if (inputReference == null)
            {
                throw new ArgumentNullException(nameof(inputReference));
            }

            return inputReference.action.WasPressedThisFrame();
        }

        /// <summary>
        /// 采集输入轴
        /// </summary>
        public Vector2 CollectAxis(InputActionReference inputReference)
        {
            if (inputReference == null)
            {
                throw new ArgumentNullException(nameof(inputReference));
            }

            return inputReference.action.ReadValue<Vector2>();
        }
    }
}
