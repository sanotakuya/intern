using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;


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
    private GameObject playerObj = null;
    private MonobitPlayer hostPlayer;

    private bool leaveHost = false;
    private float time = 0;

    [SerializeField] private int updateRate = 30;
    [SerializeField] private uint maxPlayers = 4;
    [SerializeField] private string lobbySceneName = "Lobby";
    [SerializeField] private string RoomSceneName = "Room";
    //-----------------------------------------------------------------------------
    //!	public変数
    //-----------------------------------------------------------------------------
    private void Awake()
    {
        // １秒間に30回のタイミングで、ストリーミング処理を実行します。
        MonobitEngine.MonobitNetwork.updateStreamRate = updateRate;
    }

    //-----------------------------------------------------------------------------
    //! [内容]		更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {
        // プレイヤーキャラクタが未登場の場合に登場させる
        if(MonobitNetwork.isConnect && MonobitNetwork.inRoom)
        {
            if (playerObj == null)
            {
                Object instant = Resources.Load("SD_unitychan_humanoid");
                playerObj = MonobitNetwork.Instantiate(
                                "SD_unitychan_humanoid",
                                Vector3.zero,
                                Quaternion.identity,
                                0
                                );

                playerObj.GetComponent<MovePlayer>().myCharactor = true;
                playerObj.GetComponent<MonobitView>().TransferOwnership(MonobitEngine.MonobitNetwork.host);
            }

            // ホストが変わったら解散する
            if(hostPlayer != null)
            {
                if (MonobitNetwork.host != hostPlayer)
                {
                    if (!leaveHost)
                    {
                        monobitView.RPC(
                           "RecvRoomText",
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
                "RecvRoomText",
                MonobitEngine.MonobitTargets.All,
                (string)(MonobitNetwork.playerName + "が入室しました")
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
}
