using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/21
//! [内容]       カートの押せるエリアの判定
//-----------------------------------------------------------------------------
public class CartController : MonobitEngine.MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! private変数
    //-----------------------------------------------------------------------------
    private Rigidbody          rigidbody                               ; // リジッドボディ
    private List<CartPushArea> cartPushAreas = new List<CartPushArea>(); // 当たり判定

    //-----------------------------------------------------------------------------
    //! public変数
    //-----------------------------------------------------------------------------
    [Header("対象のタグ")]               public string           targetTag; // 対象タグ
    [Header("判定に使用するコライダー")] public List<GameObject> colliders; // コライダー

    //-----------------------------------------------------------------------------
    //! Inspectorに公開する変数
    //-----------------------------------------------------------------------------

    //-----------------------------------------------------------------------------
    //! [内容]    開始処理
    //-----------------------------------------------------------------------------
    void Start()
    {
        // ホストの場合コンポーネントを取得
        if (MonobitNetwork.isHost) {
            if (!TryGetComponent<Rigidbody>(out rigidbody)) Debug.LogError("リジッドボディが見つかりません。");
            // 回転無し & Z固定
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
            // メッシュコライダー削除
            if (TryGetComponent<MeshCollider>(out var colliderCom)) {
                Destroy(colliderCom);
            }
            // コライダーにコンポーネント追加
            foreach (var collider in colliders) {
                var cartPushArea = collider.AddComponent<CartPushArea>();
                if (!cartPushArea) Debug.LogError("コンポーネントを追加出来ませんでした。");
                cartPushArea.SetTargetTag(targetTag);
                cartPushAreas.Add(cartPushArea);
            }
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    更新処理
    //-----------------------------------------------------------------------------
    void LateUpdate()
    {
        if (MonobitNetwork.isHost) {
            foreach (var cartPushArea in cartPushAreas) {
                if (!cartPushArea.isHit) {
                    rigidbody.isKinematic = true;
                }
                else {
                    if (cartPushArea.rigidBody) {
                        if (cartPushArea.rigidBody.velocity == new Vector3(0.0f, 0.0f, 0.0f)) {
                            rigidbody.isKinematic = true;
                        }
                        else {
                            rigidbody.isKinematic = false;
                        }
                    }
                    else {
                        rigidbody.isKinematic = false;
                    }
                    break;
                }
            }
        }
    }
}
