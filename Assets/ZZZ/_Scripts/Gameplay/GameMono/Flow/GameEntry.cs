using UnityEngine;

using SPFramework;
using GamePlay.GameModule;
using GamePlay.Data;

namespace GamePlay.GameMono
{
    /// <summary>
    /// 游戏启动引导
    /// </summary>
    public sealed class GameEntry : MonoBehaviour
    {
        private RootModule _rootModule;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            _rootModule = new RootModule();
        }

        private void Start()
        {
            _rootModule.Initialize();

            ModuleSystem.GetModule<ISceneModule>().LoadScene(SceneNames.MainMenu);
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
