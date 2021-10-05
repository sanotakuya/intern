using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MonobitEngine;

namespace MonobitEngine.Sample
{
    public class NumericMatching : MonobitEngine.MonoBehaviour
    {
        // カスタムパラメータリスト
        Hashtable customRoomParam = new Hashtable();

        // マッチングルームの最大人数
        private byte maxPlayers = 10;

        // ゲームスタートフラグ
        private bool isGameStart = false;

        // 制限時間
        private int battleEndFrame = 60 * 60;

        // 自身のオブジェクトを生成したかどうかのフラグ
        private bool isSpawnMyChara = false;

        // クエリー検索で範囲検索をする際の数値範囲の閾値
        private int baseValue = 0;            // 基準値
        private int baseValueMax = 0;         // 最小値
        private int baseValueMin = 0;         // 最大値
        private int baseValueIncrease = 5;    // 閾値の±増分
        private int baseValueHigh = 100;      // 範囲検索の最大上限
        private int baseValueLow = 0;         // 範囲検索の最小下限

        private bool isEnterAnotherRoom = false;

        /**
		 * 開始関数.
		 */
        public void Awake()
        {
            // デフォルトロビーへの強制入室をOFFにする
            MonobitNetwork.autoJoinLobby = false;

            // MUNサーバに接続する
            MonobitNetwork.ConnectServer("NumericMatching_v1.0");
        }

        /**
		 * GUIまわりの記述.
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

            // サーバと未接続の場合何もしない
            if (!MonobitNetwork.isConnect)
            {
                return;
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
		 * 接続中＆ルーム未入室状態でのOnGUI制御.
		 */
        private void OnGUI_OutOfRoom()
        {
            // ロビー未入室の場合、何もしない
            if (!MonobitNetwork.inLobby)
            {
                return;
            }

            // ルームの作成設定
            OnGUI_CreateOrJoinRoom();
        }

        /**
		 * ルーム作成設定.
		 */
        private void OnGUI_CreateOrJoinRoom()
        {
            // ルーム最大人数の入力
            GUILayout.BeginHorizontal();
            GUILayout.Label("Room Max Players : ", GUILayout.Width(200));
            try { maxPlayers = Convert.ToByte(GUILayout.TextField(maxPlayers.ToString(), GUILayout.Width(50))); } catch { }
            GUILayout.EndHorizontal();

            // 値の入力
            GUILayout.BeginHorizontal();
            GUILayout.Label("Your Numeric Value(" + baseValueLow + "-" + baseValueHigh + ") : ", GUILayout.Width(200));
            try { baseValue = Convert.ToInt32(GUILayout.TextField(baseValue.ToString(), GUILayout.Width(50))); } catch { }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("baseValueLow", GUILayout.Width(200));
            try { baseValueLow = System.Convert.ToInt32(GUILayout.TextField(baseValueLow.ToString(), GUILayout.Width(50))); } catch { }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("baseValueHigh", GUILayout.Width(200));
            try { baseValueHigh = System.Convert.ToInt32(GUILayout.TextField(baseValueHigh.ToString(), GUILayout.Width(50))); } catch { }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("baseValueIncrease", GUILayout.Width(200));
            try { baseValueIncrease = System.Convert.ToInt32(GUILayout.TextField(baseValueIncrease.ToString(), GUILayout.Width(50))); } catch { }
            GUILayout.EndHorizontal();

            // ルームの作成
            if ( GUILayout.Button("Create Room", GUILayout.Width(250)))
            {
                // 入力した値が範囲外の場合、入力を無視する
                if( baseValue < baseValueLow || baseValue > baseValueHigh )
                {
                    Debug.LogWarning("Your value must between" + baseValueLow + " and " + baseValueHigh);
                }
                else
                {
                    // 自身の値をクエリーパラメータとして代入
                    customRoomParam["Value"] = baseValue;

                    // ロビー抽出バラメータの設定
                    string[] customRoomParametersForLobby = { "Value" };

                    // ルームの作成
                    RoomSettings roomSettings = new RoomSettings()
                    {
                        isVisible = true,
                        isOpen = true,
                        maxPlayers = this.maxPlayers,
                        roomParameters = this.customRoomParam,
                        lobbyParameters = customRoomParametersForLobby
                    };
                    MonobitNetwork.CreateRoom(null, roomSettings, null);
                }
            }

            // ルームへの入室
            if( GUILayout.Button("Join Room", GUILayout.Width(250)))
            {
                // 入力した値が範囲外の場合、入力を無視する
                if (baseValue < baseValueLow || baseValue > baseValueHigh)
                {
                    Debug.LogWarning("Your value must between" + baseValueLow + " and " + baseValueHigh);
                }
                else
                {
                    // 自身の値に対し、検索範囲の最初の閾値を設定する
                    baseValueMin = (baseValue - baseValueIncrease < baseValueLow) ? baseValueLow : baseValue - baseValueIncrease;
                    baseValueMax = (baseValue + baseValueIncrease > baseValueHigh) ? baseValueHigh : baseValue + baseValueIncrease;

                    // 閾値から、クエリー検索のためのWHERE句を生成する
                    string queryLobbyFilter = "Value>=" + baseValueMin + " AND Value<=" + baseValueMax;

                    // ルームへの入室
                    MonobitNetwork.JoinRandomRoom(null, this.maxPlayers, Definitions.MatchmakingMode.SerialMatching, new LobbyInfo() { Kind = LobbyKind.Query, Name = "QueryLobby" }, queryLobbyFilter);
                }
            }
        }

