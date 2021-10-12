using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

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
    // 触れいてもスタックに含めないオブジェクト
    [SerializeField] private List<GameObject> exceptionGameObjectList = new List<GameObject>();
    [SerializeField] private List<string> exceptionTagtList = new List<string>();
    private List<GameObject> hitStackAreaList = new List<GameObject>();        // 現在触れているオブジェクトのリスト
    Hashtable hashtable = new Hashtable();

    //-----------------------------------------------------------------------------
    //!	public変数
    //-----------------------------------------------------------------------------
    public List<List<GameObject>> StackList = new List<List<GameObject>>(); // スタック中の網羅リスト(重複あり)
    public bool hitGround = false;     // 地面につながっているか

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
        GetStackNum();
        StackList.Clear();
    }

    //-----------------------------------------------------------------------------
    //! [内容]		遅れた更新処理
    //-----------------------------------------------------------------------------
    void LateUpdate()
    {
        
    }

    //-----------------------------------------------------------------------------
    //! [内容]		Stack数を取得
    //-----------------------------------------------------------------------------
    void GetStackNum()
    {
        List<GameObject> noDuplicationlist = new List<GameObject>();
        noDuplicationlist = hitStackAreaList.Union(noDuplicationlist).ToList();

        foreach (List<GameObject> list in StackList)
        {
            noDuplicationlist = list.Union(noDuplicationlist).ToList();
        }

        Debug.Log("StackNum:" + noDuplicationlist.Count.ToString());
    }

    //オブジェクトが触れている間
    void OnTriggerStay(Collider other)
    {
        // スタックエリアと触れているオブジェクトをリストに追加
        bool check = false;
  
        foreach(GameObject obj in exceptionGameObjectList)    // 例外リストに存在しないかゲームオブジェクトか確認
        {
            if(other.gameObject == obj)
            {
                check = true;
            }
        }

        foreach (string tag in exceptionTagtList)    // 例外リストに存在しないかタグか確認
        {
            if (other.gameObject.CompareTag(tag))
            {
                check = true;
            }
        }

        if (!check)      // なかったら追加
        {
            hitStackAreaList.Add(other.gameObject);

            if(!other.gameObject.GetComponent<ReturnHitObj>())
            {
                ReturnHitObj child = other.gameObject.AddComponent<ReturnHitObj>();
                child.parentScript = this;
                child.exceptionGameObjectList = exceptionGameObjectList;
                child.exceptionTagtList = exceptionTagtList;
            }
        }
    }

    //オブジェクトが離れた時
    private void OnTriggerExit(Collider other)
    {
        // スタックエリアから離れたオブジェクトをリストから削除

        // 接触時につけたコンポーネントを削除
        ReturnHitObj comp = other.gameObject.GetComponent<ReturnHitObj>();
        if(comp)
        {
            Destroy(comp);
        }
       
        hitStackAreaList.Remove(other.gameObject);
    }
}
