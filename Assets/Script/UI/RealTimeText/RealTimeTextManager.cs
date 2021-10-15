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
    public struct RealTimeTextInfo
    {
        public string text;        // 表示するテキスト
        public Color color;        // 色
        public float lifeTime;     // 表示時間
        public float fontSize;     // 表示サイズ
        public FontType fontType;

        public void SetDefault()
        {
            text = "デフォルト設定です";
            color.r = 1;
            color.g = 1;
            color.b = 1;

            lifeTime = 4;
            fontSize = 64;
            fontType = FontType.LightNovel;
        }
    }

    //-----------------------------------------------------------------------------
    //!	Private変数
    //-----------------------------------------------------------------------------

    // テキスト表示
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private GameObject canvas;
    [SerializeField] private List<TMP_FontAsset> fontList = new List<TMP_FontAsset>();
    private TextMeshProUGUI text;

    // マネージ用変数
    private List<RealTimeTextInfo> textQueue = new List<RealTimeTextInfo>();
    private float elapsedTime;
    private float lifeTime;


    void Start()
    {
        GameObject obj = Instantiate(textPrefab);
        obj.transform.parent = canvas.transform;

        text = obj.GetComponent<TextMeshProUGUI>();

        RealTimeTextInfo info = new RealTimeTextInfo();
        info.SetDefault();
        info.text = "1回目の表示";
        info.color = new Color(1, 0, 0);
        info.lifeTime = 3;
        EnqueueText(info);

        info.text = "2回目の表示";
        info.color = new Color(0, 1, 0);
        info.fontSize *= 2;
        info.fontType = FontType.K8x12;
        EnqueueText(info);

        info.text = "3回目の表示";
        info.color = new Color(0, 0, 1);
        info.fontSize *= 2;
        info.fontType = FontType.Zomzi;
        EnqueueText(info);
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        if(elapsedTime > lifeTime)
        {
            // キューが残っているなら
            if(textQueue.Count > 0)
            {
                DequeueText();
            }
        }
    }


    //-----------------------------------------------------------------------------
    //! [内容]		リアルタイムテキストのキューに追加
    //-----------------------------------------------------------------------------
    public void EnqueueText(RealTimeTextInfo textInfo)
    {
        textQueue.Add(textInfo);        // 最後に追加
    }

    //-----------------------------------------------------------------------------
    //! [内容]		キューから取り出してテキストを表示する
    //-----------------------------------------------------------------------------
    private void DequeueText()
    {
        // 新しいテキスト情報に変更
        RealTimeTextInfo info = textQueue[0];
        text.text = info.text;
        text.color = info.color;
        text.fontSize = info.fontSize;

        text.font = fontList[(int)info.fontType];

        // 表示時間の設定
        lifeTime = info.lifeTime;
        elapsedTime = 0;

        textQueue.RemoveAt(0);      //　先頭を削除
    }
}

