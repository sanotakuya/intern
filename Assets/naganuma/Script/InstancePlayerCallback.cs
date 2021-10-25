using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/25
//! [内容]       プレイヤー生成時のコールバック
//-----------------------------------------------------------------------------
public class InstancePlayerCallback : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! Inspectorに公開する変数
    //-----------------------------------------------------------------------------
    [Header("呼び出す関数")] public UnityEvent CallOnGeneration; // イベント

    //-----------------------------------------------------------------------------
    //! [内容]    開始処理
    //-----------------------------------------------------------------------------
    void Start()
    {
        // イベント発行
        if (CallOnGeneration == null) CallOnGeneration = new UnityEvent();
        CallOnGeneration.Invoke();
    }
}
