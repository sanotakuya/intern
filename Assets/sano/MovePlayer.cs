using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]		佐野拓哉
//!	[最終更新日]	2021/10/13
//! [内容]		プレイヤーの移動に関してのクラス
//-----------------------------------------------------------------------------
public class MovePlayer : MonobitEngine.MonoBehaviour
{
    [Tooltip("通常時の歩行速度")] public float movePower; //通常時の歩行速度
    [Tooltip("ジャンプ力")] public float jumpPower; //ジャンプ力
    [Tooltip("法定速度")] public float maxSpeed;  //最大速度

    [Header("足元チェック用オブジェクト")] public GameObject groundCheckObj;

    public bool myCharactor = false;

    Rigidbody rb;
    GroundCheck groundCheck;
    HoldThrow holdThrow;

    MonobitView monobitView = null;

    private float firstMovePower;   //通常時の速度を保存する
    private float firstSpeed;       //通常時の最高速度を保存する

    private bool isRunning;       //現在は知っている状態なのかを取得する
    
    public float targetAngle;   //次のプレイヤーの向き
    public  bool isAnotherHold;  //誰かにHoldされていないか

    bool isGroundTouch; //現在プレイヤーが地面に着いているかのフラグ
    bool isDepthLock;   //Z軸固定されているかのフラグ
    bool isHold;        //オブジェクトが掴まれているかのフラグ
    bool isJump;        //プレイヤーがジャンプしているかのフラグ

    // アニメーション
    public Animator animator;

    bool updateNetwork = false;

    private bool lastUpdateJump = false;
    private bool lastUpdateRightWalk = false;
    private bool lastUpdateLiftWalk = false;

    private bool lastUpdateUpWalk = false;
    private bool lastUpdateDownWalk = false;
    private bool lastUpdateRun = false;

    [MunRPC]
    void RecvJump(int id)
    {
        if (this.monobitView.viewID == id)
        {
            if (isGroundTouch == true)
            {
                //上に飛ばすだけ 
                rb.AddForce(new Vector3(0.0f, jumpPower, 0.0f), ForceMode.Impulse);
                animator.SetBool("isJump", true);
                isJump = true;
            }
        }
    }
    [MunRPC]
    void RecvLeftWalk(int id,bool isWalk)
    {
        if (this.monobitView.viewID == id)
        {
            lastUpdateLiftWalk = isWalk;
        }
    }
    [MunRPC]
    void RecvRightWalk(int id, bool isWalk)
    {
        if (this.monobitView.viewID == id)
        {
            lastUpdateRightWalk = isWalk;
        }
    }
    [MunRPC]
    void RecvUpWalk(int id, bool isWalk)
    {
        if (this.monobitView.viewID == id)
        {
            lastUpdateUpWalk = isWalk;
        }
    }
    [MunRPC]
    void RecvDownWalk(int id, bool isWalk)
    {
        if (this.monobitView.viewID == id)
        {
            lastUpdateDownWalk = isWalk;
        }
    }
    [MunRPC]
    void RecvRun(int id, bool isRun)
    {
        if (this.monobitView.viewID == id)
        {
            lastUpdateRun = isRun;
        }
    }
    private void Awake()
    {
        if (!MonobitNetwork.isHost)
        {
            Destroy(this.GetComponent<Rigidbody>());
            Destroy(this.GetComponent<BoxCollider>());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
        animator = this.gameObject.GetComponent<Animator>();
        groundCheck = groundCheckObj.GetComponent<GroundCheck>();
        holdThrow = this.gameObject.GetComponent<HoldThrow>();
        monobitView = GetComponent<MonobitView>();

        firstMovePower = movePower;
        firstSpeed = maxSpeed;
        isDepthLock = false;
        isAnotherHold = false;
    }

   
    private void Update()
    {
        if (myCharactor==true)
        {
            

            if (Input.GetKeyDown(KeyCode.Space))
            {
                monobitView.RPC("RecvJump", MonobitEngine.MonobitTargets.Host, monobitView.viewID);
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                monobitView.RPC("RecvLeftWalk", MonobitEngine.MonobitTargets.Host, monobitView.viewID, true);
            }
            if (Input.GetKeyUp(KeyCode.A))
            {
                monobitView.RPC("RecvLeftWalk", MonobitEngine.MonobitTargets.Host, monobitView.viewID, false);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                monobitView.RPC("RecvRightWalk", MonobitEngine.MonobitTargets.Host, monobitView.viewID, true);
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                monobitView.RPC("RecvRightWalk", MonobitEngine.MonobitTargets.Host, monobitView.viewID, false);
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                monobitView.RPC("RecvUpWalk", MonobitEngine.MonobitTargets.Host, monobitView.viewID, true);
            }
            if (Input.GetKeyUp(KeyCode.W))
            {
                monobitView.RPC("RecvUpWalk", MonobitEngine.MonobitTargets.Host, monobitView.viewID, false);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                monobitView.RPC("RecvDownWalk", MonobitEngine.MonobitTargets.Host, monobitView.viewID, true);
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                monobitView.RPC("RecvDownWalk", MonobitEngine.MonobitTargets.Host, monobitView.viewID, false);
            }
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                monobitView.RPC("RecvRun", MonobitEngine.MonobitTargets.Host, monobitView.viewID, true);
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                monobitView.RPC("RecvRun", MonobitEngine.MonobitTargets.Host, monobitView.viewID, false);
            }
        }

        ////ジャンプ
        //if (lastUpdateJump == true && isGroundTouch == true && MonobitNetwork.isHost == true)
        //{
          
        //}

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!MonobitNetwork.isHost)
        {
            return;
        }

