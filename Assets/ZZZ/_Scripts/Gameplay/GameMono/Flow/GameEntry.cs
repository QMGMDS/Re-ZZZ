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
        private RootModule _rootModule;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            _rootModule = new RootModule();
            _rootModule.Initialize();
        }

        private void Start()
        {
            ModuleSystem.GetModule<ISceneModule>().LoadScene(SceneNames.Gameplay);
        }

        private void Update()
        {
            _rootModule.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            _rootModule.Destroy();
        }
    }
}
