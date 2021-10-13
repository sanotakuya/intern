using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonobitEngine.MonoBehaviour
{
    public bool isHitGround;   //地面との当たり判定
    void OnTriggerStay(Collider other)
    {
        isHitGround = true;
    }
    void OnTriggerExit(Collider other)
    {
        isHitGround = false;
    }
}
