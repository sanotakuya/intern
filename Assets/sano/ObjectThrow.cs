using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]		佐野拓哉
//!	[最終更新日]	2021/10/06
//! [内容]		オブジェクトを持ったり投げたりするクラス
//-----------------------------------------------------------------------------
public class ObjectThrow : MonobitEngine.MonoBehaviour
{
    Camera mainCamera;

    public float power;
    Vector3 mousePos;
    Vector3 playerPos;
    Vector3 cursorVec;
    MonobitView monobitView;

    GameObject hitObject;   //当たったオブジェクトを取得

    static bool objectHoldFlg = false;

    private bool inputFlg = false;
    private float inputCnt = 0;

    // Start is called before the first frame update
    void Start()
    {
        //cube = Resources.Load<GameObject>("food/Hotdog");

        mainCamera = GameObject.Find("ThrowCamera").GetComponent<Camera>();
        monobitView = GetComponent<MonobitView>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!monobitView.isMine)
        {
            return;
        }

        playerPos = transform.position;

        //マウスのスクリーン座標をワールド座標に変換する
        mousePos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f));
        //マウスのZ軸を固定する
        mousePos.z = 0.0f;

        //Debug.Log(mousePos);
        if (objectHoldFlg == false)
        {
            Ray ray = new Ray(transform.position + new Vector3(0.0f, 0.5f, 0.0f), transform.forward);            //  前方確認（オブジェクト検知）
            if (Physics.Raycast(ray, out RaycastHit hit, 1.0f))
            {

                if (hit.transform.gameObject.name == "Hotdog")
                {
                    //オブジェクトをつかむ
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        // ブロックをもち上げる
                        hitObject = hit.transform.gameObject;
                        hitObject.transform.position = new Vector3(playerPos.x, playerPos.y + 2.0f, playerPos.z);
                        objectHoldFlg = true;
                        inputFlg = true;
                    }
                }
            }
        }
        if (objectHoldFlg == true)
        {  
            //つかんでいるオブジェクトのリジッドボディを取得
            Rigidbody rb = hitObject.GetComponent<Rigidbody>();
            //動かないようにしておく
            rb.velocity = Vector3.zero;

            //掴んでいるオブジェクトの座標を更新する
            hitObject.transform.position = new Vector3(playerPos.x, playerPos.y + 2.0f, playerPos.z);

            //マウスカーソルまでのベクトルを計算
            cursorVec = mousePos - playerPos;
            cursorVec.z = 0.0f;

            //マウスカーソルのある場所を向く
            if (cursorVec.x >= 0)
            {
                //キャラの向いている方向へ向きを変える
                float nowAngle = this.transform.eulerAngles.y;
                float angle = Mathf.LerpAngle(0.0f, 90.0f, nowAngle);
                this.transform.eulerAngles = new Vector3(0, angle, 0);
            }
            else if(cursorVec.x < 0)
            {
                //キャラの向いている方向へ向きを変える
                float nowAngle = this.transform.eulerAngles.y;
                float angle = Mathf.LerpAngle(nowAngle, 270.0f, 1.0f);
                this.transform.eulerAngles = new Vector3(0, angle, 0);
            }


            if (inputFlg == false)
            {
                //オブジェクトを飛ばす
                if (Input.GetKeyUp(KeyCode.F))
                { 
                    //単位ベクトルに向けてオブジェクトを射出する
                    rb.AddForce(cursorVec.normalized * power, ForceMode.Impulse);
                    Debug.Log("ベクトル"+cursorVec);

                    objectHoldFlg = false;
                }
            }
        }
    }
    private void FixedUpdate()
    {
        if (inputFlg == true)
        {
            inputCnt += 1;
            if (inputCnt >= 10)
            {
                inputFlg = false;
                inputCnt = 0;
            }
        }
    }
}
