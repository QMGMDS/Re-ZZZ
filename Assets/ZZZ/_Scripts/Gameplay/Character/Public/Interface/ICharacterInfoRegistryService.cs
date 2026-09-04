using SPFramework;

namespace GamePlay.Character.Public
{
    /// <summary>角色信息注册服务契约</summary>
    public interface ICharacterInfoRegistryService : IService
    {
        /// <summary>
        /// 注册角色信息
        /// </summary>
        /// <param name="characterInfoAsset">角色信息配置</param>
        /// <returns>注册完成的角色信息</returns>
        CharacterInfo RegisterCharacterInfo(CharacterInfoAsset characterInfoAsset);
    }
}
