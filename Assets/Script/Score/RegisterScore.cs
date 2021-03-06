using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;
using System.Linq;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//!	[最終更新日] 2021/10/28
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
        public int          totalStack       ; // 今まで積み上げたオブジェクトの総数
        public int          totalScore       ; // 今までの合計スコア
        public int          currentTotalScore; // 今回の合計スコア
        public int          productScore     ; // 商品スコア
        public int          bonusScore       ; // ボーナススコア
        public float        heightScore      ; // 高さスコア
        public float        height           ; // 現在の高さ
        public List<string> bonusNameList    ; // ボーナスの名前リスト
    }

    //-----------------------------------------------------------------------------
    //! 定数
    //-----------------------------------------------------------------------------
    public const int DEFAULT_SCORE = 800; // デフォルトのスコア


    //-----------------------------------------------------------------------------
    //! private変数
    //-----------------------------------------------------------------------------
    private ScoreData   _scoreData  = new ScoreData(); // 現在のスコア
    private GameObject  cartObject                   ; // カートオブジェクト
    private StackTree   stackTree                    ; // スタックツリー
    private MonobitView monobitView                  ; // モノビットビュー
    private bool        isScoring   = true           ; // スコア計算が可能か
    private bool        isHost      = true           ; // ホストか
    private AudioSource audioSource                  ;
    private int         prevTotalScore               ;

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
    void RecvScore(int      totalStack        // 今まで積み上げたオブジェクトの総数
                  ,int      totalScore        // 今までの合計スコア
                  ,int      currentTotalScore // 今回の合計スコア
                  ,int      productScore      // 商品スコア
                  ,int      bonusScore        // ボーナススコア
                  ,float    heightScore       // 高さスコア
                  ,float    height            // 現在の高さ
                  ,string[] bonusNameList     // ボーナスの名前リスト
    )
    {
        _scoreData.totalStack        = totalStack                    ;
        _scoreData.totalScore        = totalScore                    ;
        _scoreData.currentTotalScore = currentTotalScore             ;
        _scoreData.productScore      = productScore                  ;
        _scoreData.bonusScore        = bonusScore                    ;
        _scoreData.heightScore       = heightScore                   ;
        _scoreData.height            = height                        ;
        _scoreData.bonusNameList     = bonusNameList.ToList<string>();
    }

    //-----------------------------------------------------------------------------
    //! Inspectorに公開する変数
    //-----------------------------------------------------------------------------
    [Header("CashRegisterManager")]                           public CashRegisterManager cashRegisterManager; // レジマネージャー
    [Header("カートのプレハブ")]                              public GameObject          cartPrefab         ; // カートプレハブ
    [Header("スコア単位")]                                    public string              scoreUnit = "￥"   ; // スコアの単位
    [Header("スコアリスト(プレハブの名前,スコア)")]           public ScoreList           scoreList          ; // スコアリスト
    [Header("セットボーナスリスト(セットの名前,セット情報)")] public SetBonusList        setBonusList       ; // スコアリスト
    [Header("高さボーナスリスト(高さ(昇順),スコア(倍率))")]   public HeightBonusList     heightBonusList    ; // 高さボーナスリスト
    [Header("レジサウンド")]                                  public AudioClip           registerSound      ; // レジ音

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
        if (!cashRegisterManager) {
            Debug.LogError("レジマネージャーが設定されていません。");
        }

        monobitView = this.GetComponent<MonobitView>();
        if (!monobitView) {
            Debug.LogError("MonobitViewが設定されていません。");
        }

        if (!this.TryGetComponent<AudioSource>(out audioSource)) {
            Debug.LogError("オーディオソースが設定されていません。");
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {
        if (MonobitNetwork.inRoom) {
            // ホストの場合のみ処理
            if (MonobitNetwork.isHost && isHost) {

                // カートがある場合処理
                if (cartObject) {
                    // レジの範囲に入ったら
                    var registerObject = cashRegisterManager.GetIsWithAnyRegister();
                    if (registerObject && isScoring) {

                        ScoreData tmpScore = new ScoreData(); // スコア一時格納
                        tmpScore.bonusNameList = new List<string>();

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

                        // 積み上げられている数を取得
                        var stackNum = stackTree.stackList.Count;
                        tmpScore.totalStack = scoreData.totalStack + stackNum;

                        // カートに載っている物を計算する
                        foreach (var stackObject in stackTree.stackList) {
                            // スコアを取得
                            int score = 0;
                            scoreList.GetDictionary().TryGetValue(stackObject.name.Replace("(Clone)", ""), out score);
                            // スコアが設定されていない場合デフォルトのスコアを設定
                            tmpScore.productScore += score == 0 ? DEFAULT_SCORE : score;
                        }

                        Dictionary<string, int> currenctBonusList = new Dictionary<string, int>();

                        // ボーナス条件を満たしているか確認
                        foreach(var bonusCriteria in setBonusList.GetDictionary()) {
                            var hasScore = SetBonusDecision(stackTree.stackList, bonusCriteria.Value.nameList);
                            if (hasScore) {
                                currenctBonusList.Add(bonusCriteria.Key, bonusCriteria.Value.score);
                                tmpScore.bonusScore += bonusCriteria.Value.score;

                                // ボーナスのタイプ名をリストに追加
                                tmpScore.bonusNameList.Add(bonusCriteria.Key);
                            }
                        }

                        // トータルスコアを計算
                        tmpScore.currentTotalScore = (int)(((float)tmpScore.productScore + (float)tmpScore.bonusScore) * tmpScore.heightScore);
                        tmpScore.totalScore        = scoreData.totalScore + tmpScore.currentTotalScore;

                        // 今回のスコアを送信
                        monobitView.RPC("RecvScore"
                                       ,MonobitTargets.AllBuffered
                                       ,tmpScore.totalStack
                                       ,tmpScore.totalScore
                                       ,tmpScore.currentTotalScore
                                       ,tmpScore.productScore
                                       ,tmpScore.bonusScore
                                       ,tmpScore.heightScore
                                       ,height
                                       ,tmpScore.bonusNameList.ToArray()
                                       );

                        // 乗っているオブジェクトの削除
                        foreach(GameObject obj in stackTree.stackList )
                        {
                            MonobitNetwork.Destroy(obj);
                        }

                        // リセット
                        stackTree.ForceReset();

                        // スコア計算を不可に
                        isScoring = false;
                    }
                    else if (!registerObject) {
                        // スコア計算を可能に
                        isScoring = true;
                    }
                }
                // カートを検索
                else {
                    cartObject = GameObject.Find(cartPrefab.name + "(Clone)");
                    // 見つかった場合スタックツリーコンポーネントを取得
                    if (cartObject) {
                        isHost = true;
                        stackTree = cartObject.GetComponentInChildren<StackTree>();
                        if (!stackTree) {
                            Debug.LogError(cartObject.name + "にスタックツリーが見つかりません。");
                        }
                    }
                }

            }
            else {
                isHost = false;
            }
        }

        // スコアが変わった時にサウンド再生
        if (prevTotalScore != scoreData.totalScore) {
            audioSource.PlayOneShot(registerSound);
        }

        prevTotalScore = scoreData.totalScore;
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
