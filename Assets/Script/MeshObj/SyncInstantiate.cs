using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/15
//! [内容]       オブジェクトの同期生成
//-----------------------------------------------------------------------------
public class SyncInstantiate : MonobitEngine.MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! public変数
    //-----------------------------------------------------------------------------
    [Header("プレハブが存在するパス")] public string     path  ; // プレハブのパス
    [Header("同期生成するプレハブ")]   public GameObject prefab; // 生成するプレハブ

    //-----------------------------------------------------------------------------
    //! [内容]    更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {
        // ルームに入室しているか確認
        if (MonobitNetwork.inRoom) {
            // 自身がルームのホストの場合はインスタンス化
            if (MonobitNetwork.isHost) {
                // インスタンス生成
                if (prefab) {
                    var obj = MonobitNetwork.Instantiate(path + prefab.name, this.transform.position, this.transform.rotation, 0);
                    // 生成できたか確認
                    if (obj) {
                        // 自身のスケールを生成したオブジェクトに適用
                        obj.transform.localScale = this.transform.localScale;
                    }
                    else {
                        Debug.LogWarning(this.name + "はオブジェクトの生成に失敗しました。");
                    }
                }
                else {
                    Debug.LogWarning(this.name + "はプレハブが設定されていません。");
                }
            }
            // 自身を削除
            Destroy(this.gameObject);
        }
    }
}