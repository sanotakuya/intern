using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/20
//! [内容]       数が変化した時のエフェクト
//-----------------------------------------------------------------------------
public class ScoreDisplay : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! private変数
    //-----------------------------------------------------------------------------
    private RectTransform rectTransform; // キャンバスのトランスフォーム
    private Text          textCom      ; // テキストコンポーネント
    private float         timeCount    ; // 時間計測
    private float         moveY        ; // Yの移動量

    //-----------------------------------------------------------------------------
    //! public変数
    //-----------------------------------------------------------------------------


    //-----------------------------------------------------------------------------
    //! Inspectorに公開する変数
    //-----------------------------------------------------------------------------
    [Header("ターゲットオブジェクト")] public Transform target     ; // ターゲットオブジェクトのトランスフォーム
    [Header("上昇スピード")]           public float     upwardSpeed; // 上昇スピード
    [Header("フェードスピード")]       public float     fadeTime   ; // フェードスピード

    //-----------------------------------------------------------------------------
    //! [内容]    開始処理
    //-----------------------------------------------------------------------------
    void Start()
    {
        // コンポーネントを取得
        rectTransform = this.GetComponent<RectTransform>();
        textCom       = this.GetComponent<Text>();
        if (!rectTransform) Debug.LogError("RectTransfromが見つかりません。");
        if (!textCom)       Debug.LogError("テキストコンポーネントが見つかりません。");
        if (!target)        Debug.LogError("ターゲットのトランスフォームが見つかりません。");
    }

    //-----------------------------------------------------------------------------
    //! [内容]    更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {
        // 座標移動
        moveY                  += upwardSpeed * Time.deltaTime;
        rectTransform.position  = RectTransformUtility.WorldToScreenPoint(Camera.main, target.position) + new Vector2(0.0f,moveY);
        // フェード処理
        timeCount += Time.deltaTime;
        var color = textCom.color;
        color.a = 1.0f - timeCount / fadeTime;
        textCom.color = color;
        // アルファ値が0以下になった場合は親を含め全て削除する
        if (textCom.color.a <= 0.0f) {
            Destroy(this.gameObject.transform.root.gameObject);
        }
    }
}
