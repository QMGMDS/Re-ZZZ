using UnityEngine;

using SPFramework;
using ZZZ.Gameplay.Application.GameFlow;

namespace ZZZ.Gameplay.Presentation.SceneFlow
{
    public sealed class MainMenuController : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                ModuleSystem.GetModule<IGameFlow>().StartGame();
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                ModuleSystem.GetModule<IGameFlow>().QuitGame();
            }
        }
    }
}
