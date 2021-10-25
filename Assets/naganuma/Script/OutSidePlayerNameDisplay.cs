﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/25
//! [内容]       画面外に出たプレイヤーの表示
//-----------------------------------------------------------------------------
public class OutSidePlayerNameDisplay : MonobitEngine.MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! private変数
    //-----------------------------------------------------------------------------
    private GameObject          triangle              ; // 方向表示用三角形
    private List<MonobitPlayer> players               ; // プレイヤーリスト
    private int                 currentPlayerCount = 0; // 現在のプレイヤー数

    //-----------------------------------------------------------------------------
    //! public変数
    //-----------------------------------------------------------------------------
    public int OutSidePlayerCount { // 画面外のプレイヤー数
        get { return 0; }
    }

    //-----------------------------------------------------------------------------
    //! Inspectorに公開する変数
    //-----------------------------------------------------------------------------
    [Header("テキストカラー")]     public List<Color> textCol      = new List<Color>();
    [Header("透過度")]             public float       transparency = 0.5f             ;

    //-----------------------------------------------------------------------------
    //! [内容]    開始処理
    //-----------------------------------------------------------------------------
    void Start()
    {

    }

    //-----------------------------------------------------------------------------
    //! [内容]    更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {

    }

    //-----------------------------------------------------------------------------
    //! [内容]    プレイヤーを取得する(コールバック)
    //-----------------------------------------------------------------------------
    public void OnJoinPlayer(GameObject playerObject)
    {

    }
}

