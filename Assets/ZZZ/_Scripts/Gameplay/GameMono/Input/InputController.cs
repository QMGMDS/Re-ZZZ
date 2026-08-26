using System;

using UnityEngine;
using UnityEngine.InputSystem;

using GamePlay.Contract;
using GamePlay.Data;
using GamePlay.GameModel;
using SPFramework;

namespace GamePlay.GameMono
{
    /// <summary>
    /// 输入控制器
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InputController : MonoBehaviour, IIputData
    {
        [Header("输入绑定")]
        [SerializeField, Tooltip("角色移动输入引用")]
        private InputActionReference _moveInputReference;
        [SerializeField, Tooltip("角色攻击输入引用")]
        private InputActionReference _attackInputReference;

        [Header("输入处理")]
        [SerializeField, Min(0f), Tooltip("方向切换时的移动输入空窗容错秒数")]
        private float _moveInputGapToleranceSeconds = 0.05f;

        private RawInputCollector _rawInputCollector;
        private InputGapFilter _inputGapFilter;
        private RawInputData _rawInputData;
        private CharacterInputData _characterInputData;

        public RawInputData RawInputData => _rawInputData;
        public CharacterInputData CharacterInputData => _characterInputData;

        private void Awake()
        {
            if (_moveInputReference == null
                || _attackInputReference == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InputController)} 要求必须分配有效的输入引用");
            }

            _rawInputCollector = new RawInputCollector();
            _inputGapFilter = new InputGapFilter(_moveInputGapToleranceSeconds);
        }

        private void OnEnable()
        {
            _moveInputReference.action.Enable();
            _attackInputReference.action.Enable();

            _inputGapFilter.Reset();
            ServiceHub.Register<IIputData>(this);
        }

        private void Update()
        {
            _rawInputData = new RawInputData(
                _rawInputCollector.CollectButton(_attackInputReference),
                _rawInputCollector.CollectAxis(_moveInputReference));

            Vector2 normalizedMove = InputNormalization.NormalizeAxis(_rawInputData.Move);
            float deltaTime = Time.deltaTime;
            _characterInputData = new CharacterInputData(
                _rawInputData.Attack,
                _inputGapFilter.FilterAxis(normalizedMove, deltaTime));
        }

        private void OnDisable()
        {
            _moveInputReference.action.Disable();
            _attackInputReference.action.Disable();

            ServiceHub.Unregister<IIputData>(this);
        }
    }
}
