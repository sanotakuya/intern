using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//-----------------------------------------------------------------------------
//! [制作者]		小野龍大
//! [内容]		OnOffを行う
//-----------------------------------------------------------------------------
public class OnOffActive : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //!	private
    //-----------------------------------------------------------------------------
    private int cnt = 0;
    private bool isHide = false;
    [SerializeField] private int activeTime = 50;
    [SerializeField] private int hideTime = 20;
    [SerializeField] GameObject target = null;

    void FixedUpdate()
    {
        // 次官になったら切り替え
        if (isHide && cnt == hideTime)
        {
            isHide = false;
            target.gameObject.SetActive(true);
            cnt = 0;
        }
        else if (!isHide && cnt == activeTime)
        {

            isHide = true;
            target.gameObject.SetActive(false);
            cnt = 0;
        }

        cnt++;
    }
}
