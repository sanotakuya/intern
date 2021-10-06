using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]		小野龍大
//!	[最終更新日]	2021/10/06
//! [内容]		スタンプを送る
//-----------------------------------------------------------------------------
public class SendStamp : MonobitEngine.MonoBehaviour
{
    // MonobitView コンポーネント
    MonobitEngine.MonobitView monobitView = null;
    [SerializeField] float destroyTime = 2.0f;
    [SerializeField] Vector3 relative = new Vector3(0, 2, 0);

    // スタンプの受信
    [MunRPC]
    void SpawnStamp(string stampName, Vector3 pos)
    {
        GameObject stamp = MonobitNetwork.Instantiate(("icon/" + stampName), pos +relative, Quaternion.Euler(0, 0, 0), 0) as GameObject;
        Destroy(stamp, destroyTime);
    }

    void Awake()
    {
        monobitView = GetComponent<MonobitEngine.MonobitView>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        string stampName;

        if (Input.GetKeyDown(KeyCode.Alpha1))   //スマイルの送信
        {
            stampName = "smile";

            monobitView.RPC(
                "SpawnStamp", 
                MonobitEngine.MonobitTargets.All, 
                stampName, 
                this.gameObject.transform.position
                );
        }
    }
}
