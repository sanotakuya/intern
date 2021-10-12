using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------
//! [制作者]		小野龍大
//!	[最終更新日]	2021/10/07
//! [内容]		スタックされているものの情報を得るクラス
//-----------------------------------------------------------------------------
public class StackTree : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //!	public変数
    //-----------------------------------------------------------------------------
    public bool hitGround = false;     // 地面につながっているか
    public List<GameObject> stackList = new List<GameObject>();        // 現在触れているオブジェクトのリスト


    //-----------------------------------------------------------------------------
    //!	private変数
    //-----------------------------------------------------------------------------
    // 触れいてもスタックに含めないオブジェクトとタグ
    [SerializeField] private List<GameObject> exceptionGameObjectList = new List<GameObject>();
    [SerializeField] private List<string> exceptionTagtList = new List<string>();

    private List<GameObject> children = new List<GameObject>();        // 現在触れているオブジェクトのリスト

    //-----------------------------------------------------------------------------
    //! [内容]		開始処理
    //-----------------------------------------------------------------------------
    void Start()
    {
        exceptionTagtList.Add("Player");        // プレイヤーは除外する
    }

    //-----------------------------------------------------------------------------
    //! [内容]		更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {
        // 子供をすべてStackListに入れる
        foreach (GameObject obj in children)
        {
            stackList.Add(obj);
        }

        //Debug.Log("StackNum:" + stackList.Count.ToString());
        stackList.Clear();
    }

    //-----------------------------------------------------------------------------
    //! [内容]		子供として追加しても問題ないか検討する(trueで問題なし)
    //-----------------------------------------------------------------------------
    public bool AddConsideration(GameObject obj)
    {
        foreach (GameObject exception in exceptionGameObjectList)    // 例外リストに存在しないかゲームオブジェクトか確認
        {
            if (obj == exception)
            {
                return false;
            }
        }

        foreach (string tag in exceptionTagtList)    // 例外リストに存在しないかタグか確認
        {
            if (obj.CompareTag(tag))
            {
                return false;
            }
        }

        if(obj.GetComponent<StackRoot>())       // 現在誰かの子供か
        {
            return false;
        }

        return true;
    }

    //-----------------------------------------------------------------------------
    //! [内容]		子供として追加する
    //-----------------------------------------------------------------------------
    void AddChild(GameObject obj)
    {
        obj.AddComponent<StackRoot>().top = this;
        children.Add(obj);
    }

    //-----------------------------------------------------------------------------
    //! [内容]		衝突したタイミングの処理
    //-----------------------------------------------------------------------------
    void OnTriggerStay(Collider other)
    {
        GameObject obj = other.gameObject;
        // 除外リストなどを考慮して問題ないなら追加する
        if (AddConsideration(obj))
        {
            AddChild(obj);
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]		離れたタイミングの処理
    //-----------------------------------------------------------------------------
    private void OnTriggerExit(Collider other)
    {
        GameObject obj = other.gameObject;

        if (children.Contains(obj))
        {
            // objより下のrootをすべて解除する
            obj.GetComponent<StackRoot>().ReleaseRoot();
            children.Remove(obj);       // 親子ではなくなったのでremove
        }
    }
}
