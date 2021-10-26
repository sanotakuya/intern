using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;
using UnityEngine.UI;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/25
//! [内容]       画面外に出たプレイヤーの表示
//-----------------------------------------------------------------------------
public class OutSidePlayerNameDisplay : MonobitEngine.MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! 構造体
    //-----------------------------------------------------------------------------
    struct Player
    {
        public string        name      ; // 名前
        public GameObject    gameObject; // ゲームオブジェクト
        public RectTransform textTrans ; // テキストオブジェクト
        public Text          text      ; // テキスト

        public Player(string name, GameObject gameObject, RectTransform textTrans, Text text) {
            this.name       = name;
            this.gameObject = gameObject;
            this.textTrans  = textTrans;
            this.text       = text;
        }
    }

    //-----------------------------------------------------------------------------
    //! private変数
    //-----------------------------------------------------------------------------
    private static GameObject   canvasObject                           ; // キャンバスオブジェクト
    private        GameObject   triangle                               ; // 方向表示用三角形
    private static List<Player> players            = new List<Player>(); // プレイヤーリスト
    private        int          currentPlayerCount = 0                 ; // 現在のプレイヤー数
    private        Camera       mainCamera                             ; // カメラ
    private        Rect         rect               = new Rect(0,0,1,1) ;

    //-----------------------------------------------------------------------------
    //! public変数
    //-----------------------------------------------------------------------------
    public int OutSidePlayerCount { // 画面外のプレイヤー数
        get { return 0; }
    }

    //-----------------------------------------------------------------------------
    //! Inspectorに公開する変数
    //-----------------------------------------------------------------------------
    [Header("テキストプレハブ")]   public GameObject  textPrefab                      ;
    [Header("テキストカラー")]     public List<Color> textCol      = new List<Color>();
    [Header("透過度")]             public float       transparency = 0.5f             ;

    //-----------------------------------------------------------------------------
    //! [内容]    初期処理
    //-----------------------------------------------------------------------------
    void Awake()
    {
        mainCamera   = Camera.main;
        canvasObject = GameObject.Find("OutSideName");
        if (!textPrefab)   Debug.LogError("テキストプレハブが設定されていません。");
        else {
            if (!textPrefab.TryGetComponent<Text>(out var text)) {
                Debug.LogError("テキストコンポーネントがありません。");
            }
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {
        for (int i = 0; i < players.Count; i++) {
            var player = players[i];
            var viewport = mainCamera.WorldToViewportPoint(player.gameObject.transform.position);
        
            // Yは常に表示しない
            if (rect.Contains(new Vector2(viewport.x, 0.5f))) {
                player.text.enabled = false;
            }
            else {
                player.text.enabled = true;

                // UI調整
                Rect canvasRect = ((RectTransform)canvasObject.transform).rect;
                canvasRect.Set(
                         canvasRect.x      + player.textTrans.rect.width  * 0.5f
                        ,canvasRect.y      + player.textTrans.rect.height * 0.5f
                        ,canvasRect.width  - player.textTrans.rect.width
                        ,canvasRect.height - player.textTrans.rect.height
                    );

                // 画面内でプレイヤーを追従
                viewport.x = Mathf.Clamp01(viewport.x);
                viewport.y = Mathf.Clamp01(viewport.y);
                player.textTrans.anchoredPosition = Rect.NormalizedToPoint(canvasRect, viewport);
            }

            if (i < textCol.Count) {
                Color color = new Color(textCol[i].r, textCol[i].g, textCol[i].b, transparency);
                player.text.color = color;
            }
            else {
                player.text.color = new Color(1.0f, 1.0f, 1.0f, transparency);
            }
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    プレイヤーを追加する(コールバック)
    //-----------------------------------------------------------------------------
    public void OnJoinPlayer(MonobitView monobitView)
    {
        if (monobitView) {

            // テキストを生成
            if (canvasObject && textPrefab) {
                var text = Instantiate(textPrefab,Vector3.zero,Quaternion.identity) as GameObject;
                text.transform.parent = canvasObject.transform;
                text.GetComponent<RectTransform>().position = Vector3.zero;
                text.GetComponent<Text>().text = monobitView.gameObject.name;
                // リストに追加
                players.Add(new Player(monobitView.name
                                      ,monobitView.gameObject
                                      ,text.GetComponent<RectTransform>()
                                      ,text.GetComponent<Text>()
                                      ));
            }

        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    プレイヤーを削除する(コールバック)
    //-----------------------------------------------------------------------------
    public void OnLeavePlayer(MonobitView monobitView)
    {
        if (monobitView) {
            var player = players.Find(p => p.name == monobitView.gameObject.name);
            if (player.name != "") {
                players.Remove(player);
            }
        }
    }
}