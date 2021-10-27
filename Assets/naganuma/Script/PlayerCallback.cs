using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/25
//! [内容]       プレイヤーのコールバック
//-----------------------------------------------------------------------------
public class PlayerCallback : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! private変数
    //-----------------------------------------------------------------------------
    private HoldThrow holdThrow;

    //-----------------------------------------------------------------------------
    //! Inspectorに公開する変数
    //-----------------------------------------------------------------------------
    [Header("生成時呼び出す関数")]                 public UnityEvent             CallOnGeneration    ; // 生成時イベント
    [Header("削除時呼び出す関数")]                 public UnityEvent             CallOnDestroy       ; // 削除時イベント
    [Header("掴む対象が見つかった時呼び出す関数")] public UnityEvent<HoldThrow>  CallOnFindHoldObject; // 掴む対象が見つかった時のイベント

    //-----------------------------------------------------------------------------
    //! [内容]    開始処理
    //-----------------------------------------------------------------------------
    void Start()
    {
        // コンポーネントを取得
        if (!this.TryGetComponent<HoldThrow>(out holdThrow)) {
            Debug.LogError("プレイヤーにholdthrowがありません。");
        }
        // イベント発行
        CallOnGeneration?.Invoke();
    }

    //-----------------------------------------------------------------------------
    //! [内容]    更新処理
    //-----------------------------------------------------------------------------
    void LateUpdate() {
        if (holdThrow) {
            if (holdThrow.holdObject) {
                if (holdThrow.holdObject.TryGetComponent<MakeEffect>(out var makeEffect)) {
                    makeEffect.OnFindHoldObject(holdThrow);
                }
                CallOnFindHoldObject?.Invoke(holdThrow);
            }
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]     削除処理（コールバック）
    //-----------------------------------------------------------------------------
    void OnDestroy()
    {
        // インベント発行
        CallOnDestroy?.Invoke();
    }
}
