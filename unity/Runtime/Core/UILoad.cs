using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MieMieFrameWork.UI
{
    /// <summary>
    /// UI 加载工具类
    /// </summary>
    public static class UILoad
    {
        /// <summary>
        /// 当前资源加载器 未设置时使用 Addressables 默认实现
        /// </summary>
        public static IUIAssetLoader Loader { get; set; }

        /// <summary>
        /// 获取有效加载器
        /// </summary>
        private static IUIAssetLoader ResolveLoader()
        {
            if (Loader != null)
                return Loader;

            Loader = new AddressablesUIAssetLoader();
            return Loader;
        }

        /// <summary>
        /// 同步加载 UI 预制体
        /// </summary>
        public static GameObject AddressableLoad(string uiName)
        {
            float t0 = Time.realtimeSinceStartup;
            Debug.Log($"[UILoad.Load] [{uiName}] 开始同步加载 t={t0:F3}");

            GameObject uiPrefab = ResolveLoader().Load(uiName);
            if (uiPrefab == null)
            {
                Debug.LogError($"[UILoad.Load] 加载失败: {uiName}");
                return null;
            }

            float tEnd = Time.realtimeSinceStartup;
            Debug.Log($"[UILoad.Load] [{uiName}] 加载完成 总耗时={(tEnd - t0) * 1000:F1}ms t={tEnd:F3}");
            NormalizeRect(uiPrefab);
            return uiPrefab;
        }

        /// <summary>
        /// 异步加载 UI 预制体
        /// </summary>
        public static async UniTask<GameObject> AddressableLoadAsync(string uiName)
        {
            GameObject uiPrefab = await ResolveLoader().LoadAsync(uiName);
            if (uiPrefab == null)
            {
                Debug.LogError($"[UILoad.LoadAsync] 加载失败: {uiName}");
                return null;
            }

            NormalizeRect(uiPrefab);
            return uiPrefab;
        }

        /// <summary>
        /// 释放 UI 资源
        /// </summary>
        public static void Release(string uiName)
        {
            ResolveLoader().Release(uiName);
        }

        /// <summary>
        /// 归一化 RectTransform
        /// </summary>
        private static void NormalizeRect(GameObject uiPrefab)
        {
            RectTransform rectTransform = uiPrefab.GetComponent<RectTransform>();
            if (rectTransform == null)
                return;

            rectTransform.localScale = Vector3.one;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localRotation = Quaternion.identity;
        }
    }
}
