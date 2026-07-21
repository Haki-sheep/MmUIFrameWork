using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MieMieUIFrameWork.UI
{
    /// <summary>
    /// UI 资源加载抽象 宿主可注入 AddressableMgr 等实现
    /// </summary>
    public interface IUIAssetLoader
    {
        /// <summary>
        /// 同步加载并实例化 UI
        /// </summary>
        GameObject Load(string uiName);

        /// <summary>
        /// 异步加载并实例化 UI
        /// </summary>
        UniTask<GameObject> LoadAsync(string uiName);

        /// <summary>
        /// 释放 UI 资源
        /// </summary>
        void Release(string uiName);
    }
}
