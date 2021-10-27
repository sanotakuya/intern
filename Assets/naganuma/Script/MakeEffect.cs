//#define OFFLINE

using   System.Collections;
using   System.Collections.Generic;
using   UnityEngine;
using   MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/26
//! [内容]       エフェクト発生
//-----------------------------------------------------------------------------
public class MakeEffect : MonobitEngine.MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! ENUM
    //-----------------------------------------------------------------------------
    enum CREATEOBJECTTYPE {
        FALL_EFFECT // 落下エフェクト
       ,ADD_EFFECT  // 加算エフェクト
       ,SUB_EFFECT  // 減算エフェクト
    }

    //-----------------------------------------------------------------------------
    //! private変数
    //-----------------------------------------------------------------------------
    private Rigidbody  rigidbody                 ;
    private bool       isHost     = true         ;
    private bool       deleteFlag = false        ;
    private bool       isHold     = false        ;
    private Vector3    hitPos     = new Vector3(); // 当たった場所
    private bool       isDestroy  = false        ;
    private float      countTime  = 0.0f         ; // 時間計測
    private HoldThrow  holdThrow                 ;

    //-----------------------------------------------------------------------------
    //! public変数
    //-----------------------------------------------------------------------------

    //-----------------------------------------------------------------------------
    //! Inspectorに公開する変数
    //-----------------------------------------------------------------------------
    [Header("落下パーティクル")]                               public GameObject   fallPrefab                 ;
    [Header("加算パーティクル")]                               public GameObject   addPrefab                  ;
    [Header("減算パーティクル")]                               public GameObject   subPrefab                  ;
    [Header("パーティクルサイズ")]                             public float        particleSize               ;
    [Header("パーティクルを発生させるオブジェクトのスピード")] public float        objectSpeed                ;
    [Header("プレイヤーが投げた後オブジェクトを削除するか")]   public bool         isThrewAfterDeletion = true;
    [Header("削除するまでのインターバル(秒)")]                 public float        destroyInterval      = 0.0f;
    [Header("地面のタグ")]                                     public string       groundTag                  ;

    //-----------------------------------------------------------------------------
    //! [内容]    RPC受信関数(生成情報)
    //-----------------------------------------------------------------------------
    [MunRPC]
    void RecvCreateEffect(int createObjectType, Vector3 point)
    {
        switch (createObjectType) {
            case (int)CREATEOBJECTTYPE.FALL_EFFECT:
                var effectObject = Instantiate(fallPrefab, point, Quaternion.identity);

                if (effectObject) {

                    Bounds bounds = this.GetComponent<MeshFilter>().mesh.bounds;

                    // メッシュのサイズに合わせる
                    Vector3 size = new Vector3(
                                     bounds.size.x * transform.localScale.x
                                    ,bounds.size.y * transform.localScale.y
                                    ,bounds.size.z * transform.localScale.z
                                   );

                    float maxSize = Mathf.Max(size.x, size.y, size.z);

                    effectObject.transform.localScale = new Vector3(particleSize * maxSize, particleSize * maxSize, particleSize * maxSize);
                    effectObject.GetComponent<ParticleSystem>().Play();
                }
                break;
            case (int)CREATEOBJECTTYPE.ADD_EFFECT:
                Instantiate(addPrefab, this.transform.position, Quaternion.identity);
                break;
            case (int)CREATEOBJECTTYPE.SUB_EFFECT:
                Instantiate(subPrefab, this.transform.position, Quaternion.identity);
                break;
            default:
                break;
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    更新処理
    //-----------------------------------------------------------------------------
    void Update()
    {
        if (MonobitNetwork.inRoom) {
            // 自分がホストじゃない場合は処理を行わない
            if (MonobitNetwork.isHost && isHost) {
                // リジッドボディを検索
                if (!rigidbody) {
                    rigidbody = this.GetComponent<Rigidbody>();
                }
            }
            else {
                isHost = false;
            }
        }

        if (holdThrow) {
            if (holdThrow.isHold) {
                isHold = true;
            }
            else {
                if (isHold) deleteFlag = true;
            }
            // オブジェクトが離された場合
            if (holdThrow.isRelease) {
                deleteFlag = false;
                isHold     = false;
            }
        }

        if (isDestroy) {
            countTime += Time.deltaTime;
            if (countTime >= destroyInterval) {
                if (!this.TryGetComponent<StackRoot>(out var stackRoot)) {
                    MonobitNetwork.Destroy(this.gameObject);
                    isDestroy = false;
                    countTime = 0.0f;
                }
            }
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    当たり判定
    //-----------------------------------------------------------------------------
    private void OnCollisionEnter(Collision collision)
    {
        if (rigidbody && MonobitNetwork.inRoom) {
            // スピードが指定数以上の場合は処理
            if (rigidbody.velocity.magnitude >= objectSpeed) {
#if !OFFLINE
                CreateEffect(CREATEOBJECTTYPE.FALL_EFFECT, collision.GetContact(0).point);
#else
                var prefab     = Resources.Load("Effect/Smoke") as GameObject;
                var gameObject = Instantiate(prefab, this.transform.position, Quaternion.identity);
#endif

                // 落ちた先が地面だった場合
                if (collision.gameObject.tag == groundTag && deleteFlag) {
                    isDestroy = true;
                    this.tag = "Untagged";
                }
            }
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    当たり判定
    //-----------------------------------------------------------------------------
    public void OnFindHoldObject(HoldThrow hold) {
        if (this.gameObject == hold.holdObject) {
            holdThrow = hold;
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    落下エフェクトを生成
    //-----------------------------------------------------------------------------
    void CreateEffect(CREATEOBJECTTYPE createObjectType, Vector3 point = new Vector3())
    {
        this.monobitView.RPC("RecvCreateEffect"
                            ,MonobitTargets.All
                            ,(int)createObjectType
                            ,point
                            );
    }

    //-----------------------------------------------------------------------------
    //! [内容]    カートに積み上げた時のエフェクトを再生
    //-----------------------------------------------------------------------------
    public void PlayAddEffect()
    {
#if !OFFLINE
        CreateEffect(CREATEOBJECTTYPE.ADD_EFFECT);
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
        CreateEffect(CREATEOBJECTTYPE.SUB_EFFECT);
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
