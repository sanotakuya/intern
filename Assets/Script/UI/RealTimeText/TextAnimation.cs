using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//-----------------------------------------------------------------------------
//! [制作者]		小野龍大
//!	[最終更新日]	2021/10/15
//! [内容]		テキストのアニメーションを行うクラス
//!             テクストメッシュプロがアタッチされているテキストオブジェクトにアタッチして利用
//-----------------------------------------------------------------------------
public class TextAnimation : MonoBehaviour
{
    public enum AnimStyle
    {
        None,
        WaveColor,
        WavePosition

    }

    public AnimStyle animStyle = AnimStyle.WaveColor;

    //-----------------------------------------------------------------------------
    //!	private変数
    //-----------------------------------------------------------------------------
    private TMP_Text text;
    [SerializeField] private Gradient gradientColor;
    [SerializeField] private float widthMul = 10;
    private TMP_TextInfo textInfo;
    

    private void Update()
    {
        if (this.text == null)
        {
            this.text = GetComponent<TMP_Text>();
        }

        UpdateAnim();

    }

    private void UpdateAnim()
    {
        // メッシュをリセット
        text.ForceMeshUpdate(true);
        this.textInfo = text.textInfo;

        switch (animStyle)
        {
            case AnimStyle.WaveColor:

                WaveColor();

                break;
            case AnimStyle.WavePosition:

                WavePosition();

                break;
            default:
                break;
        }
    }

    private void WaveColor()
    {
        // 文字数を取得する
        int count = Mathf.Min(this.textInfo.characterCount, this.textInfo.characterInfo.Length);


        // 頂点データを編集する
        for (int i = 0; i < count; i++)
        {
            var charInfo = this.textInfo.characterInfo[i];

            // 表示されているか(されていないならスキップ)
            if (!charInfo.isVisible)
            {
                continue;
            }

            // 頂点数を取得する
            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            // Gradientカラーを配列に入れていく
            Color32[] colors = textInfo.meshInfo[materialIndex].colors32;

            float timeOffset = -0.5f * i;
            float time1 = Mathf.PingPong(timeOffset + Time.realtimeSinceStartup, 1.0f);
            float time2 = Mathf.PingPong(timeOffset + Time.realtimeSinceStartup - 0.1f, 1.0f);

            colors[vertexIndex + 0] = gradientColor.Evaluate(time1); // 左下
            colors[vertexIndex + 1] = gradientColor.Evaluate(time1); // 左上
            colors[vertexIndex + 2] = gradientColor.Evaluate(time2); // 右上
            colors[vertexIndex + 3] = gradientColor.Evaluate(time2); // 右下
        }

        // メッシュを更新
        for (int i = 0; i < this.textInfo.materialCount; i++)
        {
            if (this.textInfo.meshInfo[i].mesh == null)
            {
                continue;
            }

            this.textInfo.meshInfo[i].mesh.colors32 = this.textInfo.meshInfo[i].colors32;
            text.UpdateGeometry(this.textInfo.meshInfo[i].mesh, i);
        }
    }

    private void WavePosition()
    {
        // 頂点データを編集した配列の作成
        int count = Mathf.Min(textInfo.characterCount, textInfo.characterInfo.Length);

        // 頂点データを編集する
        for (int i = 0; i < count; i++)
        {
            var charInfo = this.textInfo.characterInfo[i];

            // 表示されているか(されていないならスキップ)
            if (!charInfo.isVisible)
            {
                continue;
            }

            // 頂点数を取得する
            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            // Wave
            Vector3[] verts = textInfo.meshInfo[materialIndex].vertices;

            float sinWaveOffset = 0.5f * i;
            float sinWave = Mathf.Sin(sinWaveOffset + Time.realtimeSinceStartup * Mathf.PI);
            verts[vertexIndex + 0].y += sinWave * widthMul;
            verts[vertexIndex + 1].y += sinWave * widthMul;
            verts[vertexIndex + 2].y += sinWave * widthMul;
            verts[vertexIndex + 3].y += sinWave * widthMul;
        }

        // メッシュを更新
        for (int i = 0; i < textInfo.materialCount; i++)
        {
            if (this.textInfo.meshInfo[i].mesh == null)
            {
                continue;
            }

            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;  // 変更
            text.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

}
