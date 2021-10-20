using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]		小野龍大
//!	[最終更新日]	2021/10/12
//! [内容]		ゲームを制御するクラスを橋渡しをするクラス
//-----------------------------------------------------------------------------
public class GameManager : MonobitEngine.MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //!	private変数
    //-----------------------------------------------------------------------------
    [SerializeField] StackTree stackTree = null;       // StackTreeがアタッチされているオブジェクト
    [SerializeField] NetworkManager networkManager = null;     // ネットワークマネージャ
    [SerializeField] RegisterScore registerScore = null;     // ネットワークマネージャ
    [SerializeField] GameTimer gameTimer = null;     // ネットワークマネージャ


    [SerializeField] RealTimeTextManager _realTimeTextManager = null;
    public RealTimeTextManager realTimeTextManager
    {  
        get { return _realTimeTextManager; }
        set { _realTimeTextManager = realTimeTextManager; }
    }

    static MonobitEngine.MonobitView monoBitView = null;

    int lastDisplayScore = 0;       // 最後に表示したときのスコア
    MonobitPlayer[] beforeMonobitPlayer;

    bool inRoom = false;
    bool playing = false;

    //-----------------------------------------------------------------------------
    //!	public変数
    //-----------------------------------------------------------------------------

    //-----------------------------------------------------------------------------
    //! [内容]		参加受信関数
    //-----------------------------------------------------------------------------
    [MunRPC]
    void RecvRoomText(string text)
    {
        RealTimeTextManager.TextInfo textInfo = new RealTimeTextManager.TextInfo();
        textInfo.SetDefault();

        textInfo.text = text;

        GetComponent<GameManager>().realTimeTextManager.EnqueueText(textInfo);
    }

    //-----------------------------------------------------------------------------
    //! [内容]		開始時
    //-----------------------------------------------------------------------------
    private void Start()
    {
        monoBitView = GetComponent<MonobitEngine.MonobitView>();
        EnterOneself();
    }

    private void Update()
    {
        // スコアが変化していたら表示
        if(lastDisplayScore != registerScore.scoreData.totalScore)
        {
            lastDisplayScore = registerScore.scoreData.totalScore;

            RealTimeTextManager.TextInfo textInfo = new RealTimeTextManager.TextInfo();
            textInfo.SetDefault();

            textInfo.text = "ボーナス内容　:";

            foreach(string str in registerScore.scoreData.bonusNameList)
            {
                textInfo.text += str;
            }

            _realTimeTextManager.EnqueueText(textInfo);

            textInfo.text = "合計スコア : " + registerScore.scoreData.totalScore.ToString();
            _realTimeTextManager.EnqueueText(textInfo);
        }

        // 入室状態に移行
        if(!inRoom && MonobitNetwork.inRoom)
        {
            monobitView.RPC(
                "RecvRoomText", 
                MonobitEngine.MonobitTargets.All, 
                (string)(MonobitNetwork.playerName + "が入室しました")
                );

            inRoom = true;
        }

        if(Input.GetKeyDown(KeyCode.Return) && MonobitNetwork.isHost && !playing)
        {
            GameStart();
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]		StackTreeのSet
    //-----------------------------------------------------------------------------
    public void SetStackTree(ref StackTree _stackTree)
    {
        stackTree = _stackTree;
    }

    //-----------------------------------------------------------------------------
    //! [内容]		NetworkManagerのSet
    //-----------------------------------------------------------------------------
    public void SetNetworkManager(ref NetworkManager _networkManager)
    {
        networkManager = _networkManager;
    }

    //-----------------------------------------------------------------------------
    //! [内容]		RegisterScoreのSet
    //-----------------------------------------------------------------------------
    public void SetRegisterScore(ref RegisterScore _registerScore)
    {
        registerScore = _registerScore;
    }

    //-----------------------------------------------------------------------------
    //! [内容]		GameManagerがゲーム開始の準備ができているか確認する
    //! [return]    準備ができていたらtrue,できていなかったらfalse 
    //-----------------------------------------------------------------------------
    bool IsReady()
    {
        if(!stackTree)
        {
            return false;
        }

        if(!networkManager)
        {
            return false;
        }

        if(!registerScore)
        {
            return false;
        }

        return true;
    }

    //-----------------------------------------------------------------------------
    //! [内容]		現在プレイ中か判断する
    //! [return]   プレイ中ならtrue,そうじゃなかったらfalse 
    //-----------------------------------------------------------------------------
    bool IsPlaying()
    {
        return playing;
    }

    //-----------------------------------------------------------------------------
    //! [内容]		ゲーム開始時にコール
    //-----------------------------------------------------------------------------
    void GameStart()
    {

        // プレイ中フラグを立てる
        playing = true;

        // 計測と始める
        gameTimer.GameStart();
    }

    //-----------------------------------------------------------------------------
    //! [内容]		ゴールに辿り着いたときコール
    //-----------------------------------------------------------------------------
    void Goal()
    {

    }

    //-----------------------------------------------------------------------------
    //! [内容]		ゲーム開終了時にコール
    //-----------------------------------------------------------------------------
    void GameEnd()
    {

        // プレイ中フラグを折る
        playing = false;
    }

    //-----------------------------------------------------------------------------
    //! [内容]		入室時にコール
    //-----------------------------------------------------------------------------
    public void EnterOneself()
    {
        monobitView.RPC("RecvRoomText", MonobitEngine.MonobitTargets.All, (string)(MonobitNetwork.playerName +"が入室しました"));
    }

    //-----------------------------------------------------------------------------
    //! [内容]		退出時にコール
    //-----------------------------------------------------------------------------
    public void ExitOneself()
    {
        monobitView.RPC("RecvRoomText", MonobitEngine.MonobitTargets.All, MonobitNetwork.playerName + "が退出しました");
    }

    //-----------------------------------------------------------------------------
    //! [内容]		プレイヤー入室時にコール
    //-----------------------------------------------------------------------------
    void EnterPlayer()
    {

    }

    //-----------------------------------------------------------------------------
    //! [内容]		プレイヤー退出時にコール
    //-----------------------------------------------------------------------------
    void ExitPlayer()
    {

    }

    //-----------------------------------------------------------------------------
    //! [内容]		stackTreeの参照を返す
    //-----------------------------------------------------------------------------
    ref StackTree GetStackTree()
    {
        return ref stackTree;
    }
}
