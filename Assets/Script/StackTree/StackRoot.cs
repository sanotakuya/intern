﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------
//! [制作者]		小野龍大
//!	[最終更新日]	2021/10/07
//! [内容]		StackTreeのルート
//-----------------------------------------------------------------------------
public class StackRoot : MonoBehaviour
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
    void Update()
    {
        // 子供をすべてStackListに入れる
        foreach(GameObject obj in children)
        {
            top.stackList.Add(obj);
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
            children.Remove(obj);       // 親子ではなくなったのでremove
        }
    }

}
