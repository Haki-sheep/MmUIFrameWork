using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MieMieFrameWork.UI
{
    /// <summary>
    /// 默认用 Addressables 直接实例化的加载器
    /// </summary>
    public sealed class AddressablesUIAssetLoader : IUIAssetLoader
    {
        /// <summary>
        /// 地址到实例句柄
        /// </summary>
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> handleDict = new();

        /// <summary>
        /// 同步加载并实例化 UI
        /// </summary>
        public GameObject Load(string uiName)
        {
            Release(uiName);

            var handle = Addressables.InstantiateAsync(uiName);
            GameObject go = handle.WaitForCompletion();
            if (go == null)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
                return null;
            }

            handleDict[uiName] = handle;
            return go;
        }

        /// <summary>
        /// 异步加载并实例化 UI
        /// </summary>
        public async UniTask<GameObject> LoadAsync(string uiName)
        {
            Release(uiName);

            var handle = Addressables.InstantiateAsync(uiName);
            await UniTask.WaitUntil(() => handle.IsDone);
            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
                return null;
            }

            handleDict[uiName] = handle;
            return handle.Result;
        }

        /// <summary>
        /// 释放 UI 资源
        /// </summary>
        public void Release(string uiName)
        {
            if (!handleDict.TryGetValue(uiName, out var handle))
                return;

            if (handle.IsValid())
                Addressables.ReleaseInstance(handle);

            handleDict.Remove(uiName);
        }
    }
}