        /**
		 * ルーム内でのGUI操作.
		 */
        private void OnGUI_InRoom()
        {
            // 自身のプレイヤーIDの表示
            GUILayout.Label("Your ID : " + MonobitNetwork.player.ID);

            // ルーム内に存在するプレイヤー数の表示
            GUILayout.Label("PlayerCount : " + MonobitNetwork.room.playerCount + " / " + ((MonobitNetwork.room.maxPlayers == 0) ? "-" : MonobitNetwork.room.maxPlayers.ToString()));

            // ルームの入室制限可否設定の表示
            GUILayout.Label("Room isOpen : " + MonobitNetwork.room.open);

            // ルーム作成者が入力した値
            GUILayout.Label("Room Owner Numeric Value : " + MonobitNetwork.room.customParameters["Value"]);

            // 自身が入力した値
            GUILayout.Label("My Skill Level : " + baseValue);

            // 制限時間の表示
            if (isGameStart)
            {
                GUILayout.Label("Rest Frame : " + this.battleEndFrame);
            }

            // 部屋からの離脱
            if (GUILayout.Button("Leave Room", GUILayout.Width(150)))
            {
                MonobitNetwork.LeaveRoom();
            }

            // ホストの場合
            if (MonobitNetwork.isHost)
            {
                // ゲームスタート
                if (!isGameStart && GUILayout.Button("Start Game", GUILayout.Width(150)))
                {
                    this.isGameStart = true;
                    // room.open = false;
                    monobitView.RPC("GameStart", MonobitTargets.All, null);
                }

                // バトル終了
                if (this.battleEndFrame <= 0)
                {
                    // room.open = true;

                    // 部屋から離脱する
                    monobitView.RPC("LeaveRoomAll", MonobitTargets.All, null);
                }
            }

            // 他にルームが作成されている場合
            if( MonobitNetwork.GetRoomData() != null && MonobitNetwork.GetRoomData().Length >= 1)
            {
                // 他の部屋に入室
                if( GUILayout.Button("Enter Another Room", GUILayout.Width(150)))
                {
                    // 一旦部屋から離脱する
                    MonobitNetwork.LeaveRoom();
                    isEnterAnotherRoom = true;
                }
            }
        }

        /**
		 * 更新処理.
		 */
        public void Update()
        {
            // ルーム入室中でなければ処理しない
            if (!MonobitNetwork.isConnect || !MonobitNetwork.inRoom)
            {
                return;
            }

            // ゲームスタート後、自分のキャラクタのSpawnが終わってなければ、自身をSpawnする
            if (this.isGameStart && !this.isSpawnMyChara)
            {
                GameStart();
            }

            // ゲームスタート後、ホストなら、制限時間管理をする
            if (this.isGameStart && MonobitNetwork.isHost)
            {
                // 制限時間の減少
                if (this.battleEndFrame > 0)
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
            position.x = UnityEngine.Random.Range(-10.0f, 10.0f);
            position.z = UnityEngine.Random.Range(-10.0f, 10.0f);
            Quaternion rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(-180.0f, 180.0f), Vector3.up);

            // プレイヤーの配置（他クライアントにも同時にInstantiateする）
            MonobitNetwork.Instantiate("SD_unitychan_generic_PC", position, rotation, 0);

            // 出現させたことを確認
            this.isSpawnMyChara = true;
        }

