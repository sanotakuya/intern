using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SphereBooster : MonoBehaviour
{

    // DistanceTextオブジェクトへの参照
    [SerializeField]
    GameObject distanceTextObject;

    // HighScoreTextオブジェクトへの参照
    [SerializeField]
    GameObject highScoreTextObject;

    // PowerMeterオブジェクトへの参照
    [SerializeField]
    GameObject powerMeterObject;

    // 角度を表す矢印オブジェクト
    [SerializeField]
    GameObject angleArrowObject;

    // 角度変更ボタン(上)
    [SerializeField]
    GameObject angleArrowUpButtonObject;

    // 角度変更ボタン(下)
    [SerializeField]
    GameObject angleArrowDownButtonObject;

    // GuideParentオブジェクトへの参照
    [SerializeField]
    GameObject guideManagerObject;

    // 加える力の大きさ
    float forceMagnitude = 0f;

    // X軸からの角度
    float forceAngle = 45.0f;

    // 力を加える方向
    Vector3 forceDirection = new Vector3(1.0f, 1.0f, 0f);

    // 飛行中フラグ
    bool isFlying = false;

    // ボタン押下フラグ
    bool isBoostPressed = false;

    // 距離測定中フラグ
    bool isCheckingDistance = false;

    // Sphereオブジェクトの初期位置格納用ベクトル
    Vector3 initPosition = Vector3.zero;

    // Sphereオブジェクトの停止位置格納用ベクトル
    Vector3 stopPosition = Vector3.zero;

    // Rigidbodyコンポーネントへの参照をキャッシュ
    Rigidbody rb;

    // DistanceTextオブジェクトのTextコンポーネントへの参照をキャッシュ
    Text distanceText;

    // UIテキストのプレフィックス
    string distancePrefix = "Distance : ";

    // UIテキストのサフィックス
    string distanceSuffix = " m";

    // HighScoreTextオブジェクトのTextコンポーネントへの参照をキャッシュ
    Text highScoreText;

    // ハイスコア表示のプレフィックス
    string highScorePrefix = "High Score : ";

    // ハイスコア表示のサフィックス
    string highScoreSuffix = " m";

    // ハイスコアの距離
    float highScoreDistance = 0f;

    // 落下判定用オブジェクトのタグ
    string fallCheckerTag = "FallChecker";

    // Sliderコンポーネントへの参照
    Slider powerMeterSlider;

    // メーターの速さ
    [SerializeField]
    float meterSpeed = 0.2f;

    // メーターが最大値になった時のディレイ
    [SerializeField]
    float delayTime = 0.08f;
    float waitTime = 0f;

    // メーターが増加中か減少中か(trueで増加中)
    bool isMeterIncreasing = true;

    // 角度増加中か減少中か
    const int AngleIncreasing = 1;
    const int AngleDecreasing = -1;

    // 角度の増分
    int angleDelta;

    // 矢印オブジェクトのRect Transformへの参照のキャッシュ
    RectTransform arrowRt;

    // 角度の上限と下限の定義
    const float MaxAngle = 90f;
    const float MinAngle = 0f;

    // Buttonコンポーネントへの参照のキャッシュ
    Button angleUpButton;
    Button angleDownButton;

    // GuideManagerへの参照のキャッシュ
    //GuideManager guideManager;

    void Start()
    {
        initPosition = gameObject.transform.position;
        rb = gameObject.GetComponent<Rigidbody>();
        distanceText = distanceTextObject.GetComponent<Text>();
        highScoreText = highScoreTextObject.GetComponent<Text>();
        powerMeterSlider = powerMeterObject.GetComponent<Slider>();

        arrowRt = angleArrowObject.GetComponent<RectTransform>();
        angleUpButton = angleArrowUpButtonObject.GetComponent<Button>();
        angleDownButton = angleArrowDownButtonObject.GetComponent<Button>();
        //guideManager = guideManagerObject.GetComponent<GuideManager>();

        // DistanceTextとHighScoreTextの初期値をセット
        SetDistanceText(0f);
        SetHighScoreText(0f);
    }

    void Update()
    {
        // キーボードからの入力を監視
        CheckInput();

        // forceAngleの変更を反映する
        CalcForceDirection();

        // powerMeterを動かす
        MovePowerMeter();

        // 角度を変更する
        ChangeForceAngle();
        ChangeArrowAngle();
    }

    void FixedUpdate()
    {
        // 距離の測定
        CheckDistance();

        if (!isBoostPressed)
        {
            // キーまたはボタンが押されていなければ
            // 処理の切り替えをせず抜ける
            return;
        }
        if (isFlying)
        {
            // 飛行中の処理
            StopFlying();
        }
        else
        {
            // ボールを飛ばす処理
            BoostSphere();
        }
        // 飛行中フラグの切り替え
        isFlying = !isFlying;

        // どちらの処理をしてもボタン押下フラグをfalseに
        isBoostPressed = false;
    }

    void StopFlying()
    {
        // 運動の停止
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 初期位置に移動させる
        gameObject.transform.position = initPosition;

        // 距離測定中フラグをFalseにセット
        isCheckingDistance = false;

        // 現在の距離をリセット
        SetDistanceText(0f);

        // 角度矢印を表示する
        angleArrowObject.SetActive(true);

        // ボタンを押せるようにする
        SetAngleButtonState(true);

        // ガイドを表示する
        //guideManager.SetGuidesState(true);
    }

    void BoostSphere()
    {
        // 向きと力の計算
        Vector3 force = forceMagnitude * forceDirection;

        // 力を加えるメソッド
        rb.AddForce(force, ForceMode.Impulse);

        // 距離測定中フラグをTrueにセット
        isCheckingDistance = true;

        // 角度矢印を非表示にする
        angleArrowObject.SetActive(false);

        // ボタンを押せないようにする
        SetAngleButtonState(false);

        // ガイドを非表示にする
        //guideManager.SetGuidesState(false);
    }

    public void OnPressedBoostButton()
    {
        isBoostPressed = true;
    }

    void CalcForceDirection()
    {
        // 入力された角度をラジアンに変換
        float rad = forceAngle * Mathf.Deg2Rad;

        // それぞれの軸の成分を計算
        float x = Mathf.Cos(rad);
        float y = Mathf.Sin(rad);
        float z = 0f;

        // Vector3型に格納
        forceDirection = new Vector3(x, y, z);
    }

    void CheckDistance()
    {
        if (!isCheckingDistance)
        {
            // 距離測定中でなければ何もしない
            return;
        }

        // 現在位置までの距離を計算する
        Vector3 currentPosition = gameObject.transform.position;
        float distance = GetDistanceInXZ(initPosition, currentPosition);

        // UIテキストに表示
        SetDistanceText(distance);

        if (rb.IsSleeping())
        {
            // ハイスコアのチェック
            stopPosition = currentPosition;
            float currentDistance = GetDistanceInXZ(initPosition, stopPosition);

            if (currentDistance > highScoreDistance)
            {
                highScoreDistance = currentDistance;
            }
            SetHighScoreText(highScoreDistance);

            // 距離測定中フラグをオフに
            isCheckingDistance = false;
        }
    }

    float GetDistanceInXZ(Vector3 startPos, Vector3 stopPos)
    {
        // 開始位置、停止位置のそれぞれで、Y軸成分を除いて計算用Vector3を作成
        Vector3 startPosCalc = new Vector3(startPos.x, 0f, startPos.z);
        Vector3 stopPosCalc = new Vector3(stopPos.x, 0f, stopPos.z);

        // 2つのVector3データから距離を算出
        float distance = Vector3.Distance(startPosCalc, stopPosCalc);
        return distance;
    }

    void SetDistanceText(float distance)
    {
        // 受け取った距離の値を使って画面に表示するテキストをセット
        distanceText.text = distancePrefix + distance.ToString("F2") + distanceSuffix;
    }

    void SetHighScoreText(float distance)
    {
        // 受け取ったハイスコアの値を使って画面に表示するテキストをセット
        highScoreText.text = highScorePrefix + distance.ToString("F2") + highScoreSuffix;
    }

    void OnTriggerEnter(Collider other)
    {
        // 衝突した相手のタグを確認する
        if (other.gameObject.tag == fallCheckerTag)
        {
            // 相手が落下判定用オブジェクトだった時の処理
            isBoostPressed = true;
        }
    }

    public Vector3 GetBoostForce()
    {
        // 力の大きさと方向をかけたベクトルを返す
        CalcForceDirection();

        // 外部のスクリプトから呼ばれた時に、スライダーの参照がなければセットする
        if (powerMeterSlider == null)
        {
            powerMeterSlider = powerMeterObject.GetComponent<Slider>();
        }
        return powerMeterSlider.maxValue * forceDirection;
    }

    void MovePowerMeter()
    {
        // 飛行中フラグがfalseの時にメーターを上下させる
        if (isFlying)
        {
            return;
        }

        // 境界値の定義
        float boundaryValue = 0f;

        // forceMagnitudeにmeterSpeedの値を加えていってメーターを上下させる
        if (isMeterIncreasing)
        {
            powerMeterSlider.value += meterSpeed;
            boundaryValue = powerMeterSlider.maxValue;
        }
        else
        {
            powerMeterSlider.value -= meterSpeed;
            boundaryValue = powerMeterSlider.minValue;
        }

        // 境界値になったら少し止めた後にメーターを逆向きに動かす
        if (Mathf.Approximately(powerMeterSlider.value, boundaryValue))
        {
            WaitAtBoundaryValue();
        }

        // スライダーの現在値をforceMagnitudeに格納
        forceMagnitude = powerMeterSlider.value;
    }

    void WaitAtBoundaryValue()
    {
        // 前のフレームが呼ばれて、処理が完了するまでにかかった時間を加算
        waitTime += Time.deltaTime;

        // waitTimeがdelayTimeを超えたら増加フラグの反転
        if (waitTime >= delayTime)
        {
            isMeterIncreasing = !isMeterIncreasing;
            waitTime = 0f;
        }
    }

    public void OnPressedAngleUpButton()
    {
        // 飛行中は角度変更ボタンを動作させないようにする
        if (!isFlying)
        {
            // 角度上昇ボタンが押された時の処理
            angleDelta = AngleIncreasing;
        }
    }

    public void OnPressedAngleDownButton()
    {
        // 飛行中は角度変更ボタンを動作させないようにする
        if (!isFlying)
        {
            // 角度下降ボタンが押された時の処理
            angleDelta = AngleDecreasing;
        }
    }

    public void OnReleasedAngleButton()
    {
        // 飛行中は角度変更ボタンを動作させないようにする
        if (!isFlying)
        {
            // 角度の増分を0にする
            angleDelta = 0;
        }
    }

    void ChangeForceAngle()
    {
        // 角度の増分が0なら、以降の処理を行わない
        if (angleDelta == 0)
        {
            return;
        }
        // forceAngleに角度の増分をfloat型として加算
        forceAngle += angleDelta * 1.0f;

        if (angleDelta == AngleIncreasing)
        {
            // forceAngleが最大値を超えた場合には最大値をセット
            if (forceAngle >= MaxAngle)
            {
                forceAngle = MaxAngle;
            }
        }
        else if (angleDelta == AngleDecreasing)
        {
            // forceAngleが最小値より小さい場合には最小値をセット
            if (forceAngle <= MinAngle)
            {
                forceAngle = MinAngle;
            }
        }
    }

    void ChangeArrowAngle()
    {
        // 角度の矢印をボールの位置に移動
        arrowRt.position = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position);

        // Z軸を中心に、forceAngleの値の回転を作って角度の矢印にセット
        arrowRt.rotation = Quaternion.AngleAxis(forceAngle, Vector3.forward);

        // GuideManagerのSetGuidePositions()を呼んでガイドを移動させる
        if (!isFlying)
        {
            //guideManager.SetGuidePositions();
        }
    }

    void SetAngleButtonState(bool isInteractable)
    {
        // ボタンが押せるかどうかの状態をセットする
        angleUpButton.interactable = isInteractable;
        angleDownButton.interactable = isInteractable;
    }

    void CheckInput()
    {
        // キーが押された時の動きを定義

        // Input.GetKeyUpはキーが一度押された後、それが離された時にTrueを返す
        if (Input.GetKeyUp(KeyCode.B))
        {
            isBoostPressed = true;
        }

        // 上キーが押された時は、角度変更ボタン(上)が押された時の処理を呼ぶ
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            OnPressedAngleUpButton();
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            OnReleasedAngleButton();
        }

        // 下キーが押された時は、角度変更ボタン(下)が押された時の処理を呼ぶ
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            OnPressedAngleDownButton();
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            OnReleasedAngleButton();
        }
    }
}
