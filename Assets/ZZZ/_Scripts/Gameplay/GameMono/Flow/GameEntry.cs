using UnityEngine;

using SPFramework;
using GamePlay.GameModule;
using GamePlay.Data;

namespace GamePlay.GameMono
{
    /// <summary>
    /// 游戏启动引导
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public sealed class GameEntry : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            ModuleSystem.RegisterModule<IEntityModule>(new EntityModule());
            ModuleSystem.RegisterModule<ICombatModule>(new CombatModule());
            ModuleSystem.RegisterModule<ISceneModule>(new SceneModule());
        }

        private void Start()
        {
            ModuleSystem.GetModule<ISceneModule>().LoadScene(SceneNames.Gameplay);
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
