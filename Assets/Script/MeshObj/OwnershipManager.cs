using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/18
//! [内容]       所有権の管理
//-----------------------------------------------------------------------------
public class OwnershipManager : MonobitEngine.MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! private変数
    //-----------------------------------------------------------------------------
    private List<MonobitView> manageMonobitViews = new List<MonobitView>(); // MonobitViewリスト
    private MonobitPlayer     currentHost                                 ; // 現在のホスト

    //-----------------------------------------------------------------------------
    //! [内容]    RPC受信関数(現在のスコア)
    //-----------------------------------------------------------------------------
    [MunRPC]
    void SendOwner(MonobitPlayer monobitPlayer, int[] viewIDs)
    {
        currentHost = monobitPlayer;

        manageMonobitViews.Clear();

        foreach (var viewID in viewIDs) {
            var monobitView = MonobitView.Find(viewID);
            if (monobitView) {
                manageMonobitViews.Add((MonobitView)monobitView);
            }
            else {
                Debug.LogWarning(this.name + "RPCの受信に失敗しました。");
            }
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    更新処理
    //-----------------------------------------------------------------------------
    void LateUpdate()
    {
        // 自分がホストな場合
        if (MonobitNetwork.isHost) {
            var player = MonobitNetwork.player;
            // 現在の所有者が自分と違う場合かプレイヤーが新たに参加した場合
            if (player != currentHost) {
                List<int> objectIDs = new List<int>();
                // 所有者を変更する
                foreach (var monobitView in manageMonobitViews) {
                    monobitView.TransferOwnership(player);
                    objectIDs.Add(monobitView.viewID);
                }
                // 自分を所有者として送信
                this.monobitView.RPC("SendOwner", MonobitTargets.AllBuffered, player, objectIDs.ToArray());
            }
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    所有権管理に追加(管理に追加されたオブジェクトは所有者が消えても削除されなくなります。)
    //! [引数]    管理に追加するオブジェクトのMonobitView
    //-----------------------------------------------------------------------------
    public void AddManage(MonobitView monobitView)
    {
        // 所有者が消えてもオブジェクトが残るように
        monobitView.isDontDestroyOnRoom = true;
        // 管理リストに追加
        manageMonobitViews.Add(monobitView);
    }
}
