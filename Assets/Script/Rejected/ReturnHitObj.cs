using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//-----------------------------------------------------------------------------
//! [制作者]		小野龍大
//!	[最終更新日]	2021/10/06
//! [内容]		このオブジェクトに触れているものを
//-----------------------------------------------------------------------------
public class ReturnHitObj : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //!	private変数
    //-----------------------------------------------------------------------------
    int hitGroundNum = 0;

    //-----------------------------------------------------------------------------
    //!	public変数
    //-----------------------------------------------------------------------------
    public List<GameObject> exceptionGameObjectList = new List<GameObject>();        // 触れいても含めないオブジェクト
    public List<string> exceptionTagtList = new List<string>();
    public List<GameObject> hitList = new List<GameObject>();       // 接触中のオブジェクトリスト
    public bool hitGround = false;     // 地面に触れているか

    public CheckStackObj parentScript = null;

    void Start()
    {

    }

    void Update()
    {
        // 情報を送る
        parentScript.hitGround = hitGround;
        parentScript.StackList.Add(hitList);
    }

    public void DestroyComponent()
    {
        foreach(GameObject obj in hitList)
        {
            obj.GetComponent<ReturnHitObj>().DestroyComponent();
        }
        Destroy(GetComponent<ReturnHitObj>());

    }

    void OnCollisionStay(Collision collision)
    {
        // 触れているオブジェクトをリストに追加
        bool check = false;

        foreach (GameObject obj in exceptionGameObjectList)    // 例外リストに存在しないか確認
        {
            if (collision.gameObject == obj)
            {
                check = true;
            }
        }

        foreach (string tag in exceptionTagtList)    // 例外リストに存在しないかタグか確認
        {
            if (collision.gameObject.CompareTag(tag))
            {
                check = true;
            }
        }

        if (!check)      //　なかったら追加
        {
            // 地面に接触していたら
            if (collision.gameObject.CompareTag("Ground"))
            {
                hitGround = true;
                hitGroundNum++;
            }

            hitList.Add(collision.gameObject);


            // 連鎖用コンポーネントとの追加
            ReturnHitObj child = collision.gameObject.GetComponent<ReturnHitObj>();

            if (!child)
            {
                child = collision.gameObject.AddComponent<ReturnHitObj>();
                child.parentScript = parentScript;
                child.exceptionGameObjectList = exceptionGameObjectList;
                child.exceptionTagtList = exceptionTagtList;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // 離れたオブジェクトをリストから削除
        
        if (collision.gameObject.CompareTag("Ground"))      // 地面から離れていたら
        {
            hitGroundNum--;

            if(hitGroundNum == 0)
            {
                hitGround = false;
            }
        }

        // 接触時につけたコンポーネントを削除
        ReturnHitObj comp = collision.gameObject.GetComponent<ReturnHitObj>();
        if (comp)
        {
            comp.DestroyComponent();
        }
        hitList.Remove(collision.gameObject);
    }
}
