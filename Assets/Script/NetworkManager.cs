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
    [SerializeField] private int updateRate = 30;

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
        // MUNサーバに接続しており、かつルームに入室している場合
        if (MonobitNetwork.isConnect && MonobitNetwork.inRoom)
        {
            // プレイヤーキャラクタが未登場の場合に登場させる
            if (playerObj == null)
            {
                Object instant = Resources.Load("SD_unitychan_humanoid");
                playerObj = MonobitNetwork.Instantiate(
                                "SD_unitychan_humanoid",
                                Vector3.zero,
                                Quaternion.identity,
                                0
                                );

                //camera.transform.parent = playerObj.transform;
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
                // ルーム内のプレイヤー一覧の表示
                GUILayout.BeginHorizontal();
                GUILayout.Label("PlayerList : ");
                foreach (MonobitPlayer player in MonobitNetwork.playerList)
                {
                    GUILayout.Label(player.name + " ");
                }
                GUILayout.EndHorizontal();

                // ルームからの退室
                if (GUILayout.Button("Leave Room", GUILayout.Width(150)))
                {
                    MonobitNetwork.LeaveRoom();

                    // シーンをリロードする
#if UNITY_5_3_OR_NEWER || UNITY_5_3
                    string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
#else
                     Application.LoadLevel(Application.loadedLevelName);
#endif
                }
            }
            // ルームに入室していない場合
            else
            {
                // ルーム名の入力
                GUILayout.BeginHorizontal();
                GUILayout.Label("RoomName : ");
                roomName = GUILayout.TextField(roomName, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                // ルームを作成して入室する
                if (GUILayout.Button("Create Room", GUILayout.Width(150)))
                {
                    if(MonobitNetwork.CreateRoom(roomName))
                    {

                    }
                }

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
                        if(MonobitNetwork.JoinRoom(room.name))
                        {
                            monobitView.RPC(
                                "RecvRoomText",
                                MonobitEngine.MonobitTargets.All,
                                (string)(MonobitNetwork.playerName + "が入室しました")
                                );
                        }
                    }
                }

                // ボタン入力でサーバから切断＆シーンリセット
                if (GUILayout.Button("Disconnect", GUILayout.Width(150)))
                {
                    // サーバから切断する
                    MonobitNetwork.DisconnectServer();

                    // シーンをリロードする
#if UNITY_5_3_OR_NEWER || UNITY_5_3
                    string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
#else
                     Application.LoadLevel(Application.loadedLevelName);
#endif
                }
            }
        }
        else    // 接続できていない時
        {
            // プレイヤーネームを入力するGUIの設定
            GUILayout.BeginHorizontal();
            GUILayout.Label("PlayerName : ");

            MonobitNetwork.playerName = GUILayout.TextField(
                (MonobitNetwork.playerName == null) ?
                    "" :
                    MonobitNetwork.playerName, GUILayout.Width(200)
                    );

            GUILayout.EndHorizontal();

            // デフォルトロビーへの自動入室を許可する
            MonobitNetwork.autoJoinLobby = true;

            // MUNサーバに接続する
            if (GUILayout.Button("Connect Server", GUILayout.Width(150)))
            {
                MonobitNetwork.ConnectServer("SimpleNetwork3D_v1.0");
            }
        }
    }
}
