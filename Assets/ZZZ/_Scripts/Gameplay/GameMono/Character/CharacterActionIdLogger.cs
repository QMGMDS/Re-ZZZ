using UnityEngine;

namespace GamePlay.GameMono
{
    /// <summary>
    /// 每帧打印角色当前动作 ID
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterActionController))]
    public sealed class CharacterActionIdLogger : MonoBehaviour
    {
        private CharacterActionController _characterActionController;

        private void Awake()
        {
            _characterActionController = GetComponent<CharacterActionController>();
        }

        private void LateUpdate()
        {
            Debug.Log($"{name} 当前动作 ID {_characterActionController.CurrentActionId}");
        }
    }
}
