using UnityEngine;
using System.Collections;
using MonobitEngine;

namespace MonobitEngine.Sample
{
    public class SearchPlayer : MonobitEngine.MonoBehaviour
    {
        // 自分の名前
        private string myName = "";

        // マッチングルームの最大人数
        private byte maxPlayers = 10;

        // マッチングルームを公開するかどうかのフラグ
        private bool isVisible = true;

        // 検索相手の名前
        private string SearchPlayerName = "";

        // 検索相手の結果が得られたか？
        private bool isSearchPlayer = false;

        // ゲームスタートフラグ
        private bool isGameStart = false;

        // 制限時間
        private int battleEndFrame = 60 * 60;

        // 自身のオブジェクトを生成したかどうかのフラグ
        private bool isSpawnMyChara = false;

        /**
         * GUI周りの制御.
         */
        public void OnGUI()
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

            // まだ未接続の場合
            if (!MonobitNetwork.isConnect)
            {
                OnGUI_Disconnect();
            }
            // まだルーム未入室の場合
            else if (!MonobitNetwork.inRoom)
            {
                OnGUI_OutOfRoom();
            }
            // ルーム入室中の場合
            else
            {
                OnGUI_InRoom();
            }
        }

        /**
         * 未接続中のOnGUI制御.
         */
        public void OnGUI_Disconnect()
        {
            // 自分の名前を入力する
            GUILayout.BeginHorizontal();
            GUILayout.Label("Your Name : ");
            this.myName = GUILayout.TextField(this.myName, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            // サーバへの接続
            if (GUILayout.Button("Connect Server", GUILayout.Width(200)))
            {
                // 空欄はNG
                if (string.IsNullOrEmpty(this.myName))
                {
                    Debug.LogWarning("Your name is null.");
                }
                else
                {
                    // プレイヤー名を設定
                    MonobitNetwork.player.name = this.myName;

                    // デフォルトロビーへの強制入室をONにする
                    MonobitNetwork.autoJoinLobby = true;

                    // サーバに接続する
                    MonobitNetwork.ConnectServer("SearchPlayer_v1.0");
                }
            }
        }

        /**
         * 接続中＆ルーム未入室状態でのOnGUI制御.
         */
        private void OnGUI_OutOfRoom()
        {
            // 自分のプレイヤー名とIDを表示
            GUILayout.Label("Your Name : " + MonobitNetwork.playerName);

            // ルーム作成設定
            OnGUI_CreateRoom();

            // プレイヤー検索設定
            OnGUI_SearchPlayer();

            // プレイヤー検索結果の表示
            OnGUI_SearchPlayerList();
        }

        /**
         * ルーム作成設定.
         */
        private void OnGUI_CreateRoom()
        {
            // 表題
            GUILayout.Label("Create Room", new GUIStyle() { normal = new GUIStyleState() { textColor = Color.white }, fontStyle = FontStyle.Bold });
            GUILayout.BeginHorizontal();
            GUILayout.Space(25);
            GUILayout.BeginVertical();

            // 自分の作成するルーム名を公開設定にするかどうかのフラグ
            this.isVisible = GUILayout.Toggle(this.isVisible, "Visible room");

            // 自分のルームに入室可能な収容人数の設定
            GUILayout.BeginHorizontal(new GUIStyle() { alignment = TextAnchor.MiddleLeft });
            GUILayout.Label("Max Players : ", GUILayout.Width(100));
            string tmpInput = GUILayout.TextField(this.maxPlayers.ToString(), GUILayout.Width(50));
            byte.TryParse(tmpInput, out this.maxPlayers);
            GUILayout.EndHorizontal();

            // 自身の名前をもとにして、ルームを作成する
            if (GUILayout.Button("Create Room", GUILayout.Width(150)))
            {
                MonobitNetwork.JoinOrCreateRoom(this.myName, new RoomSettings() { isVisible = this.isVisible, isOpen = true, maxPlayers = this.maxPlayers }, null);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        /**
         * プレイヤー検索設定.
         */
        private void OnGUI_SearchPlayer()
        {
            // 表題
            GUILayout.Label("Search Players", new GUIStyle() { normal = new GUIStyleState() { textColor = Color.white }, fontStyle = FontStyle.Bold });
            GUILayout.BeginHorizontal();
            GUILayout.Space(25);
            GUILayout.BeginVertical();

            // 検索するプレイヤー名を入力
            GUILayout.BeginHorizontal();
            GUILayout.Label("Player Name : ");
            this.SearchPlayerName = GUILayout.TextField(this.SearchPlayerName, GUILayout.Width(200));
            // プレイヤーを検索する
            if (GUILayout.Button("Search", GUILayout.Width(150)))
            {
                // 空欄はNG
                if (string.IsNullOrEmpty(this.myName))
                {
                    Debug.LogWarning("Player name is null.");
                }
                else
                {
                    // プレイヤー検索は時間がかかるので、コルーチンで実行する
                    StartCoroutine("SearchPlayerList");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        /**
         * プレイヤー検索結果の表示.
         */
        private void OnGUI_SearchPlayerList()
        {
            if (!isSearchPlayer)
            {
                return;
            }

            // 表題
            GUILayout.Label("Search Player List", new GUIStyle() { normal = new GUIStyleState() { textColor = Color.white }, fontStyle = FontStyle.Bold });
            GUILayout.BeginHorizontal();
            GUILayout.Space(50);
            GUILayout.BeginVertical();

            // 検索できたプレイヤーの分だけ表示する
            foreach (SearchPlayerData player in MonobitNetwork.SearchPlayerList)
            {
                if (!player.connect)
                {
                    // オフライン（ユーザーが存在しない）
                    GUILayout.Label(player.playerName + " is offline.");
                }
                else if (!player.inRoom)
                {
                    // ルーム未入室
                    GUILayout.Label(player.playerName + " is out of rooms.");
                }
                else
                {
                    // ルーム入室中
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(player.playerName + " is in " + player.roomName, GUILayout.Width(250));
                    // 選択したプレイヤーの部屋に入室
                    if (GUILayout.Button("Join", GUILayout.Width(50)))
                    {
                        MonobitNetwork.JoinRoom(player.roomName);
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        /**
         * フレンド検索.
         */
        private IEnumerator SearchPlayerList()
        {
            // フラグをクリア
            isSearchPlayer = false;

            // プレイヤー検索リストの作成
            string[] searchList = this.SearchPlayerName.Split(' ');

            // フレンドの検索
            MonobitNetwork.SearchPlayers(searchList);

            // 見つかるまで待つ
            while (!isSearchPlayer)
            {
                yield return null;
            }
        }

        /**
         * ルーム内でのGUI操作.
         */
        private void OnGUI_InRoom()
        {
            // 自分の名前とプレイヤーIDの表示
            GUILayout.Label("Your name : " + MonobitNetwork.playerName);
            GUILayout.Label("Your ID : " + MonobitNetwork.player.ID);

            // ルーム内に存在するプレイヤー数の表示
            GUILayout.Label("PlayerCount : " + MonobitNetwork.room.playerCount + " / " + ((MonobitNetwork.room.maxPlayers == 0) ? "-" : MonobitNetwork.room.maxPlayers.ToString()));

            // ルームの入室制限可否設定の表示
            GUILayout.Label("Room is : " + ((MonobitNetwork.room.open) ? "open," : "close,") + " and " + ((MonobitNetwork.room.visible) ? "visible." : "invisible."));

            // 制限時間の表示
            if ( isGameStart )
            {
                GUILayout.Label("Rest Frame : " + this.battleEndFrame);
            }

            // 部屋からの離脱
            if( GUILayout.Button("Leave Room", GUILayout.Width(100)))
            {
                MonobitNetwork.LeaveRoom();
            }

            // ホストの場合
            if( MonobitNetwork.isHost )
            {
                // ゲームスタート
                if( !isGameStart && GUILayout.Button("Start Game", GUILayout.Width(100)))
                {
                    this.isGameStart = true;
                    // room.open = false;
                    monobitView.RPC("GameStart", MonobitTargets.All, null);
                }

                // バトル終了
                if( this.battleEndFrame <= 0 )
                {
                    // room.open = true;

                    // 部屋から離脱する
                    monobitView.RPC("LeaveRoomAll", MonobitTargets.All, null);
                }
            }
        }

        /**
         * 更新処理.
         */
        public void Update()
        {
            // ルーム入室中でなければ処理しない
            if( !MonobitNetwork.isConnect || !MonobitNetwork.inRoom )
            {
                return;
            }

            // ゲームスタート後、自分のキャラクタのSpawnが終わってなければ、自身をSpawnする
            if( this.isGameStart && !this.isSpawnMyChara )
            {
                GameStart();
            }

            // ゲームスタート後、ホストなら、制限時間管理をする
            if( this.isGameStart && MonobitNetwork.isHost )
            {
                // 制限時間の減少
                if( this.battleEndFrame > 0 )
                {
                    this.battleEndFrame--;
                }

                // 制限時間をRPCで送信
                monobitView.RPC("TickCount", MonobitTargets.Others, this.battleEndFrame);
            }
        }

        /**
         * ゲームスタート.
         */
        [MunRPC]
        private void GameStart()
        {
            // ゲームスタートフラグを立てる
            this.isGameStart = true;

            // ある程度ランダムな場所にプレイヤーを配置する
            Vector3 position = Vector3.zero;
            position.x = Random.Range(-10.0f, 10.0f);
            position.z = Random.Range(-10.0f, 10.0f);
            Quaternion rotation = Quaternion.AngleAxis(Random.Range(-180.0f, 180.0f), Vector3.up);

            // プレイヤーの配置（他クライアントにも同時にInstantiateする）
            MonobitNetwork.Instantiate("SD_unitychan_generic_PC", position, rotation, 0);

            // 出現させたことを確認
            this.isSpawnMyChara = true;
        }

        /**
         * 全員のルーム離脱.
         */
        [MunRPC]
        private void LeaveRoomAll()
        {
            MonobitNetwork.LeaveRoom();
        }

        /**
         * 制限時間の受信.
         */
        [MunRPC]
        private void TickCount(int frame)
        {
            // ゲームスタートフラグを立てる（途中参加者のための処置）
            this.isGameStart = true;

            // 制限時間を同期する
            this.battleEndFrame = frame;
        }

        /**
         * サーバ接続成功時の処理.
         */
        public void OnConnectedToServer()
        {
            Debug.Log("OnConnectedToServer");
        }

        /**
         * サーバ接続失敗時の処理.
         */
        public void OnConnectToServerFailed(DisconnectCause cause)
        {
            Debug.Log("OnConnectToServerFailed : cause = " + cause.ToString());
        }

        /**
         * ロビー入室成功時の処理.
         */
        public void OnJoinedLobby()
        {
            Debug.Log("OnJoinedLobby");
        }

        /**
         * 接続が切断されたときの処理.
         */
        public void OnDisconnectedFromServer()
        {
            Debug.Log("OnDisconnectedFromServer");
        }

        /**
         * ルーム作成時の処理.
         */
        public void OnCreatedRoom()
        {
            Debug.Log("OnCreatedRoom");
        }

        /**
         * ルーム作成失敗時の処理.
         */
        public void OnCreateRoomFailed(object[] parameters)
        {
            Debug.Log("OnCreateRoomFailed : ErrorCode = " + parameters[0] + ", DebugMsg = " + parameters[1]);
        }

        /**
         * ルーム入室時の処理.
         */
        public void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom");
        }

        /**
         * ランダム入室失敗時の処理.
         */
        public void OnMonobitRandomJoinFailed(object[] parameters)
        {
            Debug.Log("OnMonobitRandomJoinFailed : ErrorCode = " + parameters[0] + ", DebugMsg = " + parameters[1]);
        }

        /**
         * 指定ルーム入室失敗時の処理.
         */
        public void OnJoinRoomFailed(object[] parameters)
        {
            Debug.Log("OnJoinRoomFailed : ErrorCode = " + parameters[0] + ", DebugMsg = " + parameters[1]);
        }

        /**
         * プレイヤー検索結果が返ってきた時の処理.
         */
        public void OnUpdatedSearchPlayers()
        {
            Debug.Log("OnUpdatedSearchPlayers");
            isSearchPlayer = true;
        }

        /**
         * ルーム退室時の処理.
         */
        public void OnLeftRoom()
        {
            this.isGameStart = false;
            this.isSpawnMyChara = false;
            this.battleEndFrame = 60 * 60;
        }
    }
}
