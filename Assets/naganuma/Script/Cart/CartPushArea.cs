using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/21
//! [内容]       カートの押せるエリアの当たり判定
//-----------------------------------------------------------------------------
public class CartPushArea : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! private変数
    //-----------------------------------------------------------------------------
    private bool      _isHit     = false; // 当たったか
    private string    targetTag         ; // 当たり判定の対象タグ
    private Rigidbody _rigidBody        ; // リジッドボディ

    //-----------------------------------------------------------------------------
    //! public変数
    //-----------------------------------------------------------------------------
    public bool isHit {          // 当たったか
        get { return _isHit; }
    }

    public Rigidbody rigidBody { // 当たったオブジェクトのリジッドボディ
        get { return _rigidBody; }
    }


    //-----------------------------------------------------------------------------
    //! [内容]    当たり判定を取る対象のタグ
    //-----------------------------------------------------------------------------
    public void SetTargetTag(string tagName)
    {
        targetTag = tagName;
    }

    //-----------------------------------------------------------------------------
    //! [内容]    当たり判定
    //-----------------------------------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == targetTag) {
            _isHit = true;
            var rb = other.attachedRigidbody;
            if (rb) {
                _rigidBody = rb;
            }
            else {
                _rigidBody = null;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == targetTag) {
            _isHit     = false;
            _rigidBody = null;
        }
    }
}
