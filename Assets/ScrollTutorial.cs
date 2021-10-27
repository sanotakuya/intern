using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollTutorial : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //!	private
    //-----------------------------------------------------------------------------
    [SerializeField] private NetworkManager networkManager = null;
    [SerializeField] private List<GameObject> textList = new List<GameObject>();
    [SerializeField] private List<float> posList = new List<float>();
    private Transform trans;
    private int progress = -1;
    private int cnt = 20;
    private int end = 20;
    private Vector3 startPos;

    private void Start()
    {
        trans = this.gameObject.transform;
        Init();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) && cnt == end)
        {
            textList[progress].SetActive(false);
            progress++;

            if (textList.Count <= progress)
            {
                networkManager.isTutorial = false;
                Init();
                trans.parent.gameObject.SetActive(false);
            }
            else
            {
                textList[progress].SetActive(true);
                startPos = trans.localPosition;
                cnt = 0;
            }
        }
    }

    private void FixedUpdate()
    {
        Vector3 pos = Vector3.Lerp(
            startPos, 
            new Vector3(posList[progress], trans.localPosition.y, trans.localPosition.z),
            (float)cnt/ end
            );

        trans.localPosition = pos;

        if(cnt < end)
        {
            cnt++;
        }

    }

    private void Init()
    {
        // 初期位置に戻す
        Vector3 pos = trans.localPosition;
        pos.x = posList[0];
        trans.localPosition = pos;

        //テキストのアクティブ状況を初期化
        foreach(GameObject text in textList)
        {
            text.SetActive(false);
        }
        textList[0].SetActive(true);

        progress = 0;

        startPos = transform.localPosition;
    }

}
