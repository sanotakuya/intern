using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

public class GroundCheck : MonobitEngine.MonoBehaviour
{
    public bool isHitGround;   //地面との当たり判定

    static MonobitView m_MonobitView = null;

    private void Start()
    {
        m_MonobitView = GetComponent<MonobitView>();
    }
    private void Update()
    {
        //if (!m_MonobitView.isMine)
        //{
        //    return;
        //}
    }
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Ground")
        {
            isHitGround = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        isHitGround = false;
    }
}
