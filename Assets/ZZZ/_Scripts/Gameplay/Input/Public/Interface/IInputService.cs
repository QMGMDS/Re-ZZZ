using SPFramework;

namespace GamePlay.Input.Public
{
    /// <summary>
    /// 输入服务契约
    /// </summary>
    public interface IInputService : IService
    {
        /// <summary>原始输入数据</summary>
        RawInputData RawInputData { get; }

        /// <summary>角色特供输入数据</summary>
        CharacterInputData CharacterInputData { get; }

        /// <summary>采集一次当前输入</summary>
        void InputCapture();
    }
}
