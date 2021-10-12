using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    public float movePower; //通常時の歩行速度
　　public float jumpPower; //ジャンプ力

    Rigidbody rb;
    private float firstMovePower; //通常時の速度を保存する

    float targetAngle;  //次のプレイヤーの向き
    bool isGroundTouch; //現在プレイヤーが地面に着いているかのフラグ
    bool isDepthLock;   //Z軸固定されているかのフラグ

    // Start is called before the first frame update
    void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
        firstMovePower = movePower;

        isDepthLock = false;
    }

    // Update is called once per frame
    void Update()
    {
        //移動と回転(shift押されているときはZ軸移動不可）
        float nowAngle = this.transform.eulerAngles.y;
        float angle = Mathf.LerpAngle(0.0f, targetAngle, nowAngle);
        this.transform.eulerAngles = new Vector3(0, angle, 0);
        if (isGroundTouch == true)
        {
            if (Input.GetKey(KeyCode.A))
            {
                rb.AddForce(new Vector3(-movePower, 0.0f, 0.0f), ForceMode.Force);
                targetAngle = -90.0f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                rb.AddForce(new Vector3(movePower, 0.0f, 0.0f), ForceMode.Force);
                targetAngle = 90.0f;
            }
            else if (Input.GetKey(KeyCode.W))
            {
                if (isDepthLock == false)
                {
                    rb.AddForce(new Vector3(0.0f, 0.0f, movePower), ForceMode.Force);
                    targetAngle = 0.0f;
                }
            }
            else if (Input.GetKey(KeyCode.S))
            {
                if (isDepthLock == false)
                {
                    rb.AddForce(new Vector3(0.0f, 0.0f, -movePower), ForceMode.Force);
                    targetAngle = 180.0f;
                }
            }
        
            //ジャンプ
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //上に飛ばすだけ 
                rb.AddForce(new Vector3(0.0f, jumpPower, 0.0f), ForceMode.Impulse);
            }
        }

        //走る
        if (Input.GetKey(KeyCode.LeftShift))
        {
            movePower = firstMovePower * 1.5f;
        }
        else
        {
            movePower = firstMovePower;
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
        else if(this.transform.position.z != 0.0f)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                this.transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, transform.position.y, 0.0f), Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        isGroundTouch = true;
    }
    private void OnTriggerExit(Collider other)
    {
        isGroundTouch = false;
    }
}
