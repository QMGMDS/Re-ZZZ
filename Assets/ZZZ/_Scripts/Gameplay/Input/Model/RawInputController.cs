using System;

using UnityEngine;

namespace GamePlay.Input
{
    /// <summary>
    /// 原始输入采集器
    /// </summary>
    internal sealed class RawInputController
    {
        private readonly InputConfigAsset _inputConfigAsset;

        internal RawInputController(InputConfigAsset inputConfigAsset)
        {
            if (inputConfigAsset == null)
            {
                throw new ArgumentNullException(nameof(inputConfigAsset));
            }

            _inputConfigAsset = inputConfigAsset;
        }

        /// <summary>
        /// 采集当前原始输入
        /// </summary>
        internal void GetRawInputData(ref RawInputData rawInputData)
        {
            rawInputData = new RawInputData(
                _inputConfigAsset.MoveInputReference.action.ReadValue<Vector2>(),
                _inputConfigAsset.AttackInputReference.action.WasPressedThisFrame(),
                _inputConfigAsset.EvadeInputReference.action.WasPressedThisFrame(),
                _inputConfigAsset.SkillInputReference.action.WasPressedThisFrame(),
                _inputConfigAsset.UltimateInputReference.action.WasPressedThisFrame(),
                _inputConfigAsset.SwitchInputReference.action.WasPressedThisFrame());
        }
    }
}
