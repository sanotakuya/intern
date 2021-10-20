using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/20
//! [内容]       インゲームのUI
//-----------------------------------------------------------------------------
public class InGameUIManager : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! private変数
    //-----------------------------------------------------------------------------
    private StackTree  stackTree        ; // スタックツリー
    private GameObject cartObject       ; // カートオブジェクト
    private float      prevHeightLimit  ; // 前フレームの高さ
    private Text[]     heightTextComs   ; // 高さテキストコンポーネント
    private Image      scoreBoardImage  ; // スコアボードのイメージ
    bool keyDown = false;

    //-----------------------------------------------------------------------------
    //! public変数
    //-----------------------------------------------------------------------------

    //-----------------------------------------------------------------------------
    //! Inspectorに公開する変数
    //-----------------------------------------------------------------------------
    [Header("GameManager")]                        public GameManager   gameManager  ; // ゲームマネージャー
    [Header("RegisterScore")]                      public RegisterScore registerScore; // スコア
    [Header("GameTimer")]                          public GameTimer     gameTimer    ; // ゲームタイマー
    [Header("カートプレハブ")]                     public GameObject    cartPrefab   ; // カートプレハブ
    [Header("単位テキスト")]                       public Text          UnitText     ; // 単位テキスト
    [Header("スコアテキスト")]                     public Text          scoreText    ; // スコアテキスト
    [Header("タイムテキスト")]                     public Text          timeText     ; // タイムテキスト
    [Header("カウントダウンテキスト")]             public Text          countDown    ; // ウントダウンテキスト
    [Header("高さスライダー")]                     public Slider        heightSlider ; // 高さスライダー
    [Header("高さスライダーテキスト")]             public Text          heightText   ; // 高さテキスト
    [Header("高さスライダーの表示上限値")]         public float         heightLimit  ; // スライダーの上限値
    [Header("スコアボード")]                       public GameObject    scoreBoard   ; // スコアボード
    [Header("スコアボードのフェードスピード(秒)")] public float         sb_fadeTime  ; // スコアボードフェードタイム
    [Header("(スコアボード)ランクテキスト")]       public Text          sb_rankText  ; // (スコアボード)ランクテキスト
    [Header("(スコアボード)スコアテキスト")]       public Text          sb_scoreText ; // (スコアボード)スコアテキスト
    [Header("(スコアボード)タイムテキスト")]       public Text          sb_timeText  ; // (スコアボード)タイムテキスト
    [Header("(スコアボード)スタックテキスト")]     public Text          sb_stackText ; // (スコアボード)スタックテキスト

    //-----------------------------------------------------------------------------
    //! [内容]    開始処理
    //-----------------------------------------------------------------------------
    void Start()
    {
        // インスペクターで指定されているかチェック
        if (!registerScore) Debug.LogError("RegisterScoreが指定されていません。")                 ;
        if (!gameTimer)     Debug.LogError("ゲームタイマーが指定されていません。")                ;
        if (!UnitText)      Debug.LogError("単位テキストが指定されていません。")                  ;
        if (!scoreText)     Debug.LogError("スコアテキストが指定されていません。")                ;
        if (!timeText)      Debug.LogError("タイムテキストが指定されていません。")                ;
        if (!countDown)     Debug.LogError("カウントダウンテキストが指定されていません。")        ;
        if (!heightSlider)  Debug.LogError("高さスライダーが指定されていません。")                ;
        if (!scoreBoard)    Debug.LogError("スコアボードが指定されていません。")                  ;
        if (!sb_rankText )  Debug.LogError("(スコアボード)ランクテキストが指定されていません。")  ;
        if (!sb_scoreText)  Debug.LogError("(スコアボード)スコアテキストが指定されていません。")  ;
        if (!sb_timeText )  Debug.LogError("(スコアボード)タイムテキストが指定されていません。")  ;
        if (!sb_stackText)  Debug.LogError("(スコアボード)スタックテキストが指定されていません。");
        if (!heightText)    Debug.LogError("高さテキストが指定されていません。")                  ;
        else {
            // 高さテキストの子を一括取得
            heightTextComs    = new Text[heightText.transform.childCount + 1];
            heightTextComs[0] = heightText;
            for (int i = 0; i < heightText.transform.childCount; i++) {
                var text = heightText.transform.GetChild(i);
                if (text) {
                    heightTextComs[i + 1] = text.GetComponent<Text>();
                }
            }
        }

        // スコアボードのアルファ値を変更
        scoreBoardImage = scoreBoard.GetComponent<Image>();
        if (!scoreBoardImage) Debug.LogError("スコアボードにImageコンポーネントがありません。");
        scoreBoardImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        sb_rankText .color    = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        sb_scoreText.color    = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        sb_timeText .color    = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        sb_stackText.color    = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    }

    //-----------------------------------------------------------------------------
    //! [内容]    更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {
        if (stackTree) {
            if (Input.GetKeyDown(KeyCode.Backspace)) {
                keyDown = true;
            }
            // TODO:後でisPlaying()をpublicにするように伝えるs
            if (/*gameManager.isPlaying()*/!keyDown) {
                // カウントダウン
                if (gameTimer.startCount > 0.0f) {
                    countDown.text = (gameTimer.startCount + 1.0f).ToString();
                }
                else {
                    countDown.enabled = false;
                }

                // 残り時間から分と秒を計算
                var limitCount = gameTimer.limitCount;
                int minutes = Mathf.FloorToInt(limitCount / 60.0f);               // 分
                int seconds = Mathf.FloorToInt(limitCount - minutes * 60.0f);     // 秒
                timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

                // スコア表示
                var score = registerScore.scoreData.totalScore;
                score = score > 9999999 ? 9999999 : score;
                scoreText.text = string.Format("{0:00000#,0}", score);

                // 高さを表示
                if (heightLimit != 0) {
                    var height = stackTree.GetHeight();
                    heightSlider.value = height <= heightLimit ? height : 1.0f;

                    // 高さテキストの表示変更
                    for (int i = 0; i < heightTextComs.Length; i++) {
                        float heightNum = i / 5.0f * heightLimit;
                        if (i == heightTextComs.Length - 1) {
                            if (heightNum < height) {
                                heightNum = height;
                            }
                        }
                        string heightStr = string.Format("{0:0.00}", heightNum) + "m";
                        heightTextComs[(heightTextComs.Length - 1) - i].text = heightStr;
                    }
                }
            }
            else {
                // スコアボードを有効に
                if (!scoreBoard.activeSelf) {
                    scoreBoard.SetActive(true);

                    // テキストに各データを適用 //

                    // ランク表示
                    // TODO:ランク処理
                    sb_rankText.text = "A";
                    // スコア
                    var score = registerScore.scoreData.totalScore;
                    score = score > 9999999 ? 9999999 : score;
                    sb_scoreText.text = string.Format("{0:00000#,0}", score);
                    // タイム
                    var currentTime = gameTimer.currentGameTime;
                    int minutes = Mathf.FloorToInt(currentTime / 60.0f);              // 分
                    int seconds = Mathf.FloorToInt(currentTime - minutes * 60.0f);    // 秒
                    timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                    // スタック
                    // TODO:RegisterScoreでスタック総数をカウントする処理を用意
                }

                // フェード処理
                if (scoreBoardImage.color.a < 1.0f) {
                    scoreBoardImage.color = scoreBoardImage.color + (new Color(0.0f, 0.0f, 0.0f, sb_fadeTime) * Time.deltaTime);
                    sb_rankText.color = sb_rankText.color + (new Color(0.0f, 0.0f, 0.0f, sb_fadeTime) * Time.deltaTime);
                    sb_scoreText.color = sb_scoreText.color + (new Color(0.0f, 0.0f, 0.0f, sb_fadeTime) * Time.deltaTime);
                    sb_timeText.color = sb_timeText.color + (new Color(0.0f, 0.0f, 0.0f, sb_fadeTime) * Time.deltaTime);
                    sb_stackText.color = sb_stackText.color + (new Color(0.0f, 0.0f, 0.0f, sb_fadeTime) * Time.deltaTime);
                }
                else {
                    scoreBoardImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    sb_rankText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    sb_scoreText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    sb_timeText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    sb_stackText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                }
            }
        }
        else {
            if (cartPrefab) {
                cartObject = GameObject.Find(cartPrefab.name + "(Clone)");
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
}
