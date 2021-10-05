using UnityEngine;
using System;
using System.Collections;
using MonobitEngine;
using MonobitEngine.Definitions;

public class SD_Unitychan_PC : MonobitEngine.MonoBehaviour
{
    [SerializeField]
    public AnimationClip[] animations;                      // キャラクタの顔表情のアニメーション変更用の、BaseLayer上のアニメーションクリップ登録テーブル

    private Animator animator;                              // アニメータコントローラ
    private int animId = 0;                                 // 再生中のアニメーションID
    private int moveSpeed = 0;                              // アニメーション中の進行速度を示すID
    private bool isMainCameraDisabled = false;	            // メインカメラ復旧用フラグ
    private bool isDisplayPlayerId = false;                 // プレイヤーID表示フラグ
    private int myPlayerID = -1;                            // 自身のプレイヤーID
    public int GetPlayerID() { return myPlayerID; }         // 自身のプレイヤーID の取得
    private string myPlayerName = "";                       // 自身のプレイヤー名
    public string GetPlayerName() { return myPlayerName; }  // 自身のプレイヤー名の取得
    private int currentAnimId = 0;                          // 現在再生中のアニメーションID
    private float currentFace = 0;                          // キャラクタの顔表情のアニメーションウェイト値
    private string currentFaceName = "";                    // キャラクタの顔表情のアニメーション名
    private Vector3 jumpFixedSpeed = Vector3.zero;          // キャラクタがジャンプするときの初期平行移動量
    private UInt64 myScore = 0;                             // 自身のスコア
    public UInt64 MyScore                                   // 自身のスコアの設定
    {
        get { return myScore; }
        set { myScore = value; }
    }

    // Use this for initialization
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        animId = Animator.StringToHash("animId");
        moveSpeed = Animator.StringToHash("moveSpeed");

