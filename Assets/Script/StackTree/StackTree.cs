using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]     小野龍大
//!	[最終更新日]	2021/10/07
//! [内容]		スタックされているものの情報を得るクラス
//! [詳細]       exceptionGameObjectListに追加されているオブジェクトはrootになりません
//!             exceptionTagtListに追加されているものはrootになりません
//!             unityのflow上physics->Updateの順で通ります
//!             Tree.rootともにScriptExecutionOrderにおいて優先度を高めています
//-----------------------------------------------------------------------------
public class StackTree : MonobitEngine.MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //!	public変数
    //-----------------------------------------------------------------------------
    public string groundTag = "Ground";
    public bool hitGround = false;     // 地面につながっているか
    public List<GameObject> stackList = new List<GameObject>();        // 現在触れているオブジェクトのリスト

    //-----------------------------------------------------------------------------
    //!	private変数
    //-----------------------------------------------------------------------------
    // 触れいてもスタックに含めないオブジェクトとタグ
    [SerializeField] private List<GameObject> ignoreGameObjectList = new List<GameObject>();
    [SerializeField] private List<string> ignoreTagtList = new List<string>();

    private List<GameObject> children = new List<GameObject>();        // 現在触れているオブジェクトのリスト

    [SerializeField] private float CheckMaxHeight = 12.0f;     // Treeの高さを調べる際の精度
    [SerializeField] private Vector3 BoxCastSize = new Vector3(2.5f, 0.2f, 1.0f);   //飛ばすBoxCastのサイズ
    [SerializeField] private float HeightBulky = 0.05f;      // プレイヤーの想定より低くならないように補正

    //-----------------------------------------------------------------------------
    //! [内容]		開始処理
    //-----------------------------------------------------------------------------
    void Start()
    {
        ignoreTagtList.Add("Player");        // プレイヤーは除外する


    }

    //-----------------------------------------------------------------------------
    //! [内容]		更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {
        if(!MonobitNetwork.isHost)
        {
            Destroy(this);
        }

        hitGround = false;
        stackList.Clear();

        // 子供をすべてStackListに入れる
        foreach (GameObject obj in children)
        {
            stackList.Add(obj);

            if (obj.CompareTag(groundTag))
            {
                hitGround = true;
            }
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]		子供として追加しても問題ないか検討する(trueで問題なし)
    //-----------------------------------------------------------------------------
    public bool AddConsideration(GameObject obj)
    {
        foreach (GameObject exception in ignoreGameObjectList)    // 例外リストに存在しないかゲームオブジェクトか確認
        {
            if (obj == exception)
            {
                return false;
            }
        }

        foreach (string tag in ignoreTagtList)    // 例外リストに存在しないかタグか確認
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
    //! [内容]		リセット関数(削除したとき用)
    //-----------------------------------------------------------------------------
    public void PowerReset()
    {
        children.Clear();
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
            obj.layer = LayerMask.NameToLayer("Stack"); // レイヤーをStackに変更
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]		離れたタイミングの処理
    //-----------------------------------------------------------------------------
    void OnTriggerExit(Collider other)
    {
        GameObject obj = other.gameObject;

        if (children.Contains(obj))
        {
            // objより下のrootをすべて解除する
            obj.GetComponent<StackRoot>().ReleaseRoot();
            obj.layer = LayerMask.NameToLayer("Default");       //レイヤーをもとに戻す
            children.Remove(obj);       // 親子ではなくなったのでremove
        }
    }


    //-----------------------------------------------------------------------------
    //! [内容]		StackTreeの高さを取得(実行順がこのスクリプトより遅いコンポーネントで利用可)
    //-----------------------------------------------------------------------------
    public float GetHeight()
    {
        Vector3 pos = this.transform.position;

        Vector3 startPos = pos;
        startPos.y += CheckMaxHeight;      // 上から飛ばすために上げる

        RaycastHit raycastHit;

        // boxCanstを飛ばして判定
        bool hit = Physics.BoxCast(
            startPos, 
            BoxCastSize, 
            this.transform.up * -1,
            out raycastHit,
            this.transform.rotation, 
            CheckMaxHeight,
            LayerMask.GetMask("Stack")
            );

        if(hit)
        {
            return raycastHit.point.y - pos.y + HeightBulky;

        }

        return 0.0f;
    }

    //-----------------------------------------------------------------------------
    //! [内容]		セットボーナスが現在スタックされている中に存在するか
    //! [arg]       セットボーナスの組み合わせとなるオブジェクトの名前
    //-----------------------------------------------------------------------------
    public bool CheckSetBonus(params string[] objects)
    {
        foreach(string obj in objects)
        {
            bool exist = false;

            //stackの中にセットの一つが存在するか確認する
            foreach (GameObject stack in stackList)
            {
                if(stack.name == obj)
                {
                    exist = true;
                }
                
            }

            if(!exist)
            {
                return false;
            }
        }

        return true;
    }
}
