using GamePlay.Data;

namespace GamePlay.Contract
{
    /// <summary>
    /// 接收角色输入数据
    /// </summary>
    public interface IInputCharacter
    {
        void InputCharacter(InputCharacterData inputCharacterData);
    }
}
