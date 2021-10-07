using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------
//! [制作者]		佐野拓哉
//!	[最終更新日]	2021/10/06
//! [内容]		投げるテスト用カメラ移動
//-----------------------------------------------------------------------------
public class CameraMove : MonoBehaviour
{

    GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = GameObject.Find("unitychan_dynamic_locomotion(Clone)");
        }
        else if(player!=null)
        {
            this.transform.position = player.transform.position;
        }
    }
}
