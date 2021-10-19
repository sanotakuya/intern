using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/15
//! [内容]       ゲームの時間管理
//-----------------------------------------------------------------------------
public class GameTimer : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! private変数
    //-----------------------------------------------------------------------------
    private float currentTime      = 0.0f ; // 現在の時間
    private float _currentGameTime = 0.0f ; // ゲームが始まってからの時間
    private bool  _isPlayable      = false; // プレイ可能か

    //-----------------------------------------------------------------------------
    //! public変数
    //-----------------------------------------------------------------------------
    public float currentGameTime {         // ゲームが開始されてからの時間
        get { return _currentGameTime; }
    }

    public bool  isPlayable {              // プレイ可能か
        get { return _isPlayable; }
    }

    public float startCount {              // 現在のゲーム開始までの時間
        get {
            // ゲーム開始までの時間を算出
            var startCnt = startTime - currentTime;
            // 0秒以下の場合は0を返す
            return startCnt <= 0.0f ? 0.0f : startCnt;
        }
    }

    public float limitCount {              // 制限時間までの時間
        get {
            // 制限時間までの時間を算出
            var limitCnt = limitTime - currentGameTime;
            // 0秒以下の場合は0を返す
            return limitCnt <= 0.0f ? 0.0f : limitCnt;
        }
    }

    private bool isStart = false; // プレイ開始

    //-----------------------------------------------------------------------------
    //! Inspectorに公開する変数
    //-----------------------------------------------------------------------------
    [Header("GameManager")]        public GameManager gameManager        ; // ゲームマネージャー
    [Header("開始するまでの時間")] public float       startTime   = 1.0f ; // 開始するまでの時間
    [Header("制限時間")]           public float       limitTime   = 10.0f; // 制限時間

    //-----------------------------------------------------------------------------
    //! [内容]    開始処理
    //-----------------------------------------------------------------------------
    void Start()
    {
        gameManager = this.GetComponent<GameManager>();

        // TODO: 現在はテストのため必要なし
        if (gameManager && false) {
            Debug.LogError("ゲームマネージャーが設定されていません。");
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {

        // 時間計測
        if (isStart) {
            currentTime += Time.deltaTime;
        }

        // プレイ可能なタイミングから計測開始
        if (isPlayable) {
            _currentGameTime += Time.deltaTime;
        }

        // 開始までのカウントが0ならプレイ可能
        if (startCount == 0.0f) {
            _isPlayable = true;
        }

        // 終了までのカウントが0なら終了
        if (limitCount == 0.0f) {
            // TODO:終了時処理
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    ゲーム開始
    //-----------------------------------------------------------------------------
    void TimeStart() {
        isStart = true;
    }
}
