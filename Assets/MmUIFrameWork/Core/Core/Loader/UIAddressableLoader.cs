using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MieMieUIFrameWork.UI
{
    /// <summary>
    /// 默认加载器 走抽离的 UIAddressable
    /// </summary>
    public sealed class UIAddressableLoader : IUIAssetLoader
    {
        /// <summary>
        /// 地址到实例
        /// </summary>
        private readonly Dictionary<string, GameObject> instanceDict = new();

        /// <summary>
        /// 同步加载并实例化 UI
        /// </summary>
        public GameObject Load(string uiName)
        {
            Release(uiName);
            GameObject go = UIAddressable.LoadGameObject(uiName);
            if (go != null)
                instanceDict[uiName] = go;
            return go;
        }

        /// <summary>
        /// 异步加载并实例化 UI
        /// </summary>
        public async UniTask<GameObject> LoadAsync(string uiName)
        {
            Release(uiName);
            GameObject go = await UIAddressable.LoadGameObjectAsync(uiName);
            if (go != null)
                instanceDict[uiName] = go;
            return go;
        }

        /// <summary>
        /// 释放 UI 资源
        /// </summary>
        public void Release(string uiName)
        {
            if (!instanceDict.TryGetValue(uiName, out GameObject go))
                return;

            UIAddressable.DestroyObject(go);
            instanceDict.Remove(uiName);
        }
    }
}