        //if (isAnotherHold == true)
        //{
        //    monobitView.TransferOwnership(MonobitEngine.MonobitNetwork.host);
        //}
        //他スクリプトからの参照
        isGroundTouch = groundCheck.isHitGround;
        isHold = holdThrow.isHold;

        //移動と回転(shift押されているときはZ軸移動不可）
        float nowAngle = this.transform.eulerAngles.y;
        float angle = Mathf.LerpAngle(0.0f, targetAngle, nowAngle);
        this.transform.eulerAngles = new Vector3(0, angle, 0);


        if (isJump == true && isGroundTouch == true)
        {
            isJump = false;
        }

        if (isGroundTouch == true)
        {
            if (this.GetComponent<Animator>().enabled == false)
            {
                this.GetComponent<Animator>().enabled = true;
            }
        }
        if (lastUpdateLiftWalk == true)
        {
            //rb.velocity = new Vector3(movePower*Time.deltaTime, 0.0f, 0.0f);
            //速度上限
            if (rb.velocity.magnitude <= maxSpeed)
            {
                rb.AddForce(new Vector3(-movePower, 0.0f, 0.0f), ForceMode.Force);
                if (isRunning == true) animator.SetBool("isRun", true);
                else if (isRunning == false) animator.SetBool("isWalk", true);
            }
            targetAngle = -90.0f;
        }
        else if (lastUpdateRightWalk==true)
        {
            //rb.velocity = new Vector3(movePower*Time.deltaTime, 0.0f, 0.0f);
            //速度上限
            if (rb.velocity.magnitude <= maxSpeed)
            {
                rb.AddForce(new Vector3(movePower, 0.0f, 0.0f), ForceMode.Force);
                if (isRunning == true) animator.SetBool("isRun", true);
                else if (isRunning == false) animator.SetBool("isWalk", true);
            }
            targetAngle = 90.0f;
        }
        else if (lastUpdateUpWalk==true)
        {
            if (isDepthLock == false)
            {
                //rb.velocity = new Vector3(0.0f, 0.0f, movePower * Time.deltaTime);
                //速度上限
                if (rb.velocity.magnitude <= maxSpeed)
                {
                    rb.AddForce(new Vector3(0.0f, 0.0f, movePower), ForceMode.Force);
                    if (isRunning == true) animator.SetBool("isRun", true);
                    else if (isRunning == false) animator.SetBool("isWalk", true);
                }

                targetAngle = 0.0f;
            }
        }
        else if (lastUpdateDownWalk == true)
        {
            if (isDepthLock == false)
            {
                //rb.velocity = new Vector3(0.0f, 0.0f, -movePower * Time.deltaTime);
                //速度上限
                if (rb.velocity.magnitude <= maxSpeed)
                {
                    rb.AddForce(new Vector3(0.0f, 0.0f, -movePower), ForceMode.Force);
                    if (isRunning == true) animator.SetBool("isRun", true);
                    else if (isRunning == false) animator.SetBool("isWalk", true);
                }

                targetAngle = 180.0f;
            }
        }
        else
        {
            if (isRunning == true) animator.SetBool("isRun", false);
            else if (isRunning == false) animator.SetBool("isWalk", false);
        }


        //走る
        if (lastUpdateRun == true)
        {
            movePower = firstMovePower * 1.5f;
            maxSpeed = firstSpeed * 2.0f;
            animator.SetBool("isWalk", false);
            isRunning = true;
        }
        else
        {
            movePower = firstMovePower;
            maxSpeed = firstSpeed;
            isRunning = false;
            animator.SetBool("isRun", false);
        }

        //Z軸固定
        if (this.transform.position.z >= -0.1f && this.transform.position.z <= 0.1f)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                isDepthLock = true;
                this.transform.position = new Vector3(transform.position.x, transform.position.y, 0.0f);
            }
            else
            {
                isDepthLock = false;
            }
        }
        else if (this.transform.position.z != 0.0f)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                this.transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, transform.position.y, 0.0f), Time.deltaTime);
            }
        }
    }

   
    //public override void OnMonobitSerializeViewWrite(MonobitStream stream, MonobitMessageInfo info)
    //{
    //    stream.Enqueue(lastUpdateJump);
       
    //}
    //public override void OnMonobitSerializeViewRead(MonobitStream stream, MonobitMessageInfo info)
    //{
    //    updateNetwork = true;
    //    lastUpdateJump = (bool)stream.Dequeue();
    //}


    //外部参照用関数
  
    //-----------------------------------------------------------------------------
    //! [内容]		オブジェクトを投げる方向にプレイヤーを向ける
    //-----------------------------------------------------------------------------
    public  void SetPlayerHold(bool isHold)
    {
        isAnotherHold = isHold;
    }
    
    //-----------------------------------------------------------------------------
    //! [内容]		オブジェクトを投げる方向にプレイヤーを向ける
    //-----------------------------------------------------------------------------
    public bool GetAnotherHold()
    {
        return isAnotherHold;
    }

    public void AnimationReset(string str)
    {
        animator.SetBool(str, false);
    }


}
