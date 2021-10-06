using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//!	[最終更新日] 2021/10/06
//! [内容]       横から見たカメラ
//-----------------------------------------------------------------------------
public class SideViewCamera : MonoBehaviour
{
    [Header("カメラ")]                            public Transform  cameraTransform      ; // カメラオブジェクト
    [Header("追従するオブジェクト")]              public GameObject followUpObject       ; // 追従するオブジェクト
    [Header("オブジェクトとの距離")]              public float      distance             ; // 追従するオブジェクトとの距離
    [Header("オブジェクトとカメラの角度")]        public float      angle                ; // オブジェクトとカメラの角度
    /*[Header("ディレイ有効")]                  public*/ bool       isDelay        = true; // ディレイ有効
    [Header("ディレイスピード")]                  public float      delaySpeed     = 4.0f; // ディレイスピード

    private Vector3 prevCameraAngle = Vector3.zero; // 前フレームのカメラの角度
    private Vector3 prevCameraPos   = Vector3.zero; // 前フレームのカメラの位置
    private Vector3 prevObjectPos   = Vector3.zero; // 前フレームのオブジェクトの座標
    private float   limitAngle      = 90.0f       ; // 回転角度の制限

    // Start is called before the first frame update
    void Start()
    {
        // カメラの位置を設定
        cameraTransform.position = followUpObject.transform.position + new Vector3(0.0f, 0.0f, distance);
        prevCameraPos = cameraTransform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // カメラとオブジェクトの位置を同期
        if (isDelay) {
            float cameraYPos         = cameraTransform.position.y;
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, followUpObject.transform.position + new Vector3(0.0f, 0.0f, distance), delaySpeed * Time.deltaTime);
            cameraTransform.position = new Vector3(cameraTransform.position.x, cameraYPos, cameraTransform.position.z);
        }
        else {
            cameraTransform.position += followUpObject.transform.position - prevObjectPos + new Vector3(0.0f, 0.0f, distance - prevCameraPos.z);
            prevCameraPos                    = cameraTransform.position;
            prevObjectPos                    = followUpObject.transform.position;
        }

        // カメラの角度を設定(角度の制限以内なら回転)
        if (angle <= limitAngle && angle >= -limitAngle) {
            cameraTransform.transform.RotateAround(followUpObject.transform.position, transform.right, angle - prevCameraAngle.x);
            prevCameraAngle = cameraTransform.transform.rotation.eulerAngles;
        }
    }
}
