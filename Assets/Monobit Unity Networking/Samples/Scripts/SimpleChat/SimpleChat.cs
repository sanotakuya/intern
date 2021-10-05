using UnityEngine;
using System.Collections;
using MonobitEngine;
using System.Collections.Generic;

public class SimpleChat : MonobitEngine.MonoBehaviour {

    /** ルーム名. */
    private string roomName = "";

    /** チャット発言文. */
    private string chatWord = "";

    /** チャット発言ログ. */
    List<string> chatLog = new List<string>();

    /**
     * RPC 受信関数.
     */
    [MunRPC]
    void RecvChat(string senderName, string senderWord)
    {
        chatLog.Add(senderName + " : " + senderWord);
        if( chatLog.Count > 10 )
        {
            chatLog.RemoveAt(0);
        }
    }
    
    /**
     * GUI制御.
     */
    void OnGUI()
    {
        // GUI用の解像度を調整する
        Vector2 guiScreenSize = new Vector2(800, 480);
        if (Screen.width > Screen.height)
        {
            // landscape
            GUIUtility.ScaleAroundPivot(new Vector2(Screen.width / guiScreenSize.x, Screen.height / guiScreenSize.y), Vector2.zero);
        }
        else
        {
            // portrait
            GUIUtility.ScaleAroundPivot(new Vector2(Screen.width / guiScreenSize.y, Screen.height / guiScreenSize.x), Vector2.zero);
        }

        // MUNサーバに接続している場合
        if ( MonobitNetwork.isConnect )
        {
            // ルームに入室している場合
            if ( MonobitNetwork.inRoom )
            {
                // ルーム内のプレイヤー一覧の表示
                GUILayout.BeginHorizontal();
                GUILayout.Label("PlayerList : ");
                foreach(MonobitPlayer player in MonobitNetwork.playerList)
                {
                    GUILayout.Label(player.name + " ");
                }
                GUILayout.EndHorizontal();

                // ルームからの退室
                if (GUILayout.Button("Leave Room", GUILayout.Width(150)))
                {
                    MonobitNetwork.LeaveRoom();
                    chatLog.Clear();
                }

                // チャット発言文の入力
                GUILayout.BeginHorizontal();
                GUILayout.Label("Message : ");
                chatWord = GUILayout.TextField(chatWord, GUILayout.Width(400));
                GUILayout.EndHorizontal();

                // チャット発言文を送信する
                if (GUILayout.Button("Send", GUILayout.Width(100)))
                {
                    monobitView.RPC("RecvChat", MonobitTargets.All, MonobitNetwork.playerName, chatWord);
                    chatWord = "";
                }

                // チャットログを表示する
                string msg = "";
                for(int i = 0; i < 10; ++i )
                {
                    msg += ((i < chatLog.Count) ? chatLog[i] : "") + "\r\n";
                }
                GUILayout.TextArea(msg);
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
                    MonobitNetwork.CreateRoom(roomName);
                    chatLog.Clear();
                }

                // ルーム一覧を検索
                foreach ( RoomData room in MonobitNetwork.GetRoomData())
                {
                    // ルームを選択して入室する
                    if (GUILayout.Button("Enter Room : " + room.name + "(" + room.playerCount + "/" + ((room.maxPlayers == 0) ? "-" : room.maxPlayers.ToString()) + ")"))
                    {
                        MonobitNetwork.JoinRoom(room.name);
                    }
                }
            }
        }
        // MUNサーバに接続していない場合
        else
        {
            // プレイヤー名の入力
            GUILayout.BeginHorizontal();
            GUILayout.Label("PlayerName : ");
            MonobitNetwork.playerName = GUILayout.TextField((MonobitNetwork.playerName == null) ? "": MonobitNetwork.playerName, GUILayout.Width(200));
            GUILayout.EndHorizontal();
            
            // デフォルトロビーへの自動入室を許可する
            MonobitNetwork.autoJoinLobby = true;

            // MUNサーバに接続する
            if( GUILayout.Button("Connect Server", GUILayout.Width(150)))
            {
                MonobitNetwork.ConnectServer("SimpleChat_v1.0");
            }
        }
    }
}
