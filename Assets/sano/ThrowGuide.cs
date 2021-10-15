using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowGuide : MonoBehaviour
{
    /// ガイドの開始
    public bool isDrawGuide = false;
    public bool isNowGuide = false;

    [SerializeField, Header("ガイドの起点となるオブジェクト")]
    private GameObject guideRoot;

    /// 放物線のMaterial
    [SerializeField, Header("ガイド用投擲オブジェクト")]
    private GameObject guidePrefab;
    private GameObject guideInstance;
    private Rigidbody guideRb; 　//ガイドオブジェクトのリジッドボディ

    private float guideCnt;

    // オブジェクと投げる用のスクリプト参照
    HoldThrow holdThrow;

    // 現在計算されてるオブジェクトを飛ばす方向
    Vector3 foeceDirection;　
    static float holdMass;

    /// 着弾マーカーオブジェクトのPrefab
    [SerializeField, Tooltip("着弾地点に表示するマーカー（もしつけるなら）")]
    private GameObject pointerPrefab;

    /// 着弾点のマーカーのオブジェクト
    private GameObject pointerObject;


    void Start()
    {
        //// 放物線のLineRendererオブジェクトを用意
        //CreateLineRendererObjects();

        // マーカーのオブジェクトを用意
        pointerObject = Instantiate(pointerPrefab, Vector3.zero, Quaternion.identity);
        pointerObject.SetActive(false);

        // 弾の初速度や生成座標を持つコンポーネント
        holdThrow = gameObject.GetComponent<HoldThrow>();
    }

    void Update()
    {
        // 初速度と放物線の開始座標を更新
        foeceDirection = holdThrow.GetThrowForce();

        if (isDrawGuide == true && isNowGuide == false)
        {
            Debug.Log("ガイド表示");
            guideInstance = Instantiate(guidePrefab, guideRoot.transform.position, Quaternion.identity);

            guideRb = guideInstance.GetComponent<Rigidbody>();

            guideRb.mass = holdMass;

            guideRb.AddForce(foeceDirection, ForceMode.Impulse);
            isNowGuide = true;
        }
    }

    private void FixedUpdate()
    {
        if (isNowGuide == true)
        {
            guideCnt += 1;
            if (guideCnt >= 5)
            {
                guideCnt = 0;
                isNowGuide = false;
            }
        }
    }
    //-----------------------------------------------------------------------------
    //! [内容]		 外部参照　掴んでるオブジェクトの重さを取得する
    //-----------------------------------------------------------------------------
    public void SetObjectMass(float num)
    {
        holdMass = num;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.gameObject != holdThrow.holdObject)
        {
            pointerObject.transform.position = other.transform.position;
            pointerObject.SetActive(true);
            Debug.Log("Hit" + other.transform.gameObject);
        }
    }

}
