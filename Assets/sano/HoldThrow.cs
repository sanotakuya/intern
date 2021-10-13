using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldThrow : MonobitEngine.MonoBehaviour
{

    [Header("オブジェクト探査用オブジェクト")] public GameObject objectRadarObj;

    ObjectRadar objectRadar;    //オブジェクト探査スクリプト（あたり判定にあたってるオブジェクト取得のため）

    Vector3 playerPos;      //プレイヤーの現在位置
    GameObject holdObject;  //掴むオブジェクト

    bool isHold;//オブジェクトを掴んでいるかどうか

    // Start is called before the first frame update
    void Start()
    {
        objectRadar = objectRadarObj.GetComponent<ObjectRadar>();
        isHold = false;
    }

    // Update is called once per frame
    void Update()
    {
        playerPos = transform.position; //プレイヤー位置更新

        if (objectRadar.throwObjects != null)
        {
            //オブジェクトをつかむ
            if (Input.GetKeyDown(KeyCode.F))
            {
                // ブロックをもち上げる
                holdObject = objectRadar.throwObjects[0];
                holdObject.transform.position = new Vector3(playerPos.x, playerPos.y + 2.0f, playerPos.z);
                Debug.Log("掴みます");
                isHold = true;
            }
        }
        if (isHold == true)
        {
            // ブロックをもち上げる
            holdObject.transform.position = new Vector3(playerPos.x, playerPos.y + 2.0f, playerPos.z);
        }
    }
}
