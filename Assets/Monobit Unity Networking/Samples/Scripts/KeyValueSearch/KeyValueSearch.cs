using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MonobitEngine;

namespace MonobitEngine.Sample
{
	public class KeyValueSearch : MonobitEngine.MonoBehaviour
	{
		// カスタムパラメータリスト
		Hashtable customRoomParam = new Hashtable();

		// カスタムパラメータキー（GUI入力用）
		string customRoomKey = "";

		// クエリー照合パラメータ
		string keyValueFilter = "";

		// マッチングルームの最大人数
		private byte maxPlayers = 10;

		// マッチングルームを公開するかどうかのフラグ
		private bool isVisible = true;

		// ゲームスタートフラグ
		private bool isGameStart = false;

		// 制限時間
		private int battleEndFrame = 60 * 60;

		// 自身のオブジェクトを生成したかどうかのフラグ
		private bool isSpawnMyChara = false;

		/**
		 * 開始関数.
		 */
		public void Awake()
		{
			// デフォルトロビーへの強制入室をOFFにする
			MonobitNetwork.autoJoinLobby = false;

			// MUNサーバに接続する
			MonobitNetwork.ConnectServer("KeyValueSearch_v1.0");
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
			if( !MonobitNetwork.inLobby)
			{
				return;
			}

            // ルーム一覧情報の表示
            OnGUI_RoomInfo();

			// ルームの作成設定
			OnGUI_CreateRoom();

			// ルームの入室設定
			OnGUI_JoinRoom();
		}

        /**
         * ルーム一覧情報.
         */
        private void OnGUI_RoomInfo()
        {
            // 表題
            GUILayout.Label("RoomInfo", new GUIStyle() { normal = new GUIStyleState() { textColor = Color.white }, fontStyle = FontStyle.Bold });
            GUILayout.BeginHorizontal();
            GUILayout.Space(25);

            // ロビー通知されているルーム一覧情報の取得
            RoomData[] roomList = MonobitNetwork.GetRoomData();
            foreach (RoomData room in roomList)
            {
                string roomInfo = "RoomName : " + room.name + " CustomParam ";
                foreach( string key in room.customParameters.Keys)
                {
                    roomInfo += "Key : " + key + " ";
                    roomInfo += "Value : " + room.customParameters[key].ToString() + " ";
                }
                GUILayout.Label(roomInfo);
            }

            GUILayout.EndHorizontal();
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
			GUILayout.Label("Custom Parameters");

			// ルームのカスタムパラメータの値入力
			if (this.customRoomParam.Count > 0)
			{
				Hashtable tmp = new Hashtable(this.customRoomParam);
				foreach (string key in this.customRoomParam.Keys)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("Key : " + key + ", Value : ", GUILayout.Width(125));
					tmp[key] = GUILayout.TextField(this.customRoomParam[key].ToString(), GUILayout.Width(200));
					if (GUILayout.Button("Remove", GUILayout.Width(75)))
					{
						tmp.Remove(key);
					}
					GUILayout.EndHorizontal();
				}
				this.customRoomParam = tmp;
			}

			// ルームのカスタムパラメータのキー追加
			GUILayout.BeginHorizontal();
			GUILayout.Label("New Key : ", GUILayout.Width(100));
			customRoomKey = GUILayout.TextField(customRoomKey, GUILayout.Width(200));
			if (GUILayout.Button("Add", GUILayout.Width(75)))
			{
				if (!string.IsNullOrEmpty(customRoomKey))
				{
					this.customRoomParam[customRoomKey] = "" as object;
					customRoomKey = "";
				}
			}
			GUILayout.EndHorizontal();

			// 自分の作成するルーム名を公開設定にするかどうかのフラグ
			this.isVisible = GUILayout.Toggle(this.isVisible, "Visible room");

			// 自分のルームに入室可能な収容人数の設定
			GUILayout.BeginHorizontal(new GUIStyle() { alignment = TextAnchor.MiddleLeft });
			GUILayout.Label("Max Players : ", GUILayout.Width(100));
			string tmpInput = GUILayout.TextField(this.maxPlayers.ToString(), GUILayout.Width(50));
			byte.TryParse(tmpInput, out this.maxPlayers);

