//#define OFFLINE

using   System.Collections;
using   System.Collections.Generic;
using   UnityEngine;
using   MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/22
//! [内容]       エフェクト発生
//-----------------------------------------------------------------------------
public class MakeEffect : MonobitEngine.MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! private変数
    //-----------------------------------------------------------------------------
    private Rigidbody rigidbody;

    //-----------------------------------------------------------------------------
    //! public変数
    //-----------------------------------------------------------------------------

    //-----------------------------------------------------------------------------
    //! Inspectorに公開する変数
    //-----------------------------------------------------------------------------
    [Header("パーティクルサイズ")]                             public float particleSize;
    [Header("パーティクルを発生させるオブジェクトのスピード")] public float objectSpeed;

    //-----------------------------------------------------------------------------
    //! [内容]    更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {
        // 自分がホストじゃない場合は処理を行わない
        if (MonobitNetwork.isHost) {
            // リジッドボディを検索
            if (!rigidbody) {
                rigidbody = this.GetComponent<Rigidbody>();
            }
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    当たり判定
    //-----------------------------------------------------------------------------
    private void OnCollisionEnter(Collision collision)
    {
        if (rigidbody) {
            // スピードが指定数以上の場合は処理
            if (rigidbody.velocity.magnitude >= objectSpeed) {
                Bounds bounds = this.GetComponent<MeshFilter>().mesh.bounds;
#if !OFFLINE
                var gameObject = MonobitNetwork.Instantiate("Effect/Smoke", this.transform.position, Quaternion.identity, 0);
#else
                var prefab     = Resources.Load("Effect/Smoke") as GameObject;
                var gameObject = Instantiate(prefab, this.transform.position, Quaternion.identity);
#endif
                if (gameObject) {
                    // メッシュのサイズに合わせる
                    Vector3 size = new Vector3(
                                     bounds.size.x * transform.localScale.x
                                    ,bounds.size.y * transform.localScale.y
                                    ,bounds.size.z * transform.localScale.z
                                   );

                    float maxSize = Mathf.Max(size.x, size.y, size.z);

                    gameObject.transform.localScale = new Vector3(particleSize * maxSize, particleSize * maxSize, particleSize * maxSize);
                }
            }
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    カートに積み上げた時のエフェクトを再生
    //-----------------------------------------------------------------------------
    public void PlayAddEffect()
    {
#if !OFFLINE
        var gameObject = MonobitNetwork.Instantiate("Effect/NumAddDisplay", this.transform.position, Quaternion.identity, 0);
#else
        var prefab     = Resources.Load("Effect/NumAddDisplay") as GameObject;
        var gameObject = Instantiate(prefab, this.transform.position, Quaternion.identity);
#endif
        if (gameObject) {
            var scoreDisplay = gameObject.GetComponentInChildren<ScoreDisplay>();
            scoreDisplay.target = this.transform;
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    カートから離れた時のエフェクトを再生
    //-----------------------------------------------------------------------------
    public void PlaySubEffect()
    {
#if !OFFLINE
        var gameObject = MonobitNetwork.Instantiate("Effect/NumSubDisplay", this.transform.position, Quaternion.identity, 0);
#else
        var prefab     = Resources.Load("Effect/NumSubDisplay") as GameObject;
        var gameObject = Instantiate(prefab, this.transform.position, Quaternion.identity);
#endif
        if (gameObject) {
            var scoreDisplay = gameObject.GetComponentInChildren<ScoreDisplay>();
            scoreDisplay.target = this.transform;
        }
    }
}
