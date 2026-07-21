/// <summary>
/// PlayerPanel View层扩展 - 用户编写
/// </summary>

using MieMieUIFrameWork.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public partial class PlayerPanelGen
{
    // 在这里添加额外的View逻辑
    void Update(){
        if(Keyboard.current.escapeKey.wasPressedThisFrame){
            UICoreMgr.Instance.ShowWindow<GameStopPanel>();
        }
    }
}

