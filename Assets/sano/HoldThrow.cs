using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]		佐野拓哉
//!	[最終更新日]	2021/10/13
//! [内容]		オブジェクトを持ったり投げたりするクラス
//-----------------------------------------------------------------------------
public class HoldThrow : MonobitEngine.MonoBehaviour
{

    // 外部参照

    [Header("オブジェクト探査用オブジェクト")] public GameObject objectRadarObj;

    ObjectRadar objectRadar;    // オブジェクト探査スクリプト（あたり判定にあたってるオブジェクト取得のため）

    MonobitView monobitView;

    // プレイヤーステータス
    private bool isHold;    // オブジェクトを掴んでいるかどうか
    Vector3 playerPos;      // プレイヤーの現在位置

    // 入力
    private bool isInput = false;   // 入力があるか管理する
    private float inputCnt = 0;     // 入力があってからのカウント
    const int INPUTWAIT = 10;       // 入力ウェイト

    // 掴む処理（掴んだオブジェクト）
    float minDistance = 100.0f;     // 最短距離(とりあえず大きめ数値入力しておく)
    const float RESETDISTANCE = 100;// 最短距離リセット
    GameObject holdObject;          // 掴むオブジェクト
    Rigidbody rbHoldObj;            // 掴んだオブジェクトの物理挙動
    float holdAngle;                // 掴んだオブジェクトの向き

    //投げる処理//
    //向き
    Plane plane = new Plane(); //　Rayを受け止めるためのオブジェクト
    float distance = 0;        //　交点の距離
    //角度
    [Header("飛ばす角度を保留するオブジェクト(仮)")] public GameObject meter;   
    bool isChangeRot;           //飛ばす角度を正の方向と負の方向に切り替える
    float nowRot;               //現在の角度
    //力
    public float throwPower;   //  オブジェクトを投げる力
    Vector3 forceDirection;    //　力を与える向き

    // Start is called before the first frame update
    void Start()
    {
        objectRadar = objectRadarObj.GetComponent<ObjectRadar>();
        isHold = false;


        monobitView = GetComponent<MonobitView>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (!monobitView.isMine)
        //{
        //    return;
        //}

        playerPos = transform.position; //プレイヤー位置更新

        if (objectRadar.throwObjects != null)
        {
            if (isHold == false)
            {
                // 最短距離を計算
                for (int i = 0; i < objectRadar.throwObjects.Count; i++)
                {
                    //プレイヤーの距離とオブジェクトの中心距離を計測する
                    float distance = Vector3.Distance(transform.position, objectRadar.throwObjects[i].transform.position);
                   
                    //現在の最短距離よりも近くにあれば
                    if (distance <= minDistance)
                    {
                        minDistance = distance;                     //最短距離更新
                        holdObject = objectRadar.throwObjects[i];   //掴む用の変数に格納する
                    }
                }

                // オブジェクトをつかむ
                if (Input.GetKeyDown(KeyCode.F) && isInput == false)
                {
                    if (holdObject != null)
                    {
                        // 最短距離のオブジェクトを掴む
                        holdObject.transform.position = new Vector3(playerPos.x, playerPos.y + 1.5f, playerPos.z);

                        //オブジェクトの回転初期化
                        holdAngle = 0.0f;

                        //プレイヤーの子オブジェクトにする
                        holdObject.transform.parent = this.transform;

                        isHold = true;
                        isInput = true;
                    }
                }
            }
        }
        if (isHold == true)
        {
            // ブロックをもち上げる
            holdObject.transform.position = new Vector3(playerPos.x, playerPos.y + 1.5f, playerPos.z);
            
            //オブジェクトの加速度初期化
            rbHoldObj = holdObject.GetComponent<Rigidbody>();
            rbHoldObj.velocity = Vector3.zero;

            //掴んでいるオブジェクトの回転
            Debug.Log(holdAngle);
            rbHoldObj.transform.rotation = Quaternion.AngleAxis(holdAngle, new Vector3(0, 0, 1));

            //マウスカーソルの位置にプレイヤーを向ける
            ChangePlayerDirection();

            //オブジェクトの回転（右回転）
            if (Input.GetKeyDown(KeyCode.Q) && isInput == false)
            {
                holdAngle += 90.0f;
            }
            //オブジェクトの回転（左回転）
            if (Input.GetKeyDown(KeyCode.E) && isInput == false)
            {
                holdAngle -= 90.0f;
            }

            // オブジェクトを投げる

            //投げる角度更新
            ChangeMaterAngle();

            //指定の角度にオブジェクトを飛ばす
            if (Input.GetKeyDown(KeyCode.F) && isInput == false)
            {
                //親から離脱する
                holdObject.transform.parent = null;

               

                rbHoldObj.AddForce(transform.forward * throwPower, ForceMode.Impulse);

                holdObject = null;
                isHold = false;
                isInput = true;
                minDistance = RESETDISTANCE;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isInput == true)
        {
            inputCnt += 1;
            if (inputCnt >= INPUTWAIT)
            {
                isInput = false;
                inputCnt = 0;
            }
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]		マウスカーソルの位置にプレイヤーを向ける関数
    //-----------------------------------------------------------------------------
    void ChangePlayerDirection()
    {
        //カメラ位置とマウス位置をもとにRayを作成する
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.red, 5);

        // プレイヤーのいるZ軸にPlaneを更新して、カメラの情報を元にマウスカーソルの位置を取得
        plane.SetNormalAndPosition(Vector3.forward, transform.localPosition);
        if (plane.Raycast(ray, out distance))
        {
            //当たったところの座標取得
            Vector3 hitPoint = ray.GetPoint(distance);

            // 座標を元にベクトルを算出して、交点の方を向く
            float vec = transform.position.x - hitPoint.x;
            Debug.Log("交点" + vec);

            //マウスカーソルのある場所を向く
            if (vec >= 0)       //プレイヤーの右側にカーソルがあるとき
            {
                float nowAngle = this.transform.eulerAngles.y;
                float angle = Mathf.LerpAngle(0.0f, 270.0f, nowAngle);
                this.transform.eulerAngles = new Vector3(0, angle, 0);
            }
            else if (vec < 0)    //プレイヤーの左側にカーソルがあるとき
            {
                float nowAngle = this.transform.eulerAngles.y;
                float angle = Mathf.LerpAngle(nowAngle, 90.0f, 1.0f);
                this.transform.eulerAngles = new Vector3(0, angle, 0);
            }
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]		投げる角度を計算する関数
    //-----------------------------------------------------------------------------
    void ChangeMaterAngle()
    {
        //角度が0.1度以下になるとtureになる。0度は360度
        if (0.1f >= meter.transform.eulerAngles.z)
        {
            isChangeRot = true;
        }
        //90度以上になるとfalseに切り替わる
        if (90 <= meter.transform.eulerAngles.z)
        {
            isChangeRot = false;
        }

        //trueなら角度を1足す、falseなら-1足す
        if (isChangeRot)
        {
            nowRot = 0.5f;
        }
        else
        {
            nowRot = -0.5f;
        }
        //メーターの角度に反映する
        meter.transform.Rotate(0, 0, nowRot);
    }

    void CalcForceDirection()
    {
        // 入力された角度をラジアンに変換
        float rad = meter.transform.eulerAngles.z * Mathf.Deg2Rad;

        // それぞれの軸の成分を計算
        float x = Mathf.Cos(rad);
        float y = Mathf.Sin(rad);
        float z = 0f;

        // Vector3型に格納
        forceDirection = new Vector3(x, y, z);
    }
}
