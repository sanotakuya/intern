using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/06
//! [内容]       横から見たカメラ
//-----------------------------------------------------------------------------
public class SideViewCamera : MonoBehaviour
{
    [Header("カメラ")]                     public Transform  cameraTransform       ; // カメラオブジェクト
    [Header("追従するオブジェクト")]       public GameObject followUpObject        ; // 追従するオブジェクト
    [Header("オブジェクトとの距離")]       public float      distance              ; // 追従するオブジェクトとの距離
    [Header("オブジェクトとの高さ")]       public float      vertical              ; // オブジェクトとカメラの高さ
    [Header("オブジェクトとカメラの角度")] public float      angle                 ; // オブジェクトとカメラの角度
    [Header("ディレイ有効")]               public bool       isDelay        = true ; // ディレイ有効
    [Header("ディレイスピード")]           public float      delaySpeed     = 4.0f ; // ディレイスピード
    [Header("平行投影")]                   public bool       isParallel     = false; // 平行投影

    private Camera      mainCamera                       ; // カメラ
    private float       orthographicSize                 ; // カメラの表示範囲
    private Vector3     prevCameraAngle    = Vector3.zero; // 前フレームのカメラの角度
    private const float LIMIT_ANGLE        = 90.0f       ; // 回転角度の制限

    // Start is called before the first frame update
    void Start()
    {
        if (cameraTransform) {
            // カメラの位置を設定
            cameraTransform.position  = followUpObject.transform.position + (cameraTransform.forward * distance) + (cameraTransform.up * vertical);
            // カメラコンポーネントを取得
            mainCamera = cameraTransform.gameObject.GetComponent<Camera>();
            if (mainCamera) {
                orthographicSize = mainCamera.orthographicSize;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraTransform && followUpObject) {
            // カメラのプロジェクション設定
            if (isParallel && mainCamera) {
                mainCamera.orthographic     = true;
                mainCamera.orthographicSize = -distance;
            }
            else {
                mainCamera.orthographic     = false;
                mainCamera.orthographicSize = orthographicSize;

            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (cameraTransform && followUpObject) {
            // カメラとオブジェクトの位置を同期
            if (isDelay) {
                cameraTransform.position = Vector3.Lerp(cameraTransform.position, followUpObject.transform.position + (cameraTransform.forward * distance) + (cameraTransform.up * vertical), delaySpeed * Time.deltaTime);
            }
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (cameraTransform && followUpObject) {
            // カメラとオブジェクトの位置を同期
            if (!isDelay) {
                cameraTransform.position = followUpObject.transform.position + (cameraTransform.forward * distance) + (cameraTransform.up * vertical);
            }
            // カメラの角度を設定(角度の制限以内なら回転)
            if (angle <= LIMIT_ANGLE && angle >= -LIMIT_ANGLE) {
                cameraTransform.RotateAround(followUpObject.transform.position, transform.right, angle - prevCameraAngle.x);
                prevCameraAngle = cameraTransform.rotation.eulerAngles;
            }
        }
    }
}
