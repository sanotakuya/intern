using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonobitEngine.Sample.ServerSide
{
    /**
     * @brief   アイテム収集ゲームサンプル（サーバサイド版）.
     */
    public class RakeupGame : MonobitEngine.MonoBehaviour
    {
        /**
         * @brief ゲーム内プレイヤーオブジェクト情報.
         */
        public static List<SD_Unitychan_PC> s_PlayerObject = new List<SD_Unitychan_PC>();

        /**
         * @brief ルーム名.
         */
        private string roomName = "";

        /**
         * @brief 制限時間.
         */
        private Int64 gameTimeLimit = 0;

        /**
         * @brief ゲーム開始フラグ.
         */
        private bool isGameStart = false;

        /**
         * @brief ゲーム終了フラグ.
         */
        private bool isGameEnd = false;

        /**
         * @brief 意図しないタイミングで Disconnect されたかどうかのフラグ.
         */
        private bool isUnsafeDisconnect = false;

        /**
         * @brief 現在途中切断処理中かどうかのフラグ.
         */
        private bool isDisconnecting = false;

        /**
         * @brief 現在途中切断復帰中かどうかのフラグ.
         */
        private bool isReconnecting = false;

        /**
         * @brief 再接続する場合のルーム名.
         */
        private string reconnectRoomName = "";

        /**
         * @brief 再接続する場合の自身のオブジェクト.
         */
        private GameObject myObject = null;

        /**
         * @brief 再接続する場合の自身のオブジェクトの位置.
         */
        private Vector3 myPosition = new Vector3();

        /**
         * @brief 再接続する場合の自身のオブジェクトの姿勢.
         */
        private Quaternion myRotation = new Quaternion();

        /**
         * @brief アイテム情報.
         */
        public static List<GameObject> itemObject = new List<GameObject>();

        /**
         * @brief   UI表示周り.
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

            // 縁取りを行なうように、位置をずらしつつ表示
            GUILayout.BeginArea(new Rect(-1, -1, guiScreenSize.x, guiScreenSize.y));
            {
                OnGUI_InWindow(false, new GUIStyleState() { textColor = Color.black });
            }
            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect(-1, 1, guiScreenSize.x, guiScreenSize.y));
            {
                OnGUI_InWindow(false, new GUIStyleState() { textColor = Color.black });
            }
            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect(1, -1, guiScreenSize.x, guiScreenSize.y));
            {
                OnGUI_InWindow(false, new GUIStyleState() { textColor = Color.black });
            }
            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect(1, 1, guiScreenSize.x, guiScreenSize.y));
            {
                OnGUI_InWindow(false, new GUIStyleState() { textColor = Color.black });
            }
            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect(0, 0, guiScreenSize.x, guiScreenSize.y));
            {
                OnGUI_InWindow(true, new GUIStyleState() { textColor = Color.white });
            }
            GUILayout.EndArea();
        }

        /**
         * @brief   更新関数.
         */
        void Update()
        {
            // ゲームスタート時の処理
            if (isGameStart)
            {
                // 自身のキャラクタ位置の退避
                if (myObject != null)
                {
                    myPosition = myObject.transform.position;
                    myRotation = myObject.transform.rotation;
                }

                // ルーム名の退避
                if (MonobitNetwork.room != null)
                {
                    reconnectRoomName = MonobitNetwork.room.name;
                }
            }
        }

        /**
         * @brief   GUI表示回りの内部処理.
         * @param   isDispInputGUI  入力系GUIを表示するかどうかのフラグ（縁取り文字描画機能を活かした場合の、多重描画を防止するため）.
         * @param   state           文字表示色.
         */
        private void OnGUI_InWindow(bool isDispInputGUI, GUIStyleState state)
        {
            // 接続済みかどうか
            if (MonobitNetwork.isConnect)
            {
                // 途中切断による再接続処理フラグを設定
                isUnsafeDisconnect = true;

                // 接続情報の表示
                OnGUI_Stats(isDispInputGUI, state);

                // ルーム入室済みかどうか
                if (MonobitNetwork.inRoom)
                {
                    if (isGameStart && !isGameEnd)
                    {
                        OnGUI_InGame(isDispInputGUI, state);
                    }
                    else if (isGameEnd)
                    {
                        OnGUI_Result(isDispInputGUI, state);
                    }
                    OnGUI_InRoom(isDispInputGUI, state);
                }
                else
                {
                    OnGUI_InLobby(isDispInputGUI, state);
                }
            }
            else
            {
                OnGUI_Offline(isDispInputGUI, state);
                // 意図しないタイミングで切断されたとき
                if (isUnsafeDisconnect)
                {
                    GUILayout.Window(0, new Rect(Screen.width / 2 - 100, Screen.height / 2 - 40, 200, 80), OnGUI_UnsafeDisconnect, "Disconnect");
                }
            }
        }

        // ウィンドウ表示
        void OnGUI_UnsafeDisconnect(int WindowID)
        {
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.MiddleCenter;
            GUIStyleState stylestate = new GUIStyleState();
            stylestate.textColor = Color.white;
            style.normal = stylestate;
            if (isDisconnecting)
            {
                GUILayout.Label("MUN is disconnect.\nAre you reconnect?", style);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Yes", GUILayout.Width(50)))
                {
                    // もう一度接続処理を実行する
                    isDisconnecting = false;
                    isReconnecting = true;
                    isGameStart = false;
                    isGameEnd = false;
                    MonobitNetwork.ConnectServer("RakeupGame_ServerSide_v1.0");
                }
                if (GUILayout.Button("No", GUILayout.Width(50)))
                {
                    isDisconnecting = false;

                    // シーンをオフラインシーンへ
                    MonobitNetwork.LoadLevel(OfflineSceneReconnect.SceneNameOffline);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("reconnecting...", style);
            }
        }

        /**
         * @brief   オフライン中のGUI表示.
         * @param   isDispInputGUI  入力系GUIを表示するかどうかのフラグ（縁取り文字描画機能を活かした場合の、多重描画を防止するため）.
         * @param   state           文字表示色.
         */
        private void OnGUI_Offline(bool isDispInputGUI, GUIStyleState state)
        {
            // プレイヤー名入力GUIレイヤー
            {
                // Header
                GUILayout.Label("Player Name Entry", new GUIStyle() { normal = state, fontSize = 24 });
                GUILayout.BeginHorizontal();
                GUILayout.Space(25);
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                {
                    // プレイヤー名の入力欄の表示
                    GUILayout.Label("Input player name:", new GUIStyle() { normal = state, fontSize = 16, margin = new RectOffset(6, 6, 6, 6), fixedWidth = 140 });
                    if (isDispInputGUI)
                    {
                        MonobitNetwork.playerName = GUILayout.TextField((MonobitNetwork.playerName == null) ? "" : MonobitNetwork.playerName, GUILayout.Width(150));
                    }

                    // サーバ接続ボタン入力表示
                    if (isDispInputGUI)
                    {
                        if (GUILayout.Button("Connect Server", GUILayout.Width(150)))
                        {
                            // 名前欄が空欄であれば、GUIDで生成したプレイヤー名でサーバに接続
                            if (MonobitNetwork.playerName == null || MonobitNetwork.playerName.Length <= 0)
                            {
                                MonobitNetwork.playerName = Guid.NewGuid().ToString();
                            }

                            // デフォルトロビーへの強制入室をONにする。
                            MonobitNetwork.autoJoinLobby = true;

                            // デバッグログ出力を無効にする
                            MonobitNetwork.LogLevel = MonobitLogLevel.Informational;

                            // まだ未接続の場合、MonobitNetworkに接続する。
                            if (! MonobitNetwork.isConnect)
                            {
                                isDisconnecting = false;
                                isReconnecting = false;
                                isGameStart = false;
                                isGameEnd = false;
                                MonobitNetwork.ConnectServer("RakeupGame_ServerSide_v1.0");
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();

                // Footer
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        /**
         * @brief   オンライン＆ロビー入室中のGUI表示.
         * @param   isDispInputGUI  入力系GUIを表示するかどうかのフラグ（縁取り文字描画機能を活かした場合の、多重描画を防止するため）.
         * @param   state           文字表示色.
         */
        private void OnGUI_InLobby(bool isDispInputGUI, GUIStyleState state)
        {
            // ルーム作成GUIレイヤー
            {
                // Header
                GUILayout.Label("Create Room/Join Random", new GUIStyle() { normal = state, fontSize = 18 });
                GUILayout.BeginHorizontal();
                GUILayout.Space(25);
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                {
                    // ルーム名の入力欄、および作成ボタン入力表示
                    GUILayout.Label("Input room name:", new GUIStyle() { normal = state, fontSize = 16, margin = new RectOffset(6, 6, 6, 6), fixedWidth = 140 });
                    if (isDispInputGUI)
                    {
                        this.roomName = GUILayout.TextField(this.roomName, GUILayout.Width(200));
                        if (GUILayout.Button("Create Room", GUILayout.Width(150)))
                        {
                            // 入力があった場合、指定された名前で４人部屋を生成
                            MonobitNetwork.CreateRoom(roomName, new RoomSettings() { isVisible = true, isOpen = true, maxPlayers = 4 }, null);
                        }
                    }
                    else
                    {
                        GUILayout.Space(25);
                    }
                }
                GUILayout.EndHorizontal();

                // ルームの作成ボタン入力表示
                if (isDispInputGUI)
                {
                    if (GUILayout.Button("Join Random", GUILayout.Width(150)))
                    {
                        // ランダム入室をする
                        MonobitNetwork.JoinRandomRoom();
                    }
                }
                else
                {
                    GUILayout.Space(25);
                }

                // Footer
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            // ルームリストGUIレイヤー
            {
                // ルーム一覧の取得
                RoomData[] roomList = MonobitNetwork.GetRoomData();

                // ルーム一覧が存在する場合、ルームリストを表示
                if (roomList != null)
                {
                    // Header
                    GUILayout.Label("Created Room List", new GUIStyle() { normal = state, fontSize = 24 });
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(25);
                    GUILayout.BeginVertical();

                    // ルームリストについて、１件ずつボタン入力表示
                    foreach (RoomData room in roomList)
                    {
                        if (isDispInputGUI && room.open && room.visible)
                        {
                            if (GUILayout.Button(room.name + " (" + room.playerCount + "/" + room.maxPlayers + ")", GUILayout.Width(300)))
                            {
                                MonobitNetwork.JoinRoom(room.name);
                            }
                        }
                    }

                    // Footer
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
            }
        }

        /**
         * @brief   オンライン＆ルーム入室中のGUI表示.
         * @param   isDispInputGUI  入力系GUIを表示するかどうかのフラグ（縁取り文字描画機能を活かした場合の、多重描画を防止するため）.
         * @param   state           文字表示色.
         */
        private void OnGUI_InRoom(bool isDispInputGUI, GUIStyleState state)
        {
            // ルーム退室GUIレイヤー
            {
                GUILayout.Space(25);

                // ゲーム開始までの時間表示
                if(!isGameStart && gameTimeLimit > 0)
                {
                    GUILayout.Label("Rest Time To Game Start : " + gameTimeLimit, new GUIStyle() { normal = state, fontSize = 16, margin = new RectOffset(6, 6, 6, 6), fixedWidth = 140 });
                }

                // ルーム退室ボタン入力表示
                if (isDispInputGUI)
                {
                    if (GUILayout.Button("Leave Room", GUILayout.Width(150)))
                    {
                        // 安全なDisconnectが実行されていることを成立させる
                        isUnsafeDisconnect = false;
                        isDisconnecting = false;
                        isReconnecting = false;
                        isGameStart = false;
                        isGameEnd = false;

                        // 入力があった場合、ルームから退室する
                        MonobitNetwork.LeaveRoom();
                    }
                }
                else
                {
                    GUILayout.Space(25);
                }
            }
        }

        /**
         * @brief   ゲームプレイ中の表示.
         * @param   isDispInputGUI  入力系GUIを表示するかどうかのフラグ（縁取り文字描画機能を活かした場合の、多重描画を防止するため）.
         * @param   state           文字表示色.
         */
        private void OnGUI_InGame(bool isDispInputGUI, GUIStyleState state)
        {
            // Header
            GUILayout.Label("Game Info", new GUIStyle() { normal = state, fontSize = 24 });
            GUILayout.BeginHorizontal();
            GUILayout.Space(25);
            GUILayout.BeginVertical();

            // 制限時間の表示
            if (isGameStart && gameTimeLimit > 0)
            {
                GUILayout.Label("Rest Frame : " + gameTimeLimit, new GUIStyle() { normal = state, fontSize = 16, margin = new RectOffset(6, 6, 6, 6), fixedWidth = 140 });
            }

            // Footer
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        /**
         * @brief   ゲームプレイ結果の表示.
         * @param   isDispInputGUI  入力系GUIを表示するかどうかのフラグ（縁取り文字描画機能を活かした場合の、多重描画を防止するため）.
         * @param   state           文字表示色.
         */
        private void OnGUI_Result(bool isDispInputGUI, GUIStyleState state)
        {
            // Header
            GUILayout.Label("Game Result", new GUIStyle() { normal = state, fontSize = 24 });
            GUILayout.BeginHorizontal();
            GUILayout.Space(25);
            GUILayout.BeginVertical();

            // リザルトの表示
            foreach (SD_Unitychan_PC playerObject in s_PlayerObject)
            {
                GUILayout.Label("Player ID : " + playerObject.GetPlayerID() + " Name : " + playerObject.GetPlayerName() + " Score : " + playerObject.MyScore,
                                new GUIStyle() { normal = state, fontSize = 16, margin = new RectOffset(6, 6, 6, 6), fixedWidth = 140 });
            }

            // Footer
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        /**
         * @brief   接続情報の表示.
         * @param   isDispInputGUI  入力系GUIを表示するかどうかのフラグ（縁取り文字描画機能を活かした場合の、多重描画を防止するため）.
         * @param   state           文字表示色.
         */
        private void OnGUI_Stats(bool isDispInputGUI, GUIStyleState state)
        {
            // Header
            GUILayout.Label("Stats Info", new GUIStyle() { normal = state, fontSize = 24 });
            GUILayout.BeginHorizontal();
            GUILayout.Space(25);
            GUILayout.BeginVertical();

            // オンライン上の自身のプレイヤー名の表示
            if (MonobitNetwork.playerName != null)
            {
                GUILayout.Label("My Player Name : " + MonobitNetwork.playerName, new GUIStyle() { normal = state, fontSize = 16, margin = new RectOffset(6, 6, 6, 6), fixedWidth = 140 });
            }

            // 現在入室中のルーム情報を表示
            if (MonobitNetwork.inRoom)
            {
                GUILayout.Label("Entry Room : " + MonobitNetwork.room.name + " (" + MonobitNetwork.room.playerCount + "/" + MonobitNetwork.room.maxPlayers + ")", new GUIStyle() { normal = state, fontSize = 16, margin = new RectOffset(6, 6, 6, 6), fixedWidth = 140 });
                GUILayout.Label("Host Player : " + MonobitNetwork.host.name, new GUIStyle() { normal = state, fontSize = 16, margin = new RectOffset(6, 6, 6, 6), fixedWidth = 140 });
                GUILayout.Label("Room Status : " + ((MonobitNetwork.room.visible) ? "Visible, " : "Invisible, ") + ((MonobitNetwork.room.open) ? "Open" : "Closed"), new GUIStyle() { normal = state, fontSize = 16, margin = new RectOffset(6, 6, 6, 6), fixedWidth = 140 });
            }

            // Footer
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        /**
         * @brief   接続が切断されたときの処理.
         */
        public void OnDisconnectedFromServer()
        {
            Debug.Log("Disconnected from Monobit");
            isDisconnecting = true;
        }

        /**
         * @brief   接続失敗時の処理.
         * @param   parameters  接続失敗時の詳細情報.
         */
        public void OnConnectToServerFailed(object parameters)
        {
            Debug.Log("OnConnectToServerFailed : StatusCode = " + parameters + ", ServerAddress = " + MonobitNetwork.ServerAddress);
            isDisconnecting = true;
        }

        /**
         * @brief   接続が確立した時の処理.
         */
        public void OnJoinedLobby()
        {
            // 現在残っているルーム情報から再接続を実行する
            if( !string.IsNullOrEmpty(reconnectRoomName) )
            {
                MonobitNetwork.JoinRoom(reconnectRoomName);
            }
        }

        /**
         * @brief   指定ルーム入室失敗時の処理.
         * @param   parameters  入室失敗時の詳細情報.
         */
        public void OnJoinRoomFailed(object[] parameters)
        {
            Debug.Log("OnJoinRoomFailed : ErrorCode = " + parameters[0] + ", DebugMsg = " + parameters[1]);
        }

        /**
         * @brief   ゲームスタートを受信(RPC).
         */
        [MunRPC]
        void OnPrepareToGameStart(Int64 time)
        {
            gameTimeLimit = time;
        }

        /**
         * @brief   ゲームスタートを受信(RPC).
         */
        [MunRPC]
        void OnGameStart()
        {
            // オブジェクト情報のクリア
            s_PlayerObject = new List<SD_Unitychan_PC>();
            itemObject = new List<GameObject>();

            // ルームをクローズする
            MonobitNetwork.room.open = false;

            // ゲームスタートフラグを立てる
            isGameStart = true;

            if (!isReconnecting)
            {
                // ある程度ランダムな位置・姿勢でプレイヤーを配置する
                myPosition = Vector3.zero;
                myPosition.x = UnityEngine.Random.Range(-10.0f, 10.0f);
                myPosition.z = UnityEngine.Random.Range(-10.0f, 10.0f);
                myRotation = Quaternion.AngleAxis(UnityEngine.Random.Range(-180.0f, 180.0f), Vector3.up);
            }

            // プレイヤーの配置（他クライアントにも同時にInstantiateする）
            myObject = MonobitNetwork.Instantiate("SD_unitychan_PC", myPosition, myRotation, 0);

            // 再接続処理完了
            isReconnecting = false;
        }

        /**
         * @brief   制限時間を受信(RPC).
         * @param   timeLimit   受信した制限時間.
         */
        [MunRPC]
        void OnTickCount(Int64 timeLimit)
        {
            // 制限時間を同期する
            gameTimeLimit = timeLimit;
        }

        /**
         * @brief   スコアの更新(RPC).
         * @param   playerId    該当するプレイヤーID.
         * @param   score       更新するスコアの値.
         */
        [MunRPC]
        void OnUpdateScore(Int32 playerId, UInt64 score)
        {
            foreach (SD_Unitychan_PC playerObject in s_PlayerObject)
            {
                if(playerObject.GetPlayerID() == playerId )
                {
                    playerObject.MyScore = score;
                }
            }
        }

        /**
         * @brief   ゲームスタートを受信(RPC).
         */
        [MunRPC]
        void OnGameEnd()
        {
            isGameEnd = true;
        }
    }
}
