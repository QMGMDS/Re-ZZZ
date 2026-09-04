using UnityEngine;

namespace GamePlay.UI
{
    [CreateAssetMenu(fileName = "PlayerHudConfig", menuName = "ZZZ/UI/玩家 HUD 配置")]
    public sealed class PlayerHudConfigAsset : ScriptableObject
    {
        [SerializeField, Min(0f), Tooltip("红色生命条开始追赶前的等待时间（秒）")]
        private float _redTrailDelaySeconds = 0.4f;
        [SerializeField, Min(0.0001f), Tooltip("红色生命条每秒追赶的归一化填充值")]
        private float _redTrailCatchUpSpeedPerSecond = 0.5f;

        public float RedTrailDelaySeconds => _redTrailDelaySeconds;
        public float RedTrailCatchUpSpeedPerSecond => _redTrailCatchUpSpeedPerSecond;
    }
}
