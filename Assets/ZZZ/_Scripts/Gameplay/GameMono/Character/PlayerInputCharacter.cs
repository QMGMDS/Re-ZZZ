using System;

using UnityEngine;
using UnityEngine.InputSystem;

using GamePlay.Contract;
using GamePlay.Data;

namespace GamePlay.GameMono
{
    /// <summary>
    /// 每帧向同一物体上的角色控制器写入玩家输入
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerInputCharacter : MonoBehaviour
    {
        [SerializeField, Tooltip("角色移动输入")] private InputActionReference _moveAction;
        [SerializeField, Tooltip("角色攻击输入")] private InputActionReference _attackAction;

        private IInputCharacter _inputCharacter;

        private void Awake()
        {
            _inputCharacter = GetComponent<IInputCharacter>();

            if (_attackAction == null || _moveAction == null)
            {
                throw new InvalidOperationException($"{nameof(PlayerInputCharacter)} 要求必须分配 {nameof(_attackAction)} 和 {nameof(_moveAction)}");
            }

            _attackAction.action.Enable();
            _moveAction.action.Enable();
        }

        private void Update()
        {
            var inputCharacterData = ConvertToInputCharacterData(_attackAction, _moveAction);

            _inputCharacter.InputCharacter(inputCharacterData);
        }

        private static InputCharacterData ConvertToInputCharacterData(
            InputActionReference attackAction,
            InputActionReference moveAction)
        {
            Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
            Trilean attack = attackAction.action.IsPressed() ? Trilean.True : Trilean.False;
            Trilean move = moveInput.sqrMagnitude > 0f ? Trilean.True : Trilean.False;

            var intention = new CharacterIntention(attack, move);
            return new InputCharacterData(Time.time, intention, moveInput);
        }
    }
}
