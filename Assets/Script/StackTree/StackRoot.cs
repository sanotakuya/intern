using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]		小野龍大
//!	[最終更新日]	2021/10/07
//! [内容]		StackTreeのルート
//-----------------------------------------------------------------------------
public class StackRoot : MonobitEngine.MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //!	public変数
    //-----------------------------------------------------------------------------
    public StackTree top = null;

    //-----------------------------------------------------------------------------
    //!	private変数
    //-----------------------------------------------------------------------------
    private List<GameObject> children = new List<GameObject>();        // 現在触れているオブジェクトのリスト

    //-----------------------------------------------------------------------------
    //! [内容]		更新処理
    //-----------------------------------------------------------------------------
    void Start()
    {
        if(!MonobitNetwork.isHost)
        {
            Destroy(this);
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]		更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {
        // 子供をすべてStackListに入れる
        foreach(GameObject obj in children)
        {
            if(obj != null)
            {
                top.stackList.Add(obj);

                if (obj.CompareTag(top.groundTag))
                {
                    top.hitGround = true;
                }
            }
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]		このコンポーネントを削除して、子どもたちも解除する
    //-----------------------------------------------------------------------------
    public void ReleaseRoot()
    {
        foreach(GameObject obj in children)
        {
            obj.GetComponent<StackRoot>().ReleaseRoot();
        }

        Destroy(this);
    }

    //-----------------------------------------------------------------------------
    //! [内容]		子供の追加
    //-----------------------------------------------------------------------------
    void AddChild(GameObject obj)
    {
        obj.AddComponent<StackRoot>().top = top;
        children.Add(obj);
    }

    //-----------------------------------------------------------------------------
    //! [内容]		衝突したタイミングの処理
    //-----------------------------------------------------------------------------
    void OnCollisionStay(Collision collision)
    {
        GameObject obj = collision.gameObject;

        if(top.AddConsideration(obj))
        {
            AddChild(obj);
            obj.layer = LayerMask.NameToLayer("Stack"); // レイヤーをStackに変更
        }

    }

    //-----------------------------------------------------------------------------
    //! [内容]		離れたタイミングの処理
    //-----------------------------------------------------------------------------
    void OnCollisionExit(Collision collision)
    {
        GameObject obj = collision.gameObject;

        if (children.Contains(obj))
        {
            // objより下のrootをすべて解除する
            obj.GetComponent<StackRoot>().ReleaseRoot();
            obj.layer = LayerMask.NameToLayer("Default");       //レイヤーをもとに戻す
            children.Remove(obj);       // 親子ではなくなったのでremove

        }
    }

}
