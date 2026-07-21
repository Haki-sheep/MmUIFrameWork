using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MieMieUIFrameWork.UI
{
    /// <summary>
    /// UI 核心管理类
    /// </summary>
    [Serializable]
    public class UICoreMgr :MonoBehaviour
    {
        private static UICoreMgr instance;
        public static UICoreMgr Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("UICoreMgr");
                    instance = obj.AddComponent<UICoreMgr>();
                    instance.Init();
                }
                return instance;
            }
        }
        
        /// <summary>
        /// 堆栈系统
        /// </summary>
        private UIStack uiStack;

        /// <summary>
        /// 所有窗口字典
        /// </summary>
        private Dictionary<string, UIDataBase> uiDic = new();

        [SerializeField]
        private Transform UIRoot;

        [SerializeField]
        private Camera UICamera;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            UIRoot = this.transform;
            UICamera = UIRoot.GetComponentInChildren<Camera>();
            uiStack = new UIStack();
        }

        /// <summary>
        /// 显示窗口
        /// </summary>
        public T ShowWindow<T>(Action action = null) where T : UIDataBase, new()
        {
            Type type = typeof(T);
            string uiName = type.Name;

            if (uiDic.ContainsKey(uiName))
            {
                var existingWindow = uiDic[uiName];
                existingWindow.OnShow();
                action?.Invoke();
                Debug.Log($"[UICoreMgr.ShowWindow] [{uiName}] 命中缓存 OnShow t={Time.realtimeSinceStartup:F3}");
                return existingWindow as T;
            }

            T uiWindow = new T();
            GameObject uiPrefab = UILoad.AddressableLoad(uiName);
            uiPrefab.transform.SetParent(UIRoot, false);
            uiWindow.BindGameObject(uiPrefab, UICamera);
            uiDic.Add(uiName, uiWindow);
            uiWindow.OnAwake();
            uiWindow.OnShow();
            action?.Invoke();
            return uiWindow;
        }

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        public void HideWindow<T>(Action action = null) where T : UIDataBase, new()
        {
            Type type = typeof(T);
            string uiName = type.Name;
            uiDic[uiName]?.OnHide();
            action?.Invoke();
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public void CloseWindow<T>(Action action = null) where T : UIDataBase, new()
        {
            Type type = typeof(T);
            string uiName = type.Name;

            if (uiDic.TryGetValue(uiName, out var uiWindow))
            {
                uiWindow.OnDestroy();
                UILoad.Release(uiName);
                uiDic.Remove(uiName);
            }

            action?.Invoke();
        }

        /// <summary>
        /// 异步加载面板
        /// </summary>
        public async UniTask<T> ShowWindowAsync<T>(Action<T> onComplete = null) where T : UIDataBase, new()
        {
            Type type = typeof(T);
            string uiName = type.Name;

            if (uiDic.ContainsKey(uiName))
            {
                var existingWindow = uiDic[uiName] as T;
                existingWindow.OnShow();
                onComplete?.Invoke(existingWindow);
                return existingWindow;
            }

            GameObject uiPrefab = await UILoad.AddressableLoadAsync(uiName);
            if (uiPrefab == null)
                return null;

            T uiWindow = new T();
            uiPrefab.transform.SetParent(UIRoot, false);
            uiWindow.BindGameObject(uiPrefab, UICamera);
            uiDic.Add(uiName, uiWindow);
            uiWindow.OnAwake();
            uiWindow.OnShow();
            onComplete?.Invoke(uiWindow);
            return uiWindow;
        }

        /// <summary>
        /// 显示窗口并入栈
        /// </summary>
        public T ShowWindowWithStack<T>(Action action = null) where T : UIDataBase, new()
        {
            var currentTop = uiStack.GetTopUI();
            currentTop?.OnHide();
            var newWindow = ShowWindow<T>();
            if (newWindow != null)
                uiStack.PushUI(newWindow);
            action?.Invoke();
            return newWindow;
        }

        /// <summary>
        /// 从堆栈回退
        /// </summary>
        public void PopWindowFromStack(Action action = null)
        {
            var poppedUI = uiStack.PopUI();
            poppedUI?.OnHide();
            var topUI = uiStack.GetTopUI();
            topUI?.OnShow();
            action?.Invoke();
        }

        /// <summary>
        /// 关闭窗口并从堆栈移除
        /// </summary>
        public void CloseWindowFromStack<T>(Action action = null) where T : UIDataBase, new()
        {
            uiStack.RemoveUI<T>();
            CloseWindow<T>();
            action?.Invoke();
        }
    }
}
