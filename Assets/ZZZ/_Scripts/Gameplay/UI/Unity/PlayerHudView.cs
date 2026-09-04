using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;
using PlayerCharacterInfo = GamePlay.Character.CharacterInfo;

namespace GamePlay.UI
{
    [DisallowMultipleComponent]
    public sealed class PlayerHudView : MonoBehaviour
    {
        [Header("表现配置")]
        [SerializeField, Tooltip("玩家 HUD 表现配置")]
        private PlayerHudConfigAsset _configAsset;

        [Header("头像配置")]
        [SerializeField, Tooltip("队伍槽位一头像")]
        private Image _slot1PortraitImage;
        [SerializeField, Tooltip("队伍槽位二头像")]
        private Image _slot2PortraitImage;
        [SerializeField, Tooltip("队伍槽位三头像")]
        private Image _slot3PortraitImage;

        [Header("生命条配置")]
        [SerializeField, Tooltip("主槽绿色生命条")]
        private Image _mainGreenHealthImage;
        [SerializeField, Tooltip("主槽红色延迟生命条")]
        private Image _mainRedTrailHealthImage;
        [SerializeField, Tooltip("副槽二绿色生命条")]
        private Image _slot2GreenHealthImage;
        [SerializeField, Tooltip("副槽三绿色生命条")]
        private Image _slot3GreenHealthImage;

        [SerializeField, Tooltip("主槽生命文本")]
        private TMP_Text _mainHealthText;

        private PlayerCharacterInfo _displayedActiveCharacter;
        private float _displayedMainHealthRatio;
        private Coroutine _redTrailCoroutine;

        private void Awake()
        {
            if (_configAsset == null
            || _slot1PortraitImage == null
            || _slot2PortraitImage == null
            || _slot3PortraitImage == null
            || _mainGreenHealthImage == null
            || _mainRedTrailHealthImage == null
            || _slot2GreenHealthImage == null
            || _slot3GreenHealthImage == null
            || _mainHealthText == null)
            {
                throw new InvalidOperationException($"{nameof(PlayerHudView)} 检查配置");
            }
        }

        /// <summary>
        /// 刷新玩家 HUD
        /// </summary>
        public void Refresh(IReadOnlyList<PlayerCharacterInfo> team, int activeCharacterIndex)
        {
            RefreshMainSlot(team[activeCharacterIndex]);
            RefreshSecondarySlot(
                _slot2PortraitImage,
                _slot2GreenHealthImage,
                team,
                activeCharacterIndex,
                1);
            RefreshSecondarySlot(
                _slot3PortraitImage,
                _slot3GreenHealthImage,
                team,
                activeCharacterIndex,
                2);
        }

        private void RefreshMainSlot(PlayerCharacterInfo characterInfo)
        {
            bool activeCharacterChanged = !ReferenceEquals(_displayedActiveCharacter, characterInfo);
            float healthRatio = GetHealthRatio(characterInfo);

            _slot1PortraitImage.sprite = characterInfo.CharacterAvatar;
            _mainGreenHealthImage.fillAmount = healthRatio;
            _mainHealthText.text = $"{characterInfo.CurrentHealth}/{characterInfo.BaseHealth}";

            if (activeCharacterChanged || healthRatio > _displayedMainHealthRatio)
            {
                StopRedTrail();
                _mainRedTrailHealthImage.fillAmount = healthRatio;
            }
            else if (healthRatio < _displayedMainHealthRatio)
            {
                RestartRedTrail(healthRatio);
            }

            _displayedActiveCharacter = characterInfo;
            _displayedMainHealthRatio = healthRatio;
        }

        private static void RefreshSecondarySlot(
            Image portraitImage,
            Image healthImage,
            IReadOnlyList<PlayerCharacterInfo> team,
            int activeCharacterIndex,
            int cyclicOffset)
        {
            bool hasCharacter = cyclicOffset < team.Count;
            portraitImage.gameObject.SetActive(hasCharacter);
            healthImage.gameObject.SetActive(hasCharacter);

            if (!hasCharacter)
            {
                return;
            }

            int characterIndex = (activeCharacterIndex + cyclicOffset) % team.Count;
            PlayerCharacterInfo characterInfo = team[characterIndex];
            portraitImage.sprite = characterInfo.CharacterAvatar;
            healthImage.fillAmount = GetHealthRatio(characterInfo);
        }

        private static float GetHealthRatio(PlayerCharacterInfo characterInfo)
        {
            return Mathf.Clamp01((float)characterInfo.CurrentHealth / characterInfo.BaseHealth);
        }

        private void RestartRedTrail(float targetHealthRatio)
        {
            StopRedTrail();
            _redTrailCoroutine = StartCoroutine(ChaseRedTrail(targetHealthRatio));
        }

        private void StopRedTrail()
        {
            if (_redTrailCoroutine == null)
            {
                return;
            }

            StopCoroutine(_redTrailCoroutine);
            _redTrailCoroutine = null;
        }

        private IEnumerator ChaseRedTrail(float targetHealthRatio)
        {
            if (_configAsset.RedTrailDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(_configAsset.RedTrailDelaySeconds);
            }

            while (_mainRedTrailHealthImage.fillAmount > targetHealthRatio)
            {
                _mainRedTrailHealthImage.fillAmount = Mathf.MoveTowards(
                    _mainRedTrailHealthImage.fillAmount,
                    targetHealthRatio,
                    _configAsset.RedTrailCatchUpSpeedPerSecond * Time.deltaTime);
                yield return null;
            }

            _redTrailCoroutine = null;
        }
    }
}
