using System;

using UnityEngine;

namespace GamePlay.Input
{
    /// <summary>
    /// 角色输入处理器
    /// </summary>
    internal sealed class CharacterInputProcessor
    {
        private readonly InputConfigAsset _inputConfigAsset;
        private float _zeroElapsedSeconds;
        private Vector2 _lastValidMove;

        internal CharacterInputProcessor(InputConfigAsset inputConfigAsset)
        {
            if (inputConfigAsset == null)
            {
                throw new ArgumentNullException(nameof(inputConfigAsset));
            }

            _inputConfigAsset = inputConfigAsset;
        }

        /// <summary>
        /// 将原始输入处理为角色输入
        /// </summary>
        internal void GetCharacterInput(in RawInputData rawInputData, ref CharacterInputData characterInputData)
        {
            Vector2 normalizedMove = NormalizeAxis(rawInputData.Move);

            characterInputData = new CharacterInputData(
                FilterMove(normalizedMove),
                rawInputData.Attack,
                rawInputData.Evade,
                rawInputData.Skill,
                rawInputData.Ultimate,
                rawInputData.Switch);
        }

        /// <summary>
        /// 清除当前处理状态
        /// </summary>
        internal void Reset()
        {
            _zeroElapsedSeconds = 0f;
            _lastValidMove = Vector2.zero;
        }

        private Vector2 FilterMove(Vector2 input)
        {
            float deltaTimeSeconds = Time.deltaTime;

            if (deltaTimeSeconds < 0f
                || float.IsNaN(deltaTimeSeconds)
                || float.IsInfinity(deltaTimeSeconds))
            {
                throw new InvalidOperationException($"{nameof(CharacterInputProcessor)} 读取到无效的帧间隔");
            }

            if (input.sqrMagnitude != 0f)
            {
                _lastValidMove = input;
                _zeroElapsedSeconds = 0f;
                return input;
            }

            if (_lastValidMove.sqrMagnitude == 0f)
            {
                return Vector2.zero;
            }

            _zeroElapsedSeconds += deltaTimeSeconds;
            if (_inputConfigAsset.MoveInputGapToleranceSeconds > 0f
                && _zeroElapsedSeconds <= _inputConfigAsset.MoveInputGapToleranceSeconds)
            {
                return _lastValidMove;
            }

            _lastValidMove = Vector2.zero;
            _zeroElapsedSeconds = 0f;
            return Vector2.zero;
        }

        private static Vector2 NormalizeAxis(Vector2 input)
        {
            if (input.sqrMagnitude == 0f)
            {
                return Vector2.zero;
            }

            return Vector2.ClampMagnitude(input, 1f);
        }
    }
}
