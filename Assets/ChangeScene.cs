using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//-----------------------------------------------------------------------------
//! [制作者]		小野龍大
//! [内容]		シーンを切り替える
//-----------------------------------------------------------------------------
public class ChangeScene : MonoBehaviour
{
    [SerializeField] Transform trans;
    public bool isReady = false;

    private void Start()
    {
        trans = this.gameObject.transform;
    }

    void Update()
    {
        if(trans.localPosition.y < -1700)
        {
            isReady = true;
        }
    }

    private void FixedUpdate()
    {
        if(trans.localPosition.y > -3000)
        {
            Vector3 pos = trans.localPosition;
            pos.y -= 10;
            //trans.localPosition = pos;
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]		初期化して再度実行
    //-----------------------------------------------------------------------------
    public void ReStart()
    {
        Vector3 pos = trans.localPosition;
        pos.y = 1200;
        trans.localPosition = pos;
    }
}
