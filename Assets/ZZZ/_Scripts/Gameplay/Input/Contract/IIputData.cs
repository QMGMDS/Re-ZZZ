using GamePlay.Input;

using SPFramework;

namespace GamePlay.Input.Contract
{
    /// <summary>
    /// 输入数据服务契约
    /// </summary>
    public interface IIputData : IService
    {
        RawInputData RawInputData { get; }
        CharacterInputData CharacterInputData { get; }

        /// <summary>
        /// 采集一次宿主帧输入
        /// </summary>
        void Capture(float elapsedSeconds);

        /// <summary>
        /// 消费一次逻辑 Tick 输入
        /// </summary>
        CharacterInputData ConsumeCharacterInput();
    }
}
