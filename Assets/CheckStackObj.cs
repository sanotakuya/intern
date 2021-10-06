using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------
//! [制作者]		小野龍大
//!	[最終更新日]	2021/10/06
//! [内容]		積み上がっているオブジェクトの情報を得る
//-----------------------------------------------------------------------------
public class CheckStackObj : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //!	private変数
    //-----------------------------------------------------------------------------
    [SerializeField] private List<GameObject> exceptionList = new List<GameObject>();        // 触れいてもスタックに含めないオブジェクト

    private List<Collider> stackList = new List<Collider>();        // 現在触れているオブジェクトのリスト

    //-----------------------------------------------------------------------------
    //! [内容]		開始処理
    //-----------------------------------------------------------------------------
    void Start()
    {

    }

    //-----------------------------------------------------------------------------
    //! [内容]		更新処理
    //-----------------------------------------------------------------------------
    void FixedUpdate()
    {
        Debug.Log("StackNum:" + stackList.Count.ToString());

        
    }

    //オブジェクトが触れている間
    void OnTriggerEnter(Collider other)
    {
        // スタックエリアと触れているオブジェクトのリストに追加する
        stackList.Add(other);
    }

    //オブジェクトが離れた時
    private void OnTriggerExit(Collider other)
    {
        stackList.Remove(other);
    }
}
