using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//-----------------------------------------------------------------------------
//! [制作者]		小野龍大
//!	[最終更新日]	2021/10/15
//! [内容]		リアルタイムテキストの管理
//-----------------------------------------------------------------------------
public class RealTimeTextManager : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //!	def
    //-----------------------------------------------------------------------------

    public enum FontType
    {
        K8x12,
        LightNovel,
        Zomzi
    }

    // メッセージ表示依頼用構造体
    public struct TextInfo
    {
        public string text;        // 表示するテキスト
        public Color color;        // 色
        public float lifeTime;     // 表示時間
        public float fontSize;     // 表示サイズ
        public FontType fontType;
        public TextAnimation.AnimStyle animStyle;

        public void SetDefault()
        {
            text = "デフォルト設定です";
            color.r = 1;
            color.g = 1;
            color.b = 1;
            color.a = 1;

            lifeTime = 3;
            fontSize = 64;
            fontType = FontType.LightNovel;
            animStyle = TextAnimation.AnimStyle.None;
        }
    }

    //-----------------------------------------------------------------------------
    //!	Private変数
    //-----------------------------------------------------------------------------

    // テキスト表示
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private GameObject canvas;
    [SerializeField] private List<TMP_FontAsset> fontList = new List<TMP_FontAsset>();
    private GameObject textObj;
    private TextMeshProUGUI text;


    // マネージ用変数
    private List<TextInfo> textQueue = new List<TextInfo>();
    private float elapsedTime;
    private float lifeTime;


    void Start()
    {
        
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        if(elapsedTime > lifeTime)
        {
            if(textObj)
            {
                Destroy(textObj);
            }

            // キューが残っているなら
            if (textQueue.Count > 0)
            {
                DequeueText();

            }
        }
    }


    //-----------------------------------------------------------------------------
    //! [内容]		リアルタイムテキストのキューに追加
    //-----------------------------------------------------------------------------
    public void EnqueueText(TextInfo textInfo)
    {
        textQueue.Add(textInfo);        // 最後に追加
    }

    //-----------------------------------------------------------------------------
    //! [内容]		キューから取り出してテキストを表示する
    //-----------------------------------------------------------------------------
    private void DequeueText()
    {


        //テキストを生成してCanvasに追加
        textObj = Instantiate(textPrefab);
        textObj.transform.parent = canvas.transform;
        text = textObj.GetComponent<TextMeshProUGUI>();

        // 新しいテキスト情報に変更
        TextInfo info = textQueue[0];
        text.text = info.text;
        text.color = info.color;
        text.fontSize = info.fontSize;

        text.font = fontList[(int)info.fontType];

        textObj.GetComponent<TextAnimation>().animStyle = info.animStyle;

        // 表示時間の設定
        lifeTime = info.lifeTime;
        elapsedTime = 0;

        textQueue.RemoveAt(0);      //　先頭を削除
    }  
}