        /**
         * 制限時間を受信.
         */
        [MunRPC]
        void TickCount(int frame)
        {
            // ゲームスタートフラグを立てる（途中参加者のための処置）
            this.isGameStart = true;

            // 制限時間を同期する
            this.battleEndFrame = frame;
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
		 * サーバ接続成功時の処理.
		 */
        public void OnConnectedToServer()
        {
            Debug.Log("OnConnectedToServer");

            // クエリーロビーに入室
            MonobitNetwork.JoinLobby(new LobbyInfo() { Kind = LobbyKind.Query, Name = "QueryLobby" });
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
            // すでにルーム入室に成功している場合、連続して処理をしないようにする
            if ((Int16)mun.STREAM.ERRORCODE_ID.RESULT_FAILURE_MASTER_ALREADY_JOINED_ROOM == (Int16)parameters[0]) return;

            // 閾値が限界まで達してしまった場合、または baseValueIncreaseが0以下の場合、
            // そもそもルームが存在しなかった（あるいは検索中に消えた、検索対象がなくなった）ことになるので、エラーで抜ける。
            if (baseValueIncrease <= 0 || (baseValueMin == baseValueLow && this.baseValueMax == baseValueHigh))
            {
                Debug.Log("OnMonobitRandomJoinFailed : ErrorCode = " + parameters[0] + ", DebugMsg = " + parameters[1]);
                return;
            }

            // ランダムルーム入室のクエリー検索の閾値を広げる。
            if (Mathf.Abs(baseValueMax - baseValue) <= Math.Abs(baseValueMin - baseValue))
            {
                if ((baseValueMax + baseValueIncrease) > baseValueHigh)
                {
                    baseValueMax = baseValueHigh;
                    baseValueMin = Mathf.Max(baseValueMin - baseValueIncrease, baseValueLow);
                }
                else
                {
                    baseValueMax += baseValueIncrease;
                }
            }
            else
            {
                if ((baseValueMin - baseValueIncrease) < baseValueLow)
                {
                    baseValueMin = baseValueLow;
                    baseValueMax = Mathf.Min(baseValueMax + baseValueIncrease, baseValueHigh);
                }
                else
                {
                    baseValueMin -= baseValueIncrease;
                }
            }

            // 閾値から、クエリー検索のためのWHERE句を生成する
            string queryLobbyFilter = "Value>=" + baseValueMin + " AND Value<=" + baseValueMax;

            // ルームへの入室
            MonobitNetwork.JoinRandomRoom(null, this.maxPlayers, Definitions.MatchmakingMode.SerialMatching, new LobbyInfo() { Kind = LobbyKind.Query, Name = "QueryLobby" }, queryLobbyFilter);
        }

        /**
		 * 指定ルーム入室失敗時の処理.
		 */
        public void OnJoinRoomFailed(object[] parameters)
        {
            Debug.Log("OnJoinRoomFailed : ErrorCode = " + parameters[0] + ", DebugMsg = " + parameters[1]);
        }

        /**
         * ルーム退室時の処理.
         */
        public void OnLeftRoom()
        {
            Debug.Log( "OnLeftRoom" );
            this.isGameStart = false;
            this.isSpawnMyChara = false;
            this.battleEndFrame = 60 * 60;
            
            if ( isEnterAnotherRoom ){
                isEnterAnotherRoom = false;
                
                // 自身の値に対し、検索範囲の最初の閾値を設定する
                baseValueMin = (baseValue - baseValueIncrease < baseValueLow) ? baseValueLow : baseValue - baseValueIncrease;
                baseValueMax = (baseValue + baseValueIncrease > baseValueHigh) ? baseValueHigh : baseValue + baseValueIncrease;

                // 閾値から、クエリー検索のためのWHERE句を生成する
                string queryLobbyFilter = "Value>=" + baseValueMin + " AND Value<=" + baseValueMax;

                // ルームへの入室
                MonobitNetwork.JoinRandomRoom(null, this.maxPlayers, Definitions.MatchmakingMode.SerialMatching, new LobbyInfo() { Kind = LobbyKind.Query, Name = "QueryLobby" }, queryLobbyFilter);
            }
        }
    }
}
