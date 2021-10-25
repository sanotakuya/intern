using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;
using System.Linq;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/13
//! [内容]       レジの当たり判定の送受信
//-----------------------------------------------------------------------------
public class CashRegisterManager : MonobitEngine.MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! private変数
    //-----------------------------------------------------------------------------
    private List<bool> inAreas = new List<bool>(); // レジのエリアに入っているか

    //-----------------------------------------------------------------------------
    //! public変数
    //-----------------------------------------------------------------------------

    //-----------------------------------------------------------------------------
    //! Inspectorに公開する変数
    //-----------------------------------------------------------------------------
    [Header("レジのオブジェクト")] public List<CashRegisterArea> cashRegisterAreas; // レジエリアコンポーネント

    //-----------------------------------------------------------------------------
    //! [内容]    RPC受信関数(ホストからレジのエリアに入ったかを取得)
    //-----------------------------------------------------------------------------
    [MunRPC]
    void RecvInRegister(bool[] senderinAreas)
    {
        inAreas = senderinAreas.ToList<bool>();
    }

    //-----------------------------------------------------------------------------
    //! [内容]    更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {
        // ホストの場合のみ処理
        if (MonobitNetwork.isHost) {
            List<bool> tmpInAreas = new List<bool>();
            // 当たり判定の状況を取得
            foreach(var registar in cashRegisterAreas) {
                tmpInAreas.Add(registar.inArea);
            }
            // 当たり判定の状況を送信
            this.monobitView.RPC("RecvInRegister", MonobitTargets.All, tmpInAreas.ToArray());
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    どのレジにいるかを取得
    //! [戻り値]  レジのゲームオブジェクト(当たっていない場合はNULLを返す)
    //-----------------------------------------------------------------------------
    public GameObject GetIsWithAnyRegister()
    {
        for (int i = 0; i < inAreas.Count; i++) {
            if (inAreas[i]) {
                return cashRegisterAreas[i].gameObject;
            }
        }
        return null;
    }
}
