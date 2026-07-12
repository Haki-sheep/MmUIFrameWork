using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MieMieFrameWork.UI
{
    /// <summary>
    /// 宿主存在 AddressableMgr 时桥接其 UI 所需方法
    /// </summary>
    internal sealed class AddressableMgrBridgeLoader : IUIAssetLoader
    {
        /// <summary>
        /// 地址到实例
        /// </summary>
        private readonly Dictionary<string, GameObject> instanceDict = new();

        /// <summary>
        /// 同步加载方法
        /// </summary>
        private readonly MethodInfo loadGameObjectMethod;

        /// <summary>
        /// 异步加载方法
        /// </summary>
        private readonly MethodInfo loadGameObjectAsyncMethod;

        /// <summary>
        /// 销毁方法
        /// </summary>
        private readonly MethodInfo destroyObjectMethod;

        /// <summary>
        /// 尝试创建桥接加载器
        /// </summary>
        public static bool TryCreate(out AddressableMgrBridgeLoader loader)
        {
            loader = null;
            Type mgrType = ResolveAddressableMgrType();
            if (mgrType == null)
                return false;

            MethodInfo loadGo = mgrType.GetMethod(
                "LoadGameObject",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(Transform), typeof(bool) },
                null);

            MethodInfo loadGoAsync = mgrType.GetMethod(
                "LoadGameObjectAsync",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(Transform), typeof(bool) },
                null);

            MethodInfo destroyObj = mgrType.GetMethod(
                "DestroyObject",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(GameObject) },
                null);

            if (loadGo == null || loadGoAsync == null || destroyObj == null)
                return false;

            loader = new AddressableMgrBridgeLoader(loadGo, loadGoAsync, destroyObj);
            return true;
        }

        /// <summary>
        /// 构造
        /// </summary>
        private AddressableMgrBridgeLoader(
            MethodInfo loadGameObjectMethod,
            MethodInfo loadGameObjectAsyncMethod,
            MethodInfo destroyObjectMethod)
        {
            this.loadGameObjectMethod = loadGameObjectMethod;
            this.loadGameObjectAsyncMethod = loadGameObjectAsyncMethod;
            this.destroyObjectMethod = destroyObjectMethod;
        }

        /// <summary>
        /// 同步加载并实例化 UI
        /// </summary>
        public GameObject Load(string uiName)
        {
            Release(uiName);
            object result = loadGameObjectMethod.Invoke(null, new object[] { uiName, null, false });
            GameObject go = result as GameObject;
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
            object taskObj = loadGameObjectAsyncMethod.Invoke(null, new object[] { uiName, null, false });
            GameObject go = await AwaitUniTaskGameObject(taskObj);
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

            destroyObjectMethod.Invoke(null, new object[] { go });
            instanceDict.Remove(uiName);
        }

        /// <summary>
        /// 解析 AddressableMgr 类型
        /// </summary>
        private static Type ResolveAddressableMgrType()
        {
            const string typeName = "MieMieFrameWork.AddressableMgr";
            Type type = Type.GetType($"{typeName}, Assembly-CSharp");
            if (type != null)
                return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }

            return null;
        }

        /// <summary>
        /// 等待 UniTask GameObject
        /// </summary>
        private static async UniTask<GameObject> AwaitUniTaskGameObject(object uniTaskObj)
        {
            if (uniTaskObj == null)
                return null;

            Type taskType = uniTaskObj.GetType();
            MethodInfo getAwaiter = taskType.GetMethod("GetAwaiter", BindingFlags.Public | BindingFlags.Instance);
            if (getAwaiter == null)
                return null;

            object awaiter = getAwaiter.Invoke(uniTaskObj, null);
            Type awaiterType = awaiter.GetType();
            MethodInfo isCompletedGetter = awaiterType.GetProperty("IsCompleted")?.GetGetMethod();
            MethodInfo getResult = awaiterType.GetMethod("GetResult", BindingFlags.Public | BindingFlags.Instance);

            while (isCompletedGetter != null && !(bool)isCompletedGetter.Invoke(awaiter, null))
                await UniTask.Yield();

            return getResult?.Invoke(awaiter, null) as GameObject;
        }
    }
}
