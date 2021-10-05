using UnityEngine;
using System.Collections;
using MonobitEngine;
using MonobitEngine.Definitions;

public class OnlineScene : MonobitEngine.MonoBehaviour
{
    // ゲームスタートフラグ
    private bool m_bGameStart = false;

    // 制限時間
    private int battleEndFrame = 60 * 60;

    // 自身のオブジェクトを生成したかどうかのフラグ
    private bool spawnMyChara = false;

    // GUI処理
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

        // プレイヤーIDの表示
        if (MonobitNetwork.player != null)
        {
            GUILayout.Label("My Player ID : " + MonobitNetwork.player.ID);
        }

        // ルーム情報の取得
        Room room = MonobitNetwork.room;
        if (room != null)
        {
            // ルーム名の表示
            GUILayout.Label("Room Name : " + room.name);

            // ルーム内に存在するプレイヤー数の表示
            GUILayout.Label("PlayerCount : " + room.playerCount);

            // ルームがオープンかクローズか
            GUILayout.Label("Room IsOpen : " + room.open);

            // 制限時間の表示
            if (m_bGameStart)
            {
                GUILayout.Label("Rest Frame : " + this.battleEndFrame);
            }
        }
        // 部屋からの離脱
        if (GUILayout.Button("Leave Room", GUILayout.Width(100)))
        {
            // 部屋から離脱する
            MonobitNetwork.LeaveRoom();

            // 一旦切断する
            MonobitNetwork.DisconnectServer();

            // シーンをオフラインシーンへ
            MonobitNetwork.LoadLevel(OfflineScene.SceneNameOffline);
        }

        // ホストの場合
        if (MonobitNetwork.isHost)
        {
            // ゲームスタート前にゲームスタートするかどうか
            if (!m_bGameStart && GUILayout.Button("Start Game", GUILayout.Width(100)))
            {
                // ゲームスタートフラグを立てる
                m_bGameStart = true;

                // ルームをクローズする
                //room.open = false;

                // バトルスタートを通知
                monobitView.RPC("OnGameStart", MonobitTargets.All, null);
            }

            for (int i = 0; i < MonobitNetwork.otherPlayersList.Length; i++)
            {
                if (!m_bGameStart && GUILayout.Button("Switch Host ID:" + MonobitNetwork.otherPlayersList[i].ID, GUILayout.Width(200)))
                {
                    MonobitNetwork.ChangeHost(MonobitNetwork.otherPlayersList[i]);
                }
            }
        }
    }

    // 更新処理
    void Update()
    {
        // ゲームスタート後、まだ自分のキャラクタのspawnが終わってない状態であれば、自身のキャラクタをspawnする
        if (m_bGameStart && !spawnMyChara)
        {
            OnGameStart();
        }

        // ゲームスタート後、ホストなら
        if (m_bGameStart && MonobitNetwork.isHost)
        {
            // 制限時間の減少
            if (battleEndFrame > 0)
            {
                battleEndFrame--;
            }
            // 制限時間をRPCで送信
            object[] param = new object[]
            {
                battleEndFrame
            };
            monobitView.RPC("TickCount", MonobitTargets.Others, param);
            
            // バトル終了
            if ( 0 == battleEndFrame ){
                m_bGameStart = false;
                
                // ルームをオープンにする
                MonobitNetwork.room.open = true;
                
                monobitView.RPC( "OnGameEnd", MonobitTargets.AllViaServer );
            }
        }
    }

    // ゲームスタートを受信(RPC)
    [MunRPC]
    void OnGameStart()
    {
        // ゲームスタートフラグを立てる
        m_bGameStart = true;

        // ある程度ランダムな場所にプレイヤーを配置する
        Vector3 position = Vector3.zero;
        position.x = Random.Range(-10.0f, 10.0f);
        position.z = Random.Range(-10.0f, 10.0f);

        Quaternion rotation = Quaternion.AngleAxis(Random.Range(-180.0f, 180.0f), Vector3.up);

        // プレイヤーの配置（他クライアントにも同時にInstantiateする）
        MonobitNetwork.Instantiate("SD_unitychan_generic_PC", position, rotation, 0);

        // 出現させたことを確認
        spawnMyChara = true;
    }
    
    // ゲームエンドを受信(RPC)
    [MunRPC]
    void OnGameEnd()
    {
        Debug.Log( "OnGameEnd" );
        
        // 一旦切断する
        MonobitNetwork.DisconnectServer();

        // シーンをオフラインシーンへ
        MonobitNetwork.LoadLevel(OfflineScene.SceneNameOffline);
    }

    // 制限時間を受信(RPC)
    [MunRPC]
    void TickCount(int frame)
    {
        // ゲームスタートフラグを立てる（途中参加者のための処置）
        m_bGameStart = true;

        // 制限時間を同期する
        this.battleEndFrame = frame;
    }

	// 接続が切断されたときの処理
	public void OnDisconnectedFromServer()
	{
		Debug.Log("Disconnected from server");

        // シーンをオフラインシーンへ
        MonobitNetwork.LoadLevel(OfflineScene.SceneNameOffline);
    }
}