        if (!monobitView.isMine)
        {
            gameObject.transform.Find("Camera").GetComponent<Camera>().enabled = false;
            gameObject.transform.Find("Camera").GetComponent<AudioListener>().enabled = false;
        }
        else
        {
            GameObject.Find("Main Camera").GetComponent<Camera>().enabled = false;
            GameObject.Find("Main Camera").GetComponent<AudioListener>().enabled = false;
            monobitView.RPC("SetPlayerInfo", MonobitTargets.AllBuffered, MonobitNetwork.player.ID, MonobitNetwork.player.name);
            isMainCameraDisabled = true;
		}
    }

	void OnDestroy()
	{
		if( isMainCameraDisabled )
		{
            GameObject go = GameObject.Find("Main Camera");
            if( go != null )
            {
                go.GetComponent<Camera>().enabled = true;
            }
        }
        if (MonobitEngine.Sample.ClientSide.RakeupGame.s_PlayerObject.Contains(this))
        {
            MonobitEngine.Sample.ClientSide.RakeupGame.s_PlayerObject.Remove(this);
        }
        if (MonobitEngine.Sample.ServerSide.RakeupGame.s_PlayerObject.Contains(this))
        {
            MonobitEngine.Sample.ServerSide.RakeupGame.s_PlayerObject.Remove(this);
        }
    }

    // Update is called once per frame
    public void Update()
    {
		if (monobitView.isMine)
        {
            if (monobitView.isMine)
            {
                // キャラクタの移動＆アニメーション切り替え
                if (currentAnimId == 3)
                {
                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Jump") || animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.55)
                    {
                        gameObject.transform.position += jumpFixedSpeed;
                    }
                    else
                    {
                        jumpFixedSpeed = Vector3.zero * Time.deltaTime;
                        gameObject.transform.position += jumpFixedSpeed;
                        currentAnimId = 0;
                        animator.SetInteger(animId, currentAnimId);
                        animator.SetFloat(moveSpeed, jumpFixedSpeed.magnitude);
                    }
                }
                else if (currentAnimId == 4)
                {
                    if (animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Emotion") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.90)
                    {
                        jumpFixedSpeed = Vector3.zero * Time.deltaTime;
                        gameObject.transform.position += jumpFixedSpeed;
                        currentAnimId = 0;
                        animator.SetInteger(animId, currentAnimId);
                        animator.SetFloat(moveSpeed, jumpFixedSpeed.magnitude);
                    }
                }
                else if (Input.GetButtonDown("Jump"))
                {
                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Jump") && !animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Emotion"))
                    {
                        gameObject.transform.position += jumpFixedSpeed;
                        currentAnimId = 3;
                        animator.SetInteger(animId, currentAnimId);
                        animator.SetFloat(moveSpeed, jumpFixedSpeed.magnitude);
                    }
                }
                else if (Input.GetKeyDown("z"))
                {
                    if (animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Stand"))
                    {
                        jumpFixedSpeed = Vector3.zero * Time.deltaTime;
                        gameObject.transform.position += jumpFixedSpeed;
                        currentAnimId = 4;
                        animator.SetInteger(animId, currentAnimId);
                        animator.SetFloat(moveSpeed, jumpFixedSpeed.magnitude);
                    }
                }
                else if (Input.GetKey("up"))
                {
                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Jump") && !animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Emotion"))
                    {
                        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Stand") || animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Walk"))
                        {
                            jumpFixedSpeed = gameObject.transform.forward * 1.5f * Time.deltaTime;
                        }
                        else
                        {
                            jumpFixedSpeed = gameObject.transform.forward * 3.0f * Time.deltaTime;
                        }
                        gameObject.transform.position += jumpFixedSpeed;
                        currentAnimId = 1;
                        animator.SetInteger(animId, currentAnimId);
                        animator.SetFloat(moveSpeed, jumpFixedSpeed.magnitude);
                    }
                }
                else if (Input.GetKey("down"))
                {
                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Jump") && !animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Emotion"))
                    {
                        jumpFixedSpeed = gameObject.transform.forward * -0.1f * Time.deltaTime;
                        gameObject.transform.position += jumpFixedSpeed;
                        currentAnimId = 2;
                        animator.SetInteger(animId, currentAnimId);
                        animator.SetFloat(moveSpeed, jumpFixedSpeed.magnitude);
                        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.0f)
                        {
                            animator.Play(Animator.StringToHash("Walking@loop"), 0, 1.0f);
                        }
                    }
                }
                else
                {
                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Jump") && !animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Emotion"))
                    {
                        jumpFixedSpeed = Vector3.zero * Time.deltaTime;
                        gameObject.transform.position += jumpFixedSpeed;
                        currentAnimId = 0;
                        animator.SetInteger(animId, currentAnimId);
                        animator.SetFloat(moveSpeed, jumpFixedSpeed.magnitude);
                        ChangeFace("default@sd_generic");
                    }
                }
                if (Input.GetKey("right"))
                {
                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Jump") && !animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Emotion"))
                    {
                        gameObject.transform.Rotate(0, 30.0f * Time.deltaTime, 0);
                    }
                }
                if (Input.GetKey("left"))
                {
                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Jump") && !animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Emotion"))
                    {
                        gameObject.transform.Rotate(0, -30.0f * Time.deltaTime, 0);
                    }
                }
                if (Input.GetKeyDown("x"))
                {
                    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Jump") && !animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Emotion"))
                    {
                        MonobitNetwork.Instantiate("Cube", transform.position, transform.rotation, 0);
                    }
                }
                animator.SetLayerWeight(1, currentFace);
                monobitView.RPC("SetFaceID", MonobitTargets.OthersBuffered, currentAnimId, currentFace, currentFaceName);
            }
            else
            {
                animator.SetInteger(animId, currentAnimId);
                animator.SetLayerWeight(1, currentFace);
                foreach (var animation in animations)
                {
                    if (currentFaceName == animation.name)
                    {
                        ChangeFace(currentFaceName);
                    }
                }
            }
        }
    }

    // GUI描画メソッド
    public void OnGUI()
    {
        if (isDisplayPlayerId)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(gameObject.transform.position + new Vector3(0, 1.5f, 0));
            if (screenPos.z >= 0.0f)
            {
                GUILayout.BeginArea(new Rect(screenPos.x - 71, Screen.height - screenPos.y - 1, 140, 48));
                {
                    OnGUIInWindow(new GUIStyleState() { textColor = Color.black });
                }
                GUILayout.EndArea();
                GUILayout.BeginArea(new Rect(screenPos.x - 71, Screen.height - screenPos.y + 1, 140, 48));
                {
                    OnGUIInWindow(new GUIStyleState() { textColor = Color.black });
                }
                GUILayout.EndArea();
                GUILayout.BeginArea(new Rect(screenPos.x - 69, Screen.height - screenPos.y - 1, 140, 48));
                {
                    OnGUIInWindow(new GUIStyleState() { textColor = Color.black });
                }
                GUILayout.EndArea();
                GUILayout.BeginArea(new Rect(screenPos.x - 69, Screen.height - screenPos.y + 1, 140, 48));
                {
                    OnGUIInWindow(new GUIStyleState() { textColor = Color.black });
                }
                GUILayout.EndArea();
                GUILayout.BeginArea(new Rect(screenPos.x - 70, Screen.height - screenPos.y, 140, 48));
                {
                    OnGUIInWindow(new GUIStyleState() { textColor = Color.cyan });
                }
                GUILayout.EndArea();
            }
        }
    }

    private void OnGUIInWindow(GUIStyleState state)
    {
        GUILayout.Label(string.Format("PlayerId : {0:D4}", myPlayerID), new GUIStyle() { fontSize = 20, fontStyle = FontStyle.Bold, normal = state });
        GUILayout.Label(string.Format("Score : {0}", myScore), new GUIStyle() { fontSize = 20, fontStyle = FontStyle.Bold, normal = state });
    }

    // RPC受信：プレイヤー情報の設定
    [MunRPC]
    public void SetPlayerInfo(int playerId, string playerName)
    {
        // プレイヤーIDおよびプレイヤー名の設定を行なう
        isDisplayPlayerId = true;
        myPlayerID = playerId;
        myPlayerName = playerName;

        // 自身のオブジェクト情報を RakeupGames に登録する
        MonobitEngine.Sample.ServerSide.RakeupGame.s_PlayerObject.Add(this);
        MonobitEngine.Sample.ClientSide.RakeupGame.s_PlayerObject.Add(this);
    }

    // RPC受信：表情ウェイトの設定
    [MunRPC]
    public void SetFaceID(int animId, float faceWeight, string faceName)
    {
        currentAnimId = animId;
        currentFace = faceWeight;
        currentFaceName = faceName;
    }

    //アニメーションEvents側につける表情切り替え用イベントコール
    public void OnCallChangeFace(string str)
    {
        if (monobitView.isMine)
        {
            int ichecked = 0;
            foreach (var animation in animations)
            {
                if (str == animation.name)
                {
                    ChangeFace(str);
                    break;
                }
                else if (ichecked <= animations.Length)
                {
                    ichecked++;
                }
                else
                {
                    //str指定が間違っている時にはデフォルトで
                    str = "default@sd_generic";
                    ChangeFace(str);
                }
            }
        }
    }

    void ChangeFace(string str)
    {
        currentFace = 1;
        currentFaceName = str;
        if (animator != null)
        {
            animator.CrossFade(str, 0);
        }
    }
}