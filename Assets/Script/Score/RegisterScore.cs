using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//!	[最終更新日] 2021/10/14
//! [内容]       レジのスコア処理
//-----------------------------------------------------------------------------
public class RegisterScore : MonobitEngine.MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! [内容]    スコアリストをInspectorに表示するのに必要なクラス
    //-----------------------------------------------------------------------------
    [System.Serializable]
    public class ScoreList : Utility.DictionaryBase<string, int, ScorePair> { }
    [System.Serializable]
    public class ScorePair : Utility.KeyAndValue<string, int>
    {
        public ScorePair(string key, int value) : base(key, value) { }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    セットボーナスリストをInspectorに表示するのに必要なクラス
    //-----------------------------------------------------------------------------
    [System.Serializable]
    public class SetBonusList : Utility.DictionaryBase<string, List<string>, SetBonusPair> { }
    [System.Serializable]
    public class SetBonusPair : Utility.KeyAndValue<string, List<string>>
    {
        public SetBonusPair(string key, List<string> value) : base(key, value) { }
    }

    //-----------------------------------------------------------------------------
    //! private変数
    //-----------------------------------------------------------------------------
    private int        totalScore = 0; // 現在のスコア
    private GameObject cartObject    ; // カートオブジェクト
    private StackTree  stackTree     ; // スタックツリー

    //-----------------------------------------------------------------------------
    //! public変数
    //-----------------------------------------------------------------------------

    //-----------------------------------------------------------------------------
    //! [内容]    RPC受信関数(現在のスコア)
    //-----------------------------------------------------------------------------
    [MunRPC]
    void RecvScore(int senderTotalScore)
    {
        totalScore = senderTotalScore;
    }

    //-----------------------------------------------------------------------------
    //! Inspectorに公開する変数
    //-----------------------------------------------------------------------------
    [Header("カートのオブジェクト(テスト用)")]                    public GameObject   testCartObject  ; // TODO：テスト用、テストが終わったら消す
    [Header("カートのプレハブ")]                                  public GameObject   cartPrefab      ; // カートプレハブ
    [Header("スコア単位")]                                        public string       scoreUnit = "￥"; // スコアの単位
    [Header("スコアリスト(プレハブの名前,スコア)")]               public ScoreList    scoreList       ; // スコアリスト

    [Tooltip("セット情報は0要素目にスコア、それ以降はプレハブの名前を入力してください。")]
    [Header("セットボーナスリスト(セットの名前,セット情報)")]     public SetBonusList setBonusList    ; // スコアリスト

    //-----------------------------------------------------------------------------
    //! [内容]    有効時処理
    //-----------------------------------------------------------------------------
    void Awake()
    {
        // List型で保持されているのでDictionaryに変換しておく
        scoreList.GetDictionary();
        setBonusList.GetDictionary();
    }

    //-----------------------------------------------------------------------------
    //! [内容]    開始処理
    //-----------------------------------------------------------------------------
    void Start()
    {
        if (!cartPrefab) {
            Debug.LogError("カートのプレハブが設定されていません。");
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {
        // ホストの場合のみ処理
        if (MonobitNetwork.isHost || true) {

            // カートがある場合処理
            if (cartObject) {
                // TODO：レジの範囲に入ったらに変更する
                if (testCartObject.transform.position.x >= 10.0f || true) {
                    // スタックツリーから高さ情報を取得
                    //var height = stackTree.GetHeight();
                    // カートに載っている物を計算する
                    int sumScore = 0;
                    foreach (var stackObject in stackTree.stackList) {
                        // スコアを取得
                        int score = 0;
                        scoreList.GetDictionary().TryGetValue(stackObject.name.Replace("(Clone)", ""), out score);
                        sumScore += score;
                    }
                    Debug.Log(sumScore);
                }
            }
            // カートを検索
            else {
                //testCartObject = GameObject.Find(cartPrefab.name + "(Clone)");
                // TODO:テスト中
                cartObject = testCartObject;
                // 見つかった場合スタックツリーコンポーネントを取得
                if (cartObject) {
                    stackTree = cartObject.GetComponentInChildren<StackTree>();
                    if (!stackTree) {
                        Debug.LogError(cartObject.name + "にスタックツリーが見つかりません。");
                    }
                }
            }

        }

        if (cartPrefab == testCartObject) {
            Debug.Log("同じ");
        }

    }
}
