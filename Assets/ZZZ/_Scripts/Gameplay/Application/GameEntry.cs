using UnityEngine;

using SPFramework;
using ZZZ.Gameplay.Application.GameFlow;

namespace ZZZ.Gameplay.Application
{
    public sealed class GameEntry : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            ModuleSystem.RegisterModule<ISceneModule>(new SceneModule());
            ModuleSystem.RegisterModule<IGameFlow>(new GameFlowModule());
        }

        private void Update()
        {
            ModuleSystem.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            ModuleSystem.Destroy();
        }
    }
}
