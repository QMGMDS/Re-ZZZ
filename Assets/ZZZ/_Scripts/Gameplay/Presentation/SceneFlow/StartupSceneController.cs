using UnityEngine;

using SPFramework;
using ZZZ.Gameplay.Application.GameFlow;

namespace ZZZ.Gameplay.Presentation.SceneFlow
{
    public sealed class StartupSceneController : MonoBehaviour
    {
        private void Start()
        {
            ModuleSystem.GetModule<IGameFlow>().EnterMainMenu();
        }
    }
}
