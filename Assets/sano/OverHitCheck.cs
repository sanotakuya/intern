using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;
public class OverHitCheck : MonobitEngine.MonoBehaviour
{
    public bool isHitOver;   //頭上との当たり判定

    MonobitView m_MonobitView = null;

    private void Start()
    {
        // 親オブジェクトのMonobitViewを取得する
        if (GetComponentInParent<MonobitEngine.MonobitView>() != null)
        {
            m_MonobitView = GetComponentInParent<MonobitEngine.MonobitView>();
        }
    }
    private void Update()
    {
        if (!m_MonobitView.isMine)
        {
            return;
        }
    }
    void OnTriggerStay(Collider other)
    {
        isHitOver = true;
    }
    void OnTriggerExit(Collider other)
    {
        isHitOver = false;
    }
}
