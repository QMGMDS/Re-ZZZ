using System;

using UnityEngine;

using GamePlay.Input.Public;
using SPFramework;

namespace GamePlay.Input
{
    /// <summary>
    /// 输入控制器
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InputController : MonoBehaviour, IInputService
    {
        [Header("输入配置")]
        [SerializeField, Tooltip("输入模块静态配置资产")]
        private InputConfigAsset _inputConfigAsset;

        // 私有依赖的逻辑模型
        private RawInputController _rawInputController;
        private CharacterInputProcessor _characterInputProcessor;

        // 内部持有数据
        private RawInputData _rawInputData;
        private CharacterInputData _characterInputData;

        /// <inheritdoc/>
        public RawInputData RawInputData => _rawInputData;
        /// <inheritdoc/>
        public CharacterInputData CharacterInputData => _characterInputData;

        private void Awake()
        {
            if (_inputConfigAsset == null)
            {
                throw new InvalidOperationException($"{nameof(InputController)} 必须配置 {nameof(_inputConfigAsset)}");
            }

            _inputConfigAsset.Validate();

            _rawInputController = new RawInputController(_inputConfigAsset);
            _characterInputProcessor = new CharacterInputProcessor(_inputConfigAsset);
        }

        private void OnEnable()
        {
            SetInputActionsEnabled(true);
            _characterInputProcessor.Reset();

            ServiceHub.Register<IInputService>(this);
        }

        private void OnDisable()
        {
            SetInputActionsEnabled(false);
            _characterInputProcessor.Reset();

            ServiceHub.Unregister<IInputService>(this);
        }

        /// <inheritdoc/>
        public void InputCapture()
        {
            _rawInputController.GetRawInputData(ref _rawInputData);
            _characterInputProcessor.GetCharacterInput(in _rawInputData, ref _characterInputData);
        }

        private void SetInputActionsEnabled(bool isEnabled)
        {
            if (isEnabled)
            {
                _inputConfigAsset.MoveInputReference.action.Enable();
                _inputConfigAsset.AttackInputReference.action.Enable();
                _inputConfigAsset.EvadeInputReference.action.Enable();
                _inputConfigAsset.SkillInputReference.action.Enable();
                _inputConfigAsset.UltimateInputReference.action.Enable();
                _inputConfigAsset.SwitchInputReference.action.Enable();
                return;
            }

            _inputConfigAsset.MoveInputReference.action.Disable();
            _inputConfigAsset.AttackInputReference.action.Disable();
            _inputConfigAsset.EvadeInputReference.action.Disable();
            _inputConfigAsset.SkillInputReference.action.Disable();
            _inputConfigAsset.UltimateInputReference.action.Disable();
            _inputConfigAsset.SwitchInputReference.action.Disable();
        }
    }
}
