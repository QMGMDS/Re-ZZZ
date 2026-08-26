using System;

using GamePlay.Contract;
using GamePlay.Data;
using GamePlay.GameMono;
using SPFramework;

namespace GamePlay.GameModule
{
    /// <summary>
    /// 处理角色攻击请求
    /// </summary>
    public sealed class CombatModule : Module, ICombatModule
    {
        private IEntityModule _entityModule;

        public override void OnCreate()
        {
            _entityModule = ModuleSystem.GetModule<IEntityModule>();
        }

        public override void OnDestroy()
        {
            _entityModule = null;
        }

        /// <inheritdoc/>
        public void SubmitAttack(AttackRequest attackRequest)
        {
            if (!_entityModule.TryGetEntity(attackRequest.AttackerId, out Entity attackerEntity)
                || !_entityModule.TryGetEntity(attackRequest.TargetId, out Entity targetEntity))
            {
                return;
            }

            CharacterInfoController attackerInfo =
                attackerEntity.GetComponent<CharacterInfoController>();
            CharacterInfoController targetInfo =
                targetEntity.GetComponent<CharacterInfoController>();

            // Entity 也可能被其他非角色对象使用 此时不是角色攻击业务
            if (attackerInfo == null || targetInfo == null)
            {
                return;
            }

            CharacterInfoRuntime attackerRuntime = attackerInfo.Runtime;
            CharacterInfoRuntime targetRuntime = targetInfo.Runtime;
            if (attackerRuntime == null || targetRuntime == null)
            {
                throw new InvalidOperationException(
                    "攻击请求涉及的角色信息运行时数据尚未初始化");
            }

            if (attackerRuntime.Faction == targetRuntime.Faction)
            {
                return;
            }

            targetInfo.ReceiveAttack(attackRequest);
        }
    }
}
