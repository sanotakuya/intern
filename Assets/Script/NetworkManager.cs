using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;
using System;


//-----------------------------------------------------------------------------
//! [制作者]		小野龍大
//!	[最終更新日]	2021/10/06
//! [内容]		ネットワークの管理をする
//-----------------------------------------------------------------------------

public class NetworkManager : MonobitEngine.MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //!	private変数
    //-----------------------------------------------------------------------------
    private string roomName = "";       // ルーム名
    private bool isCharacterCreate = false;       // 自キャラを生成したか
    private int myCharacterID = -1;
    private MonobitPlayer hostPlayer;

    private bool leaveHost = false;
    private float time = 0;

    struct RoomPlayer
    {
        public MonobitPlayer player;
        public int characterID;
    }
    List<RoomPlayer> roomPlayers = new List<RoomPlayer>();

    MonobitPlayer[] beforePlayers = new MonobitPlayer[0];

    [SerializeField] private int updateRate = 30;
    [SerializeField] private uint maxPlayers = 4;
    [SerializeField] private string lobbySceneName = "Lobby";
    [SerializeField] private string RoomSceneName = "Room";
    //-----------------------------------------------------------------------------
    //! [内容]		参加受信関数
    //-----------------------------------------------------------------------------
    [MunRPC]
    void RecvEnterRoomText(string playerName,int playerID, int characterID)
    {
        RealTimeTextManager.TextInfo textInfo = new RealTimeTextManager.TextInfo();
        textInfo.SetDefault();

        textInfo.text = playerName + "が入室しました";
        textInfo.animStyle = TextAnimation.AnimStyle.WavePosition;

        GetComponent<GameManager>().realTimeTextManager.EnqueueText(textInfo);

        GameObject obj = MonobitView.Find(characterID).gameObject;
        obj.name = playerName;

        // 自分の事だったら
        if (playerID == MonobitNetwork.player.ID)
        {
            myCharacterID = characterID;

            //obj.GetComponent<MovePlayer>().myCharactor = true;
            
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]		参加受信関数
    //-----------------------------------------------------------------------------
    [MunRPC]
    void RecvExitRoomText(string playerName)
    {
        RealTimeTextManager.TextInfo textInfo = new RealTimeTextManager.TextInfo();
        textInfo.SetDefault();

        textInfo.text = playerName + "が退出しました";
        textInfo.animStyle = TextAnimation.AnimStyle.WavePosition;

        GetComponent<GameManager>().realTimeTextManager.EnqueueText(textInfo);
    }

    //-----------------------------------------------------------------------------
    //! [内容]		参加受信関数
    //-----------------------------------------------------------------------------
    [MunRPC]
    void RecvPlayerInfo(string playerName, int playerID, int characterID)
    {
        GameObject obj = MonobitView.Find(characterID).gameObject;
        obj.name = playerName;

    }

    //-----------------------------------------------------------------------------
    //! [内容]		参加受信関数
    //-----------------------------------------------------------------------------
    [MunRPC]
    void RecvGameStart()
    {
        GameObject obj = MonobitView.Find(myCharacterID).gameObject;

        obj.GetComponent<MovePlayer>().myCharactor = true;

        this.GetComponent<GameManager>().playing = true;
    }

    //-----------------------------------------------------------------------------
    //! [内容]		参加受信関数
    //-----------------------------------------------------------------------------
    [MunRPC]
    void RecvCountStart()
    {
        GameObject obj = MonobitView.Find(myCharacterID).gameObject;

        this.GetComponent<GameManager>().playing = true;
    }

    //-----------------------------------------------------------------------------
    //!	public変数
    //-----------------------------------------------------------------------------
    private void Awake()
    {
        // １秒間に30回のタイミングで、ストリーミング処理を実行します。
        MonobitEngine.MonobitNetwork.updateStreamRate = updateRate;
        MonobitEngine.MonobitNetwork.sendRate = updateRate;
    }

    //-----------------------------------------------------------------------------
    //! [内容]		更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {
        // プレイヤーキャラクタが未登場の場合に登場させる
        if(MonobitNetwork.isConnect && MonobitNetwork.inRoom)
        {
            // ホストを取得できるまでは通らない
            if (hostPlayer != null)
            {
                // ホストが変わったら解散する
                if (MonobitNetwork.host != hostPlayer)
                {
                    if (!leaveHost)
                    {
                        monobitView.RPC(
                           "RecvExitRoomText",
                           MonobitEngine.MonobitTargets.All,
                           "ホストが退出したのでルームが解散されます"
                           );

                        leaveHost = true;

                    }


                    time += Time.deltaTime;

                    if(time > 3)
                    {
                        LeaveRoom();
                    }
                }
                else if (MonobitNetwork.isHost)
                {
                    UpdatePlayerList();
                }
            }
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]		GUI制御
    //-----------------------------------------------------------------------------
    void OnGUI()
    {
        if (MonobitNetwork.isConnect)    // MUNサーバーへの接続確認
        {
            // ルームに入室している場合
            if (MonobitNetwork.inRoom)
            {
                // ルームからの退室
                if (GUILayout.Button("Leave Room", GUILayout.Width(150)))
                {
                    LeaveRoom();
                }
            }
            // ルームに入室していない場合
            else
            {
                // ルーム名の入力
                EnterText("RoomName : ", ref roomName);

                // ルームを作成して入室するad
                if (GUILayout.Button("Create Room", GUILayout.Width(150)))
                {
                    CreateRoom(roomName);
                }
              
                // ルームを検索して入室できる
                SearchAndEnterRoom();

                // ボタン入力でサーバから切断＆シーンリセット
                if (GUILayout.Button("Disconnect", GUILayout.Width(150)))
                {
                    DisconnectServer();
                }
            }
        }
        else    // 接続できていない時
        {
            string name = MonobitNetwork.player.name;
            MonobitNetwork.player.name = EnterText("playerName : ",ref name);

            // デフォルトロビーへの自動入室を許可する
            MonobitNetwork.autoJoinLobby = true;

            // MUNサーバに接続する
            if (GUILayout.Button("Connect Server", GUILayout.Width(150)))
            {
                MonobitNetwork.ConnectServer("SimpleNetwork3D_v1.0");
            }
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]		ロビーを作成する関数
    //-----------------------------------------------------------------------------
    private void CreateRoom(string roomName)
    {
        // ルーム設定を行う
        MonobitEngine.RoomSettings setting = new RoomSettings();
        setting.maxPlayers = maxPlayers;
        setting.isVisible = true;
        setting.isOpen = true;

        // 現在のロビー情報
        MonobitEngine.LobbyInfo info = MonobitNetwork.lobby;

        // ロビーの作成
        MonobitNetwork.CreateRoom(roomName, setting, info);
    }

    //-----------------------------------------------------------------------------
    //! [内容]		簡易テキスト入力
    //-----------------------------------------------------------------------------
    private string EnterText(string label, ref string text)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label);
        text = GUILayout.TextField(text, GUILayout.Width(200));
        GUILayout.EndHorizontal();

        return text;
    }

    //-----------------------------------------------------------------------------
    //! [内容]		ルームを検索して選択、入室
    //-----------------------------------------------------------------------------
    private void SearchAndEnterRoom()
    {
        // ルーム一覧を検索
        foreach (RoomData room in MonobitNetwork.GetRoomData())
        {
            // ルームパラメータの可視化
            System.String roomParam =
                System.String.Format(
                    "{0}({1}/{2})",
                    room.name,      // ルーム名
                    room.playerCount,       // 入室人数
                    ((room.maxPlayers == 0) ? "-" : room.maxPlayers.ToString())     // 最大人数
                );

            // ルームを選択して入室する
            if (GUILayout.Button("Enter Room : " + roomParam))
            {
                JoinRoom(room.name);
            }
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]		ルームに入室
    //-----------------------------------------------------------------------------
    private void JoinRoom(string roomName)
    {
        // シーンをリロードする
#if UNITY_5_3_OR_NEWER || UNITY_5_3
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
#else
                             Application.LoadLevel(Application.loadedLevelName);
#endif

        MonobitNetwork.JoinRoom(roomName);
    }

    //-----------------------------------------------------------------------------
    //! [内容]		サーバーから切断時
    //-----------------------------------------------------------------------------
    private void DisconnectServer()
    {
        // サーバから切断する
        MonobitNetwork.DisconnectServer();

        
    }

    private void LeaveRoom()
    {
        MonobitNetwork.LeaveRoom();
    }

    //-----------------------------------------------------------------------------
    //! [内容]		ロビーに参加したことがサーバーから送られてきたら
    //-----------------------------------------------------------------------------
    public void OnJoinedRoom()
    {
        hostPlayer = MonobitNetwork.host;

        monobitView.RPC(
                "RecvEnterRoomText",
                MonobitEngine.MonobitTargets.All,
                (string)(MonobitNetwork.playerName)
                );
    }

    //-----------------------------------------------------------------------------
    //! [内容]		ロビーから退出したことがサーバーから送られてきたら
    //-----------------------------------------------------------------------------
    public void OnLeftRoom()
    {
        // シーンをリロードする
#if UNITY_5_3_OR_NEWER || UNITY_5_3
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
#else
                     Application.LoadLevel(Application.loadedLevelName);
#endif
    }

    //-----------------------------------------------------------------------------
    //! [内容]		プレイヤーリストの変化を確認する
    //-----------------------------------------------------------------------------
    void UpdatePlayerList()
    {
        MonobitPlayer[] playerList = MonobitNetwork.playerList;
        // 前回のプレイヤーリストと違ったら
        if (beforePlayers != playerList)
        {
            // 退出したプレイヤーを探す
            foreach (MonobitPlayer player in beforePlayers)
            {
                if (Array.IndexOf(playerList, player) == -1)
                {
                    ExitRoomPlayer(player);
                }

            }

            // 入室したプレイヤーを探す
            foreach (MonobitPlayer player in playerList)
            {
                // 新たなプレイヤーを見つけたら
                if (Array.IndexOf(beforePlayers, player) == -1)
                {
                    EnterNewPlayer(player);
                }
            }
        }

        // 更新
        beforePlayers = MonobitNetwork.playerList;
    }

    //-----------------------------------------------------------------------------
    //! [内容]		新たなプレイヤーが参加したとき
    //-----------------------------------------------------------------------------
    void EnterNewPlayer(MonobitPlayer player)
    {
        // ルームプレイヤー情報を作成
        RoomPlayer roomPlayer = new RoomPlayer();
        roomPlayer.player = player;

        // キャラクターを生成
        GameObject playerObj = MonobitNetwork.Instantiate(
                            "SD_unitychan_humanoid",
                            Vector3.zero,
                            Quaternion.identity,
                            0
                            );

        // キャラクターのidセット
        roomPlayer.characterID = playerObj.GetComponent<MonobitView>().viewID;

        // リストに追加
        roomPlayers.Add(roomPlayer);

        //入室メッセージ送信
        monobitView.RPC(
                "RecvEnterRoomText",
                MonobitEngine.MonobitTargets.All,
                (string)(player.name),
                roomPlayer.player.ID,
                roomPlayer.characterID
        );


        // 現在いるプレイヤー情報を送信
        foreach(RoomPlayer temp in roomPlayers)
        {
            monobitView.RPC(
               "RecvPlayerInfo",
               player,
               (string)(temp.player.name),
               temp.player.ID,
               temp.characterID
                );
        }
       
    }

    //-----------------------------------------------------------------------------
    //! [内容]		誰かが退出したとき
    //-----------------------------------------------------------------------------
    void ExitRoomPlayer(MonobitPlayer player)
    {
        RoomPlayer temp = new RoomPlayer();

        foreach (RoomPlayer roomPlayer in roomPlayers)
        {
            if(roomPlayer.player == player)
            {
                GameObject obj = MonobitView.Find(roomPlayer.characterID).gameObject;

                MonobitNetwork.Destroy(obj);

                temp = roomPlayer;
            }
        }

        roomPlayers.Remove(temp);


        //退出メッセージ送信
        monobitView.RPC(
                "RecvExitRoomText",
                MonobitEngine.MonobitTargets.All,
                (string)(player.name)
        );
    }

    void SetCharacter()
    {
        // 現在いるプレイヤー情報を送信
        foreach (RoomPlayer temp in roomPlayers)
        {
            monobitView.RPC(
               "RecvPlayerInfo",
               MonobitTargets.All,
               (string)(temp.player.name),
               temp.player.ID,
               temp.characterID
                );
        }
    }
}
