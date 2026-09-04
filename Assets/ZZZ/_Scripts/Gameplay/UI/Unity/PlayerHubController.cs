using System;

using UnityEngine;

using SPFramework;
using GamePlay.Character.Public;

namespace GamePlay.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerHudView))]
    public sealed class PlayerHubController : MonoBehaviour
    {
        // 依赖表现层
        private PlayerHudView _playerHudView;

        // 服务接口
        private IPlayerTeamService _playerTeamService;

        // 事件退订句柄
        private IDisposable _characterSwitchedSubscription;

        private void Awake()
        {
            _playerHudView = GetComponent<PlayerHudView>();
        }

        private void OnEnable()
        {
            _characterSwitchedSubscription = EventBus.Subscribe<CharacterSwitchedEvent>(OnCharacterSwitched);
        }

        private void OnDisable()
        {
            _characterSwitchedSubscription.Dispose();
        }

        private void Start()
        {
            if (!ServiceHub.TryGet<IPlayerTeamService>(out IPlayerTeamService playerTeamService))
            {
                throw new InvalidOperationException($"{nameof(PlayerHubController)} 未获取到 {nameof(IPlayerTeamService)}");
            }
            _playerTeamService = playerTeamService;

            _playerHudView.Refresh(_playerTeamService.CharacterInfos, _playerTeamService.CurrentActiveCharacterIndex);
        }

        #region 事件回调

        private void OnCharacterSwitched(CharacterSwitchedEvent characterSwitchedEvent)
        {
            _playerHudView.Refresh(_playerTeamService.CharacterInfos, _playerTeamService.CurrentActiveCharacterIndex);
        }

        #endregion
    }
}
