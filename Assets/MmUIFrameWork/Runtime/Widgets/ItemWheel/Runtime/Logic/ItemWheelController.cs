using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ItemWheelController : MonoBehaviour
{
    [SerializeField]
    private RectTransform wheelRoot;

    [SerializeField, LabelText("扩展响应区域内距离")]
    private float expandedInnerRadius = 0f;

    [SerializeField, LabelText("扩展响应区域外距离")]
    private float expandedOuterRadius = 0f;

    private RingLayoutData[] sectorLayoutDataList;
    private IRingBehaviour[] ringBehaviourList;
    private int currentItemIndex = -1;

    private ItemWheelPointerResolver pointerResolver;

    private void Awake()
    {
        if (wheelRoot == null)
            wheelRoot = transform as RectTransform;

        sectorLayoutDataList = BuildLayoutFromSectors();
        ringBehaviourList = BuildRingBehaviourList();

        // 初始化解析位置 并且设置扩展距离
        pointerResolver = new ItemWheelPointerResolver(wheelRoot, sectorLayoutDataList);
        pointerResolver.ActivationInnerRadius = sectorLayoutDataList[0].innerRadius - expandedInnerRadius;
        pointerResolver.ActivationOuterRadius = sectorLayoutDataList[0].outerRadius + expandedOuterRadius;
    }

    private void Update()
    {
        if (pointerResolver == null || Mouse.current == null)
            return;

        // 获取鼠标落在哪个扇环
        int index = pointerResolver.GetMouseOnWitchSectorIndex(Mouse.current.position.ReadValue());
        if (index == currentItemIndex)
            return;

        // 触发退出选中事件
        if (currentItemIndex >= 0 && currentItemIndex < ringBehaviourList.Length)
            ringBehaviourList[currentItemIndex]?.OnExit();

        // 设置当前选中项
        currentItemIndex = index;
        Debug.Log($"当前选中项: {currentItemIndex}");

        // 触发进入选中事件
        if (currentItemIndex >= 0 && currentItemIndex < ringBehaviourList.Length)
            ringBehaviourList[currentItemIndex]?.OnEnter();
    }

    /// <summary>
    /// 从 SectorRingDraw 构建布局数据
    /// </summary>
    private RingLayoutData[] BuildLayoutFromSectors()
    {
        RingDraw[] ringList = wheelRoot.GetComponentsInChildren<RingDraw>();
        RingLayoutData[] layoutList = new RingLayoutData[ringList.Length];

        for (int i = 0; i < ringList.Length; i++)
        {
            RingDraw ring = ringList[i];
            layoutList[i] = new RingLayoutData
            {
                index = ParseSectorIndex(ring.transform, i),
                startAngle = ring.StartAngle,
                sweepAngle = ring.SweepAngle,
                innerRadius = ring.InnerRadius,
                outerRadius = ring.OuterRadius
            };
        }

        System.Array.Sort(layoutList, (a, b) => a.index.CompareTo(b.index));
        return layoutList;
    }


    /// <summary>
    /// 收集每个扇环的交互行为
    /// </summary>
    private IRingBehaviour[] BuildRingBehaviourList()
    {
        IRingBehaviour[] behaviourList = new IRingBehaviour[sectorLayoutDataList.Length];

        foreach (var layout in sectorLayoutDataList)
        {
            Transform sector = wheelRoot.Find($"Sector_{layout.index}");
            if (sector != null)
                behaviourList[layout.index] = sector.GetComponent<IRingBehaviour>();
        }

        return behaviourList;
    }

    /// <summary>
    /// 扇环索引填充
    /// </summary>
    /// <param name="sector"></param>
    /// <param name="fallback"></param>
    /// <returns></returns>
    private static int ParseSectorIndex(Transform sector, int fallback)
    {
        string name = sector.name;
        if (name.StartsWith("Sector_") && int.TryParse(name.Substring(7), out int index))
            return index;

        return fallback;
    }

}
