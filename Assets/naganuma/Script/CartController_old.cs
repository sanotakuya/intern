using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//!	[最終更新日] 2021/10/06
//! [内容]       カート操作クラス
//-----------------------------------------------------------------------------
public class CartController_old : MonobitEngine.MonoBehaviour {
    [Header("カートのスピード")] public float   cartSpeed        = 1.0f; // カートスピード
    [Header("加速度")]           public float   acceleration     = 4.0f; // 加速度
    [Header("カートの減速値")]   public float   cartDeceleration = 0.5f; // カートの低速値
    [Header("カートの中心")]     public Vector3 centerPos              ; // カートの中心

    private Rigidbody rigidBody               ; // カートのリジッドボディ
    private Vector3   speed     = Vector3.zero; // スピード

    // Start is called before the first frame update
    void Start()
    {
        // リジッドボディを取得
        rigidBody = this.GetComponent<Rigidbody>();
        if (rigidBody) {
            rigidBody.centerOfMass = centerPos;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (rigidBody)
        {
            speed = Vector3.Lerp(speed, new Vector3(cartSpeed, 0.0f), acceleration * Time.deltaTime); // カートの最終移動ベクトル
            // 低速移動
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                if (cartSpeed != 0.0f) {
                    speed.x -= cartDeceleration / cartSpeed;
                }
                // スピードをマイナス値にしない
                if (speed.x <= 0.0f) speed.x = 0.0f;
            }

            // スピードを制限
            if (rigidBody.velocity.x < cartSpeed && rigidBody.velocity.x > -cartSpeed) {
                bool isMove = false;
                // 左移動
                if (Input.GetKey(KeyCode.J)) {
                    rigidBody.velocity = -speed;
                    isMove = true;
                }
                // 右移動
                if (Input.GetKey(KeyCode.L)) {
                    rigidBody.velocity =  speed;
                    isMove = true;
                }
                if (!isMove) {
                    speed = Vector3.zero;
                }
            }
        }
    }
}
