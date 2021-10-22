using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;
using TMPro;

public class DisplayPlayerName : MonobitEngine.MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //!	Private
    //-----------------------------------------------------------------------------
    [SerializeField] GameObject textPrefab;
    [SerializeField] Vector3 offset = new Vector3(0, 1, 0);

    static GameObject screenSpaceCanvas;

    RectTransform rectTrans = null;
    GameObject textObj = null;

    void Start()
    {
        // キャンバスを探す
        if(!screenSpaceCanvas)
        {
            screenSpaceCanvas = GameObject.Find("ScreenSpaceCanvas");
        }

        // 生成してキャンバスの子にする
        textObj = Instantiate(textPrefab);
        textObj.transform.parent = screenSpaceCanvas.transform;

        textObj.GetComponent<TextMeshProUGUI>().text = monobitView.owner.name;
        rectTrans = textObj.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        textObj.GetComponent<TextMeshProUGUI>().text = gameObject.name;
        rectTrans.position = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position + offset);
    }

    private void OnDestroy()
    {
        if(rectTrans)
        {
            Destroy(rectTrans.gameObject);
        }
    }
}
