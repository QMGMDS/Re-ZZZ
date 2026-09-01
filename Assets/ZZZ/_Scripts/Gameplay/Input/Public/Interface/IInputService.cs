using GamePlay.Input;
using SPFramework;

namespace GamePlay.Input.Public
{
    /// <summary>
    /// 输入服务契约
    /// </summary>
    public interface IInputService : IService
    {
        RawInputData RawInputData { get; }
        CharacterInputData CharacterInputData { get; }

        /// <summary>
        /// 采集一次当前输入
        /// </summary>
        void InputCapture();
    }
}
