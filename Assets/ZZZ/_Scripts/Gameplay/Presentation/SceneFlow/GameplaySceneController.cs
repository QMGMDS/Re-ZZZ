using UnityEngine;

using SPFramework;
using ZZZ.Gameplay.Application.GameFlow;

namespace ZZZ.Gameplay.Presentation.SceneFlow
{
    public sealed class GameplaySceneController : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                ModuleSystem.GetModule<IGameFlow>().EnterMainMenu();
            }
        }
    }
}
