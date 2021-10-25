
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideManager : MonoBehaviour
{

    // 『Sphere』オブジェクトへの参照
    [SerializeField]
    GameObject player;

    // SphereBoosterへの参照をキャッシュ
    SphereBooster sphereBooster;

    // 『Sphere』オブジェクトのRigidbodyへの参照をキャッシュ
    Rigidbody sphereRb;

    // インスタンス化されたGuideオブジェクトのリスト
    List<GameObject> guideList;

    // 画面にプロットするガイドの数を定義
    int prots = 8;

    void Start()
    {
        sphereBooster = player.GetComponent<SphereBooster>();
        sphereRb = player.GetComponent<Rigidbody>();
        guideList = new List<GameObject>();

        // Prefabをインスタンス化するメソッドを呼ぶ
        InstantiateGuidePrefabs();
        SetGuidePositions();
    }

    void Update()
    {

    }

    void FixedUpdate()
    {

    }

    void InstantiateGuidePrefabs()
    {
        // 『GuideParent』の位置を『Sphere』オブジェクトの位置へ移動
        gameObject.transform.position = player.transform.position;

        // 『GuideParent』の位置をguidePosにセット
        Vector3 guidePos = gameObject.transform.position;

        // Prefabをロードする
        GameObject guidePrefab = (GameObject)Resources.Load("Prefabs/Guide");

        for (int i = 0; i < prots; i++)
        {
            // Prefabをインスタンス化
            GameObject guideObject = (GameObject)Instantiate(guidePrefab, guidePos, Quaternion.identity);

            // インスタンス化したオブジェクトをGuideParentの子オブジェクトにする
            guideObject.transform.SetParent(gameObject.transform);

            // オブジェクト名を設定する
            guideObject.name = "Guide_" + i.ToString();

            // リストへ追加
            guideList.Add(guideObject);
        }
    }

    void SetGuidePositions()
    {
        // 『GuideParent』の位置を『Sphere』オブジェクトの位置へ移動
        gameObject.transform.position = player.transform.position;

        // 『GuideParent』の位置を開始位置に設定
        Vector3 startPos = gameObject.transform.position;

        // リストの検証
        if (guideList == null || guideList.Count == 0)
        {
            return;
        }

        // 物理学的なパラメータを取得
        // 『Sphere』オブジェクトに加わる力
        Vector3 force = sphereBooster.GetBoostForce();

        // 『Sphere』オブジェクトの質量
        float mass = sphereRb.mass;

        // Unityの世界に働く重力
        Vector3 gravity = Physics.gravity;

        // 『Sphere』オブジェクトが斜方投射される時の初速度
        Vector3 speed = force / mass;

        // プロット数に応じて、各プロットの時刻をリストに格納
        List<float> timeProtsList = GetTimeProtsList(speed, gravity, prots);

        // リストの検証
        if (timeProtsList == null || timeProtsList.Count == 0)
        {
            return;
        }

        // 時刻リストを元に、プロットするガイドの位置を設定
        for (int i = 0; i < prots; i++)
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
}