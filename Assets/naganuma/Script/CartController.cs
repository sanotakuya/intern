using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartController : MonoBehaviour {
    [Header("カートのスピード")]
    public float      cartSpeed        = 1.0f; // カートスピード
    [Header("カートの減速値")]
    public float      cartDeceleration = 0.5f; // カートの低速値
    [Header("カートの中心")]
    public Vector3    centerPos;               // カートの中心

    private Rigidbody rigidBody;               // カートのリジッドボディ

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
            // TODO 速度の補間をする
            Vector3 speed = new Vector3(cartSpeed, 0.0f); // カートの最終移動ベクトル
            // 低速移動
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                speed.x -= cartDeceleration;
                // スピードをマイナス値にしない
                if (speed.x <= 0.0f) speed.x = 0.0f;
            }

            // スピードを制限
            if (rigidBody.velocity.x < speed.x && rigidBody.velocity.x > -speed.x) {
                // 左移動
                if (Input.GetKey(KeyCode.A))
                {
                    rigidBody.velocity = -speed;
                }
                // 右移動
                if (Input.GetKey(KeyCode.D))
                {
                    rigidBody.velocity =  speed;
                }
            }
            Debug.Log(rigidBody.velocity);
        }
    }
}
