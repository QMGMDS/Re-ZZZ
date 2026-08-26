using GamePlay.Data;

using SPFramework;

namespace GamePlay.Contract
{
    /// <summary>
    /// 输入数据服务契约
    /// </summary>
    public interface IIputData : IService
    {
        RawInputData RawInputData { get; }
        CharacterInputData CharacterInputData { get; }
    }
}
