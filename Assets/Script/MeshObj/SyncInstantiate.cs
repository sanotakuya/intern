using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/12
//! [内容]       オブジェクトの同期生成
//-----------------------------------------------------------------------------
public class SyncInstantiate : MonobitEngine.MonoBehaviour
{
    [Header("同期生成するプレハブ")] public GameObject prefab; // 生成するプレハブ

    // Update is called once per frame
    void Update()
    {
        // ルームに入室しているか確認
        if (MonobitNetwork.inRoom) {
            // インスタンス生成
            if (prefab) {
                var obj = MonobitNetwork.Instantiate(prefab.name, this.transform.position, this.transform.rotation, 0);
                // 生成できたか確認
                if (!obj) {
                    Debug.LogWarning(this.name + "はオブジェクトの生成に失敗しました。");
                }
            }
            else {
                Debug.LogWarning(this.name + "はプレハブが設定されていません。");
            }
            // 自身を削除
            Destroy(this.gameObject);
        }
    }
}