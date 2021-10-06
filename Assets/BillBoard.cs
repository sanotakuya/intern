using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;


//-----------------------------------------------------------------------------
//! [制作者]		小野龍大
//!	[最終更新日]	2021/10/06
//! [内容]		ビルボード風にするクラス
//-----------------------------------------------------------------------------
public class BillBoard : MonobitEngine.MonoBehaviour
{
    //[SerializeField] GameObject camera;
    void Update()
    {
        // カメラの方に向ける
        Vector3 vec3 = Camera.current.transform.position;
        vec3.y = transform.position.y;
        transform.LookAt(vec3);
    }
}
