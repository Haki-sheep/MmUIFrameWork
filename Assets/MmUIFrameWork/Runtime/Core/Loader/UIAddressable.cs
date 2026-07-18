using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MieMieFrameWork.UI
{
    /// <summary>
    /// UI 专用 Addressable 最小加载集
    /// 从 AddressableMgr 抽离 LoadGameObject LoadGameObjectAsync DestroyObject
    /// 不依赖 ModuleHub 与对象池
    /// </summary>
    public static class UIAddressable
    {
        /// <summary>
        /// 是否已初始化
        /// </summary>
        private static bool ready;

        /// <summary>
        /// 初始化任务源
        /// </summary>
        private static UniTaskCompletionSource readyTcs;

        /// <summary>
        /// 同步实例化 UI 用完 DestroyObject
        /// </summary>
        public static GameObject LoadGameObject(string address, Transform parent = null)
        {
            EnsureReadySync();
            var handle = Addressables.InstantiateAsync(address, parent);
            GameObject result = handle.WaitForCompletion();
            if (result != null)
            {
                PrepareInstance(result);
                return result;
            }

            Debug.LogError($"[UIAddressable] 无法实例化 {address}");
            if (handle.IsValid())
                Addressables.Release(handle);
            return null;
        }

        /// <summary>
        /// 异步实例化 UI 用完 DestroyObject
        /// </summary>
        public static async UniTask<GameObject> LoadGameObjectAsync(string address, Transform parent = null)
        {
            await EnsureReadyAsync();
            var handle = Addressables.InstantiateAsync(address, parent);
            await UniTask.WaitUntil(() => handle.IsDone);
            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogError($"[UIAddressable] 异步实例化失败 {address}");
                if (handle.IsValid())
                    Addressables.Release(handle);
                return null;
            }

            PrepareInstance(handle.Result);
            return handle.Result;
        }

        /// <summary>
        /// 销毁实例 先 ReleaseInstance 失败再 Destroy
        /// </summary>
        public static void DestroyObject(GameObject obj)
        {
            if (obj == null)
                return;

            if (Addressables.ReleaseInstance(obj))
                return;

            Object.Destroy(obj);
        }

        /// <summary>
        /// 同步确保 Addressables 就绪
        /// </summary>
        private static void EnsureReadySync()
        {
            EnsureReadyAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 异步确保 Addressables 就绪
        /// </summary>
        private static async UniTask EnsureReadyAsync()
        {
            if (ready)
                return;

            if (readyTcs == null)
            {
                readyTcs = new UniTaskCompletionSource();
                var init = Addressables.InitializeAsync();
                await UniTask.WaitUntil(() => init.IsDone);
                if (init.Status != AsyncOperationStatus.Succeeded)
                    Debug.LogError("[UIAddressable] Addressables 初始化失败");
                ready = true;
                readyTcs.TrySetResult();
            }
            else
            {
                await readyTcs.Task;
            }
        }

        /// <summary>
        /// 去掉名字里的 Clone 后缀
        /// </summary>
        private static void PrepareInstance(GameObject go)
        {
            go.name = go.name.Replace("(Clone)", "");
        }
    }
}
