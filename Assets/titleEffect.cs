using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


//-----------------------------------------------------------------------------
//! [制作者]		小野龍大
//! [内容]		タイトルの操作を行う
//-----------------------------------------------------------------------------
public class titleEffect : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //!	private
    //-----------------------------------------------------------------------------
    private RectTransform title;
    private int cnt = 0;
    private bool isFoldBack = false;
    [SerializeField] private int foldBackCnt = 50;

    [SerializeField] private Vector3 startSize = new Vector3(1.5f, 1.5f, 1.0f);     // 開始サイズ
    [SerializeField] private Vector3 maxSize = new Vector3(2.0f, 2.0f, 1.0f);       // 最大サイズ

    private void Start()
    {
        title = GetComponent<RectTransform>();
    }

    private void FixedUpdate()
    {
        title.localScale = Vector3.Lerp(startSize, maxSize, (float)cnt / (float)foldBackCnt);

        // 折返しまで来たらtrueに
        if (cnt == foldBackCnt)
        {
            isFoldBack = true;
        }
        else if (cnt == 0)
        {
            isFoldBack = false;

        }

        // 進んでる方向に向かって値を変更
        if (isFoldBack)
        {
            cnt--;
        }
        else
        {
            cnt++;

        }
    }
}
