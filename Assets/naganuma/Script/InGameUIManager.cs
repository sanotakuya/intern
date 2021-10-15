﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/15
//! [内容]       インゲームのUI
//-----------------------------------------------------------------------------
public class InGameUIManager : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! private変数
    //-----------------------------------------------------------------------------

    //-----------------------------------------------------------------------------
    //! public変数
    //-----------------------------------------------------------------------------

    //-----------------------------------------------------------------------------
    //! Inspectorに公開する変数
    //-----------------------------------------------------------------------------
    [Header("RegisterScore")]  public RegisterScore registerScore; // スコア
    [Header("GameTimer")]      public GameTimer     gameTimer    ; // ゲームタイマー
    [Header("単位テキスト")]   public Text          UnitText     ; // 単位テキスト
    [Header("スコアテキスト")] public Text          scoreText    ; // スコアテキスト
    [Header("タイムテキスト")] public Text          timeText     ; // タイムテキスト

    //-----------------------------------------------------------------------------
    //! [内容]    開始処理
    //-----------------------------------------------------------------------------
    void Start()
    {
        // インスペクターで指定されているかチェック
        //if (!registerScore) Debug.LogError("RegisterScoreが指定されていません。");
        if (!gameTimer)     Debug.LogError("ゲームタイマーが指定されていません。");
        if (!UnitText)      Debug.LogError("単位テキストが指定されていません。");
        if (!scoreText)     Debug.LogError("スコアテキストが指定されていません。");
        if (!timeText)      Debug.LogError("タイムテキストが指定されていません。");
    }

    //-----------------------------------------------------------------------------
    //! [内容]    更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {
        // 残り時間から分と秒を計算
        int minutes = Mathf.FloorToInt(gameTimer.limitCount / 60.0f)          ; // 分
        int seconds = Mathf.FloorToInt(gameTimer.limitCount - minutes * 60.0f); // 秒
        timeText.text = string.Format("{0:00}:{0:00}", minutes, seconds);
    }
}
