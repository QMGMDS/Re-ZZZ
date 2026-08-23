using SPFramework;

namespace ZZZ.Gameplay.Application.GameFlow
{
    public sealed class GameFlowModule : Module, IGameFlow
    {
        private const string MainMenuSceneName = "主菜单";
        private const string GameplaySceneName = "游戏主场景";

        private ISceneModule _sceneModule;

        public override void OnCreate()
        {
            _sceneModule = ModuleSystem.GetModule<ISceneModule>();
        }

        public override void OnDestroy()
        {
        }

        public void EnterMainMenu()
        {
            _sceneModule.LoadScene(MainMenuSceneName);
        }

        public void StartGame()
        {
            _sceneModule.LoadScene(GameplaySceneName);
        }

        public void QuitGame()
        {
            UnityEngine.Application.Quit();
        }
    }
}
