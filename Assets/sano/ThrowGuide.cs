using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;
public class ThrowGuide : MonobitEngine.MonoBehaviour
{
    MonobitView m_MonobitView = null;

    //　ガイド開始
    public bool isGuideStart;

    // 『Sphere』オブジェクトへの参照
    [SerializeField]
    GameObject guidePrent;

    // ガイド表示用オブジェクト
    [SerializeField]
    GameObject guidePrefab;

    // SphereBoosterへの参照をキャッシュ
    HoldThrow holdThrow;

    // 『Sphere』オブジェクトのRigidbodyへの参照をキャッシュ
    Rigidbody sphereRb;

    // インスタンス化されたGuideオブジェクトのリスト
    List<GameObject> guideList;
    
    //投げるオブジェクトの重さ
    static float holdMass;

    // 画面にプロットするガイドの数を定義
    public int protSize;

    void Start()
    {
        holdThrow = this.GetComponent<HoldThrow>();
        sphereRb = this.GetComponent<Rigidbody>();
        guideList = new List<GameObject>();

        // 親オブジェクトのMonobitViewを取得する
        if (GetComponentInParent<MonobitEngine.MonobitView>() != null)
        {
            m_MonobitView = GetComponentInParent<MonobitEngine.MonobitView>();
        }

        // Prefabをインスタンス化するメソッドを呼ぶ
        InstantiateGuidePrefabs();
    }

    void Update()
    {
        if (!m_MonobitView.isMine)
        {
            return;
        }
        if (isGuideStart == true)
        {
            SetGuidePositions();

            guidePrent.SetActive(true);
        }
        else
        {
            // ガイドの位置をリセット
            for (int i = 0; i < protSize; i++)
            {
                guideList[i].transform.position = guidePrent.transform.position;
                guidePrent.SetActive(false);
            }
        }
    }

    void FixedUpdate()
    {

    }

    void InstantiateGuidePrefabs()
    {
        // 『GuideParent』の位置をguidePosにセット
        Vector3 guidePos = guidePrent.transform.position;

        for (int i = 0; i < protSize; i++)
        {
            // Prefabをインスタンス化
            GameObject guideObject = (GameObject)Instantiate(guidePrefab, guidePos, Quaternion.identity);

            // インスタンス化したオブジェクトをGuideParentの子オブジェクトにする
            guideObject.transform.SetParent(guidePrent.transform);

            // オブジェクト名を設定する
            guideObject.name = "Guide_" + i.ToString();

            // リストへ追加
            guideList.Add(guideObject);
        }
    }

    void SetGuidePositions()
    {
        // 『GuideParent』の位置を開始位置に設定
        Vector3 startPos = guidePrent.transform.position;

        // リストの検証
        if (guideList == null || guideList.Count == 0)
        {
            return;
        }

        // 物理学的なパラメータを取得
        // 『Sphere』オブジェクトに加わる力
        Vector3 force = holdThrow.GetThrowForce();
    
        // Unityの世界に働く重力
        Vector3 gravity = Physics.gravity;

        // 『Sphere』オブジェクトが斜方投射される時の初速度
        Vector3 speed = force / holdMass;

        // プロット数に応じて、各プロットの時刻をリストに格納
        List<float> timeProtsList = GetTimeProtsList(speed, gravity, protSize);

        // リストの検証
        if (timeProtsList == null || timeProtsList.Count == 0)
        {
            return;
        }

        // 時刻リストを元に、プロットするガイドの位置を設定
        for (int i = 0; i < protSize; i++)
        {
            // リストから時刻の値を取り出す
            float time = timeProtsList[i];

            // リストで対応するインデックスのガイドオブジェクトについて位置を設定
            guideList[i].transform.position = GetExpectedPosition(startPos, speed, gravity, time);
        }
    }

    List<float> GetTimeProtsList(Vector3 speed, Vector3 gravity, int prots)
    {
        // 斜方投射後、地面に到達する時刻を計算
        float landingTime = -2.0f * speed.y / gravity.y;

        // 時刻格納用のリストを作成
        List<float> timeProtsList = new List<float>();

        // ガイドのプロット数が0なら作成直後の長さ0のリストを返す
        if (prots <= 0)
        {
            return timeProtsList;
        }

        // プロット数に応じて、ガイドを表示する位置を計算するための時刻をリストに追加
        for (int i = 1; i <= prots; i++)
        {
            float timeProt = i * landingTime / prots;
            timeProtsList.Add(timeProt);
        }
        return timeProtsList;
    }

    Vector3 GetExpectedPosition(Vector3 startPos, Vector3 speed, Vector3 gravity, float time)
    {
        // 時刻を元に、ガイドの位置を計算する
        Vector3 position = (speed * time) + (gravity * 0.5f * Mathf.Pow(time, 2));
        Vector3 guidePos = startPos + position;
        return guidePos;
    }

    //-----------------------------------------------------------------------------
    //! [内容]		 外部参照　掴んでるオブジェクトの重さを取得する
    //-----------------------------------------------------------------------------
    public void SetObjectMass(float num)
    {
        holdMass = num;
    }

    public void SetGuidesState(bool isStart)
    {
        isGuideStart = isStart;
    }
}
