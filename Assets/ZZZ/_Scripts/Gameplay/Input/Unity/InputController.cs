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

        private RawInputController _rawInputController;
        private CharacterInputProcessor _characterInputProcessor;
        private RawInputData _rawInputData;
        private CharacterInputData _characterInputData;
        private bool _isInitialized;
        private bool _isServiceRegistered;

        public RawInputData RawInputData => _rawInputData;
        public CharacterInputData CharacterInputData => _characterInputData;

        private void Awake()
        {
            if (_inputConfigAsset == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InputController)} 必须配置 {nameof(_inputConfigAsset)}");
            }

            _inputConfigAsset.Validate();
            _rawInputController = new RawInputController(_inputConfigAsset);
            _characterInputProcessor = new CharacterInputProcessor(_inputConfigAsset);
            _isInitialized = true;
        }

        private void OnEnable()
        {
            EnsureInitialized();

            _characterInputProcessor.Reset();
            SetInputActionsEnabled(true);
            ServiceHub.Register<IInputService>(this);
            _isServiceRegistered = true;
        }

        /// <inheritdoc/>
        public void InputCapture()
        {
            _rawInputController.GetRawInputData(ref _rawInputData);
            _characterInputProcessor.GetCharacterInput(
                in _rawInputData,
                ref _characterInputData);
        }

        private void OnDisable()
        {
            if (_isServiceRegistered)
            {
                ServiceHub.Unregister<IInputService>(this);
                _isServiceRegistered = false;
            }

            if (!_isInitialized)
            {
                return;
            }

            SetInputActionsEnabled(false);
            _characterInputProcessor.Reset();
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(InputController)} 尚未初始化");
            }
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
