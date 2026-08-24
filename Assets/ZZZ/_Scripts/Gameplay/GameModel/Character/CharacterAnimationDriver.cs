using System;

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

using GamePlay.Data;

namespace GamePlay.GameModule
{
    /// <summary>
    /// 根据角色动作及其逻辑时间手动采样动画
    /// </summary>
    public sealed class CharacterAnimationDriver : IDisposable
    {
        private PlayableGraph _graph;
        private AnimationPlayableOutput _output;
        private AnimationClipPlayable _clipPlayable;
        private AnimationClip _currentClip;

        public CharacterAnimationDriver(Animator animator)
        {
            _graph = PlayableGraph.Create(nameof(CharacterAnimationDriver));
            _graph.SetTimeUpdateMode(DirectorUpdateMode.Manual); // 手动控制
            _output = AnimationPlayableOutput.Create(_graph, "CharacterAnimation", animator);
            _graph.Play();
        }

        /// <summary>
        /// 将当前动作直接采样到指定的逻辑秒数
        /// </summary>
        public void Evaluate(CharacterActionAsset currentAction, float logicalProgressSeconds)
        {
            if (currentAction == null)
            {
                return;
            }

            AnimationClip animationClip = currentAction.AnimationClip;

            if (_currentClip != animationClip)
            {
                Bind(animationClip);
            }

            double normalizedProgress = (double)logicalProgressSeconds / currentAction.DurationSeconds;
            _clipPlayable.SetTime(normalizedProgress * animationClip.length);

            _graph.Evaluate(0f);
        }

        private void Bind(AnimationClip animationClip)
        {
            AnimationClipPlayable nextPlayable = AnimationClipPlayable.Create(_graph, animationClip);
            nextPlayable.SetSpeed(0d);
            _output.SetSourcePlayable(nextPlayable);

            if (_clipPlayable.IsValid())
            {
                _graph.DestroyPlayable(_clipPlayable);
            }

            _clipPlayable = nextPlayable;
            _currentClip = animationClip;
        }

        public void Dispose()
        {
            _graph.Destroy();
        }
    }
}
