using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/18
//! [内容]       オブジェクトの同期生成
//-----------------------------------------------------------------------------
public class SyncInstantiate : MonobitEngine.MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! public変数
    //-----------------------------------------------------------------------------
    [Header("OwnershipManager")]       public OwnershipManager ownershipManager; // 所有権管理
    [Header("プレハブが存在するパス")] public string           path            ; // プレハブのパス
    [Header("同期生成するプレハブ")]   public GameObject       prefab          ; // 生成するプレハブ

    //-----------------------------------------------------------------------------
    //! [内容]    開始処理
    //-----------------------------------------------------------------------------
    void Start()
    {
        if (!prefab) {
            Debug.LogError(this.name + "でプレハブが設定されていません。");
        }
        if (!ownershipManager) {
            Debug.LogError(this.name + "でOwnershipManagerが設定されていません。");
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {
        // ルームに入室しているか確認
        if (MonobitNetwork.inRoom)
        {
            // 自身がルームのホストの場合はインスタンス化
            if (MonobitNetwork.isHost)
            {
                // インスタンス生成
                var obj = MonobitNetwork.Instantiate(path + prefab.name, this.transform.position, this.transform.rotation, 0);
                // 生成できたか確認
                if (obj) {
                    var monobitView = obj.GetComponent<MonobitView>();
                    if (monobitView) {
                        ownershipManager.AddManage(monobitView);
                    }
                    else {
                        Debug.LogWarning(this.name + "はMonobitViewがアタッチされていません。");
                    }
                }
                else {
                    Debug.LogWarning(this.name + "はオブジェクトの生成に失敗しました。");
                }
            }
            // 自身を削除
            Destroy(this.gameObject);
        }
    }
}