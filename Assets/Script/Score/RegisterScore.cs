using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;
using System.Linq;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//!	[最終更新日] 2021/10/14
//! [内容]       レジのスコア処理
//-----------------------------------------------------------------------------
public class RegisterScore : MonobitEngine.MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! [内容]    スコアリストのジェネリック隠蔽
    //-----------------------------------------------------------------------------
    [System.Serializable]
    public class ScoreList : Utility.DictionaryBase<string, int, ScorePair> { }
    [System.Serializable]
    public class ScorePair : Utility.KeyAndValue<string, int>
    {
        public ScorePair(string key, int value) : base(key, value) { }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    セットボーナスリストのジェネリック隠蔽
    //-----------------------------------------------------------------------------
    [System.Serializable]
    public class SetBonusList : Utility.DictionaryBase<string, SetBonusData, SetBonusPair> { }
    [System.Serializable]
    public class SetBonusPair : Utility.KeyAndValue<string, SetBonusData>
    {
        public SetBonusPair(string key, SetBonusData value) : base(key, value) { }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    高さボーナスリストのジェネリック隠蔽
    //-----------------------------------------------------------------------------
    [System.Serializable]
    public class HeightBonusList : Utility.DictionaryBase<float, float, HeightBonusPair> { }
    [System.Serializable]
    public class HeightBonusPair : Utility.KeyAndValue<float, float> {
        public HeightBonusPair(float key, float value) : base(key, value) { }
    }

    //-----------------------------------------------------------------------------
    //! 構造体
    //-----------------------------------------------------------------------------
    [System.Serializable]
    public struct SetBonusData // セットボーナスのデータ
    {
        public int          score   ; // スコア
        public List<string> nameList; // 名前リスト(プレハブ)
    }

    public struct ScoreData   // スコアデータ
    {
        public int   totalScore  ;      // 今までの合計スコア
        public int   currentTotalScore; // 今回の合計スコア
        public int   productScore;      // 商品スコア
        public int   bonusScore  ;      // ボーナススコア
        public float heightScore ;      // 高さスコア
    }

    //-----------------------------------------------------------------------------
    //! private変数
    //-----------------------------------------------------------------------------
    private ScoreData  _scoreData  = new ScoreData(); // 現在のスコア
    private GameObject cartObject                   ; // カートオブジェクト
    private StackTree  stackTree                    ; // スタックツリー
    private bool       isScoring   = true           ; // スコア計算が可能か

    //-----------------------------------------------------------------------------
    //! public変数
    //-----------------------------------------------------------------------------
    public ScoreData scoreData {  // スコアデータ
        get { return _scoreData; }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    RPC受信関数(現在のスコア)
    //-----------------------------------------------------------------------------
    [MunRPC]
    void RecvScore(ScoreData senderScoreData)
    {
        _scoreData = senderScoreData;
    }

    //-----------------------------------------------------------------------------
    //! Inspectorに公開する変数
    //-----------------------------------------------------------------------------
    [Header("カートのオブジェクト(テスト用)")]                public GameObject      testCartObject  ; // TODO：テスト用、テストが終わったら消す
    [Header("カートのプレハブ")]                              public GameObject      cartPrefab      ; // カートプレハブ
    [Header("スコア単位")]                                    public string          scoreUnit = "￥"; // スコアの単位
    [Header("スコアリスト(プレハブの名前,スコア)")]           public ScoreList       scoreList       ; // スコアリスト
    [Header("セットボーナスリスト(セットの名前,セット情報)")] public SetBonusList    setBonusList    ; // スコアリスト
    [Header("高さボーナスリスト(高さ(昇順),スコア(倍率))")]   public HeightBonusList heightBonusList ; // 高さボーナスリスト

    //-----------------------------------------------------------------------------
    //! [内容]    有効時処理
    //-----------------------------------------------------------------------------
    void Awake()
    {
        // List型で保持されているのでDictionaryに変換しておく
        scoreList.GetDictionary();
        setBonusList.GetDictionary();
        heightBonusList.GetDictionary();
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
                if (testCartObject.transform.position.x >= 10.0f || true && isScoring) {

                    ScoreData tmpScore = new ScoreData(); // スコア一時格納

                    // スタックツリーから高さ情報を取得
                    float height = stackTree.GetHeight();

                    // 高さボーナス
                    foreach (var heightBonus in heightBonusList.GetDictionary().OrderBy(c => c.Key)) {
                        if (height >= heightBonus.Key) {
                            tmpScore.heightScore = heightBonus.Value;
                        }
                    }

                    // 格納されている最大の高さを取得
                    var maxHeight = heightBonusList.GetDictionary().OrderByDescending(c => c.Key).FirstOrDefault().Key;

                    // 最大の高さより大きい場合
                    if (height > maxHeight) {
                        int distance = Mathf.FloorToInt(height - maxHeight);
                        tmpScore.heightScore += distance;
                    }

                    // カートに載っている物を計算する
                    foreach (var stackObject in stackTree.stackList) {
                        // スコアを取得
                        int score = 0;
                        scoreList.GetDictionary().TryGetValue(stackObject.name.Replace("(Clone)", ""), out score);
                        tmpScore.productScore += score;
                    }

                    Dictionary<string, int> currenctBonusList = new Dictionary<string, int>();

                    // ボーナス条件を満たしているか確認
                    foreach(var bonusCriteria in setBonusList.GetDictionary()) {
                        var hasScore = SetBonusDecision(stackTree.stackList, bonusCriteria.Value.nameList);
                        if (hasScore) {
                            currenctBonusList.Add(bonusCriteria.Key, bonusCriteria.Value.score);
                            tmpScore.bonusScore += bonusCriteria.Value.score;
                        }
                    }

                    // トータルスコアを計算
                    tmpScore.currentTotalScore = (int)(((float)tmpScore.productScore + (float)tmpScore.bonusScore) * tmpScore.heightScore);
                    tmpScore.totalScore        = scoreData.totalScore + tmpScore.currentTotalScore;

                    // 今回のスコアを送信
                    RecvScore(tmpScore);

                    // スコア計算を不可に
                    isScoring = false;
                }
                else {
                    // スコア計算を可能に
                    isScoring = true;
                }
            }
            // カートを検索
            else {
                testCartObject = GameObject.Find(cartPrefab.name + "(Clone)");
                // 見つかった場合スタックツリーコンポーネントを取得
                if (cartObject) {
                    stackTree = cartObject.GetComponentInChildren<StackTree>();
                    if (!stackTree) {
                        Debug.LogError(cartObject.name + "にスタックツリーが見つかりません。");
                    }
                }
            }

        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    セットボーナスの判定を行う
    //! [引数]    stackList -> スタックツリーコンポーネントのスタックリスト
    //! [引数]    nameList  -> セットボーナスのプレハブ名リスト
    //! [戻り値]  bool
    //-----------------------------------------------------------------------------
    private bool SetBonusDecision(List<GameObject> stackList, List<string> nameList)
    {
        // セットボーナスの名前リストがスタックリストより大きさ場合は処理しない
        if (stackList.Count < nameList.Count - 1) {
            return false;
        }

        // スタックの中にあるか確認
        foreach (var name in nameList) {

            bool isExists = false; // 存在するか

            foreach (var stackObject in stackList) {

                // 名前が同じなら
                if (stackObject.name.Replace("(Clone)", "") == name) {
                    isExists = true;
                }

            }

            // 存在しない場合
            if (!isExists) {
                return false;
            }

        }

        return true;
    }
}
