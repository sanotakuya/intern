using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectThrow : MonoBehaviour
{
    Camera mainCamera;
    GameObject cube;
    GameObject cubeInstance;

    public float power;
    Vector3 mousePos;
    Vector3 playerPos;
    Vector3 cursorVec;

    
    // Start is called before the first frame update
    void Start()
    {
        cube = Resources.Load<GameObject>("Cube (1)");

        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        playerPos = transform.position;

        //マウスのスクリーン座標をワールド座標に変換する
        mousePos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f));
        //マウスのZ軸を固定する
        mousePos.z = 0.0f;

        Debug.Log(mousePos);
        //オブジェクトを飛ばす
        if (Input.GetKeyDown(KeyCode.F))
        {
            //インスタンスを生成
            cubeInstance = Instantiate(cube, new Vector3(transform.position.x, transform.position.y + 2.0f, transform.position.z), Quaternion.identity);
            //インスタンスのリジッドボディを取得
            Rigidbody rb = cubeInstance.GetComponent<Rigidbody>();
            //マウスカーソルまでのベクトルを計算
            cursorVec = mousePos - playerPos;
            cursorVec.z = 0.0f;
            //単位ベクトルに向けてオブジェクトを射出する
            rb.AddForce(cursorVec.normalized * power, ForceMode.Impulse);
            Debug.Log("ベクトル" + cursorVec.normalized);
        }
    }
}
