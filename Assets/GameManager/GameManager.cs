using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------
//! [制作者]		小野龍大
//!	[最終更新日]	2021/10/12
//! [内容]		ゲームを制御するクラスを橋渡しをするクラス
//-----------------------------------------------------------------------------
public class GameManager : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //!	private変数
    //-----------------------------------------------------------------------------
    [SerializeField] StackTree stackTree = null;       // StackTreeがアタッチされているオブジェクト
    [SerializeField] NetworkManager networkManager = null;     // ネットワークマネージャ
    [SerializeField] RegisterScore registerScore = null;     // ネットワークマネージャ
    [SerializeField] RealTimeTextManager realTimeTextManager = null;
    int lastDisplayScore = 0;

    bool playing = false;

    //-----------------------------------------------------------------------------
    //!	public変数
    //-----------------------------------------------------------------------------

    //-----------------------------------------------------------------------------
    //! [内容]		生成時
    //-----------------------------------------------------------------------------
    private void Awake()
    {
        
    }

    private void FixedUpdate()
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

            realTimeTextManager.EnqueueText(textInfo);

            textInfo.text = "合計スコア : " + registerScore.scoreData.totalScore.ToString();
            realTimeTextManager.EnqueueText(textInfo);
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
