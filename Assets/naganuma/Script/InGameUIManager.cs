using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/20
//! [内容]       インゲームのUI
//-----------------------------------------------------------------------------
public class InGameUIManager : MonobitEngine.MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! private変数
    //-----------------------------------------------------------------------------
    private StackTree     stackTree                ; // スタックツリー
    private GameObject    cartObject               ; // カートオブジェクト
    private float         prevHeightLimit          ; // 前フレームの高さ
    private Text[]        heightTextComs           ; // 高さテキストコンポーネント
    private CanvasGroup   uiCanvasGroup            ; // UIキャンバスグループ
    private CanvasGroup   sb_canvasGroup           ; // スコアボードキャンバスグループ
    private float         timeCount                ; // 経過時間
    private float         height                   ; // 現在の高さ
    private bool          isHost = true            ; // ホストか
    private GameUISound   gameUiSound              ;
    private bool          isPlayTimeUpSound = false;

    //-----------------------------------------------------------------------------
    //! public変数
    //-----------------------------------------------------------------------------

    //-----------------------------------------------------------------------------
    //! Inspectorに公開する変数
    //-----------------------------------------------------------------------------
    [Header("UIキャンバス")]                   public Canvas        uiCanvas     ; // UIキャンバス
    [Header("スコアボードキャンバス")]         public Canvas        sb_canvas    ; // スコアボードキャンバス
    [Header("フェードスピード(秒)")]           public float         fadeTime     ; // スコアボードフェードタイム
    [Header("GameManager")]                    public GameManager   gameManager  ; // ゲームマネージャー
    [Header("RegisterScore")]                  public RegisterScore registerScore; // スコア
    [Header("GameTimer")]                      public GameTimer     gameTimer    ; // ゲームタイマー
    [Header("カートプレハブ")]                 public GameObject    cartPrefab   ; // カートプレハブ
    [Header("単位テキスト")]                   public Text          UnitText     ; // 単位テキスト
    [Header("スコアテキスト")]                 public Text          scoreText    ; // スコアテキスト
    [Header("タイムテキスト")]                 public Text          timeText     ; // タイムテキスト
    [Header("カウントダウンテキスト")]         public Text          countDown    ; // ウントダウンテキスト
    [Header("高さスライダー")]                 public Slider        heightSlider ; // 高さスライダー
    [Header("高さスライダーテキスト")]         public Text          heightText   ; // 高さテキスト
    [Header("高さスライダーの表示上限値")]     public float         heightLimit  ; // スライダーの上限値
    [Header("スコアボード")]                   public GameObject    scoreBoard   ; // スコアボード
    [Header("(スコアボード)ランクテキスト")]   public Text          sb_rankText  ; // (スコアボード)ランクテキスト
    [Header("(スコアボード)スコアテキスト")]   public Text          sb_scoreText ; // (スコアボード)スコアテキスト
    [Header("(スコアボード)タイムテキスト")]   public Text          sb_timeText  ; // (スコアボード)タイムテキスト
    [Header("(スコアボード)スタックテキスト")] public Text          sb_stackText ; // (スコアボード)スタックテキスト

    //-----------------------------------------------------------------------------
    //! [内容]    RPC受信関数(現在のスコア)
    //-----------------------------------------------------------------------------
    [MunRPC]
    void RecvHeight(float senderHeight) {
        height = senderHeight;
    }

    //-----------------------------------------------------------------------------
    //! [内容]    開始処理
    //-----------------------------------------------------------------------------
    void Start() {
        // インスペクターで指定されているかチェック
        if (!uiCanvas)      Debug.LogError("uiCanvasが指定されていません。");
        if (!sb_canvas)     Debug.LogError("scoreBoardCanvasが指定されていません。");
        if (!registerScore) Debug.LogError("RegisterScoreが指定されていません。");
        if (!gameTimer)     Debug.LogError("ゲームタイマーが指定されていません。");
        if (!UnitText)      Debug.LogError("単位テキストが指定されていません。");
        if (!scoreText)     Debug.LogError("スコアテキストが指定されていません。");
        if (!timeText)      Debug.LogError("タイムテキストが指定されていません。");
        if (!countDown)     Debug.LogError("カウントダウンテキストが指定されていません。");
        if (!heightSlider)  Debug.LogError("高さスライダーが指定されていません。");
        if (!scoreBoard)    Debug.LogError("スコアボードが指定されていません。");
        if (!sb_rankText)   Debug.LogError("(スコアボード)ランクテキストが指定されていません。");
        if (!sb_scoreText)  Debug.LogError("(スコアボード)スコアテキストが指定されていません。");
        if (!sb_timeText)   Debug.LogError("(スコアボード)タイムテキストが指定されていません。");
        if (!sb_stackText)  Debug.LogError("(スコアボード)スタックテキストが指定されていません。");
        if (!heightText)    Debug.LogError("高さテキストが指定されていません。");
        else {
            // 高さテキストの子を一括取得
            heightTextComs = new Text[heightText.transform.childCount + 1];
            heightTextComs[0] = heightText;
            for (int i = 0; i < heightText.transform.childCount; i++) {
                var text = heightText.transform.GetChild(i);
                if (text) {
                    heightTextComs[i + 1] = text.GetComponent<Text>();
                }
            }
        }
        // キャンバスグループを取得
        uiCanvasGroup = uiCanvas.GetComponent<CanvasGroup>();
        sb_canvasGroup = sb_canvas.GetComponent<CanvasGroup>();
        if (!(uiCanvasGroup || sb_canvasGroup)) {
            Debug.LogError("キャンバスグループが見つかりません。");
        }

        if (!this.TryGetComponent<GameUISound>(out gameUiSound)) {
            Debug.LogError("InGameUiSOundがありません。");
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {
        if (MonobitNetwork.inRoom) {
            if (MonobitNetwork.isHost && isHost) {
                if (cartPrefab && !stackTree) {
                    cartObject = GameObject.Find(cartPrefab.name + "(Clone)");
                    // 見つかった場合スタックツリーコンポーネントを取得
                    if (cartObject) {
                        stackTree = cartObject.GetComponentInChildren<StackTree>();
                        if (!stackTree) {
                            Debug.LogError(cartObject.name + "にスタックツリーが見つかりません。");
                        }
                    }
                }
                if (stackTree) {
                    isHost = true;
                    this.monobitView.RPC("RecvHeight", MonobitTargets.All, stackTree.GetHeight());
                }
            }
            else {
                isHost = false;
            }
        }

        if (gameManager.IsPlaying()) {

            if (!uiCanvas.gameObject.activeSelf) {
                uiCanvas.gameObject.SetActive(true);
                // カウントダウンSEを再生
                gameUiSound.PlaySE(GameUISound.SETYPE.COUNTDOWN);
            }

            // カウントダウン
            if (gameTimer.startCount > 0.0f) {
                countDown.text = (gameTimer.startCount + 1.0f).ToString();
            }
            else {
                if (countDown.enabled) {
                    // BGM再生
                    //gameUiSound.PlayBGM(GameUISound.BGMTYPE.DOTABATA_FAST);
                }
                countDown.enabled = false;
            }

            // 残り時間から分と秒を計算
            var limitCount = gameTimer.limitCount;
            int minutes    = Mathf.FloorToInt(limitCount / 60.0f);               // 分
            int seconds    = Mathf.FloorToInt(limitCount - minutes * 60.0f);     // 秒
            timeText.text  = string.Format("{0:00}:{1:00}", minutes, seconds);

            // 残り時間3秒以下でSE再生&文字色を赤に
            if (limitCount <= 3.0f) {
                timeText.color = new Color(1.0f, 0.0f, 0.0f);
                if (!isPlayTimeUpSound) {
                    gameUiSound.PlaySE(GameUISound.SETYPE.TIMEUP);
                    isPlayTimeUpSound = true;
                }
            }

            // スコア表示
            var score = registerScore.scoreData.totalScore;
            score = score > 9999999 ? 9999999 : score;
            scoreText.text = string.Format("{0:00000#,0}", score);

            // 高さを表示
            if (heightLimit != 0) {

                var valueHeight = height < 1.0f ? 0.0f : height - 1.0f;
                heightSlider.value = valueHeight <= heightLimit ? valueHeight / heightLimit : 1.0f;

                // 高さテキストの表示変更
                for (int i = 0; i < heightTextComs.Length; i++) {
                    float heightNum = (i / 5.0f * heightLimit) + 1.0f;
                    if (i == heightTextComs.Length - 1) {
                        if (heightNum < height) {
                            heightNum = height;
                        }
                    }
                    string heightStr = string.Format("{0:0.00}", heightNum) + "×";
                    heightTextComs[(heightTextComs.Length - 1) - i].text = heightStr;
                }
            }
        }
        else if (gameTimer.limitCount <= 0.0f) {
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
                var currentTime  = gameTimer.currentGameTime;
                int minutes      = Mathf.FloorToInt(currentTime / 60.0f);              // 分
                int seconds      = Mathf.FloorToInt(currentTime - minutes * 60.0f);    // 秒
                sb_timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                // スタック
                var stack = registerScore.scoreData.totalStack;
                stack = stack > 9999 ? 9999 : stack;
                sb_stackText.text = string.Format("{0:00#,0}", stack);
            }

            // フェード処理
            if (sb_canvasGroup.alpha < 1.0f && fadeTime != 0.0f) {
                timeCount += Time.deltaTime;
                sb_canvasGroup.alpha = timeCount / fadeTime;
                uiCanvasGroup.alpha = 1.0f - (timeCount / fadeTime);
            }
            else {
                sb_canvasGroup.alpha = 1.0f;
                uiCanvasGroup.alpha = 0.0f;
            }
        }
    }
}