			// 任意の名称を持つルームを作成する
			if (GUILayout.Button("Create Room", GUILayout.Width(150)))
			{
                // カスタムルームパラメータから、ロビー送信パラメータを抽出（とりあえず全部送信するようにする）
                string[] customRoomParametersForLobby = new string[this.customRoomParam.Keys.Count];
                int keyIndex = 0;
                foreach( string key in this.customRoomParam.Keys)
                {
                    customRoomParametersForLobby[keyIndex] = key;
                    keyIndex++;
                }

				RoomSettings roomSettings = new RoomSettings()
				{
					isVisible = this.isVisible,
					isOpen = true,
					maxPlayers = this.maxPlayers,
					roomParameters = this.customRoomParam,
					lobbyParameters = customRoomParametersForLobby
				};

				MonobitNetwork.CreateRoom(null, roomSettings, null);
			}

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		/**
		 * ルーム入室設定.
		 */
		private void OnGUI_JoinRoom()
		{
			// 表題
			GUILayout.Label("Join Room", new GUIStyle() { normal = new GUIStyleState() { textColor = Color.white }, fontStyle = FontStyle.Bold });
			GUILayout.BeginHorizontal();
			GUILayout.Space(25);
			GUILayout.BeginVertical();

			// クエリー文の入力
			GUILayout.BeginHorizontal();
			GUILayout.Label("Key Values : ");
			this.keyValueFilter = GUILayout.TextField(this.keyValueFilter, GUILayout.Width(400));
			GUILayout.EndHorizontal();

			// カスタムルームパラメータと一致するランダム入室
			if (GUILayout.Button("Join Room", GUILayout.Width(150)))
			{
				MonobitNetwork.JoinRandomRoom(null, this.maxPlayers, Definitions.MatchmakingMode.SerialMatching, new LobbyInfo() { Kind = LobbyKind.Query, Name = "QueryLobby" }, this.keyValueFilter);
			}

			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
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
			GUILayout.Label("Room is : " + ((MonobitNetwork.room.open) ? "open," : "close,") + " and " + ((MonobitNetwork.room.visible) ? "visible." : "invisible."));

			// カスタムルームパラメータの表示
			GUILayout.Label("Custom Room Parameters");
			foreach (DictionaryEntry dic in MonobitNetwork.room.customParameters)
			{
				GUILayout.Label("\tKeys : " + dic.Key + ", Value : " + dic.Value);
			}

			// 制限時間の表示
			if (isGameStart)
			{
				GUILayout.Label("Rest Frame : " + this.battleEndFrame);
			}

			// 部屋からの離脱
			if (GUILayout.Button("Leave Room", GUILayout.Width(100)))
			{
				MonobitNetwork.LeaveRoom();
			}

			// ホストの場合
			if (MonobitNetwork.isHost)
			{
                // ルームのカスタムパラメータの値入力
                bool isCustomRoomParamChanged = false;
                if (this.customRoomParam.Count > 0)
                {
                    Hashtable tmp = new Hashtable(this.customRoomParam);
                    foreach (string key in this.customRoomParam.Keys)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Key : " + key + ", Value : ");
                        tmp[key] = GUILayout.TextField(this.customRoomParam[key].ToString(), GUILayout.Width(200));
                        GUILayout.EndHorizontal();
                    }
                    foreach (string key in this.customRoomParam.Keys)
                    {
                        if (!tmp.ContainsKey(key) || tmp[key] != this.customRoomParam[key])
                        {
                            isCustomRoomParamChanged = true;
                            this.customRoomParam = tmp;
                            break;
                        }
                    }
                }

                // ルームのカスタムパラメータのキー追加
                GUILayout.BeginHorizontal();
                GUILayout.Label("New Key : ");
                customRoomKey = GUILayout.TextField(customRoomKey, GUILayout.Width(200));
                if (GUILayout.Button("Add", GUILayout.Width(75)))
                {
                    if (!string.IsNullOrEmpty(customRoomKey))
                    {
                        isCustomRoomParamChanged = true;
                        this.customRoomParam[customRoomKey] = "" as object;
                        customRoomKey = "";
                    }
                }
                if (isCustomRoomParamChanged)
                {
                    MonobitNetwork.room.SetCustomParameters(this.customRoomParam);
                }
                GUILayout.EndHorizontal();

                // ゲームスタート
                if (!isGameStart && GUILayout.Button("Start Game", GUILayout.Width(100)))
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
			position.x = Random.Range(-10.0f, 10.0f);
			position.z = Random.Range(-10.0f, 10.0f);
			Quaternion rotation = Quaternion.AngleAxis(Random.Range(-180.0f, 180.0f), Vector3.up);

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
