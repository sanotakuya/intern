using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;
using TMPro;


public class StartText : MonobitEngine.MonoBehaviour
{
    [SerializeField] TextMeshProUGUI target;
    [SerializeField] GameObject gameUi;

    private void Update()
    {
        if(MonobitNetwork.isHost)
        {
            target.text = "Enterでゲーム開始\n(注意)途中参加はできません";
        }
        else
        {
            target.text = "ホストのゲーム開始まで待機中";

        }

        if(gameUi.active)
        {
            this.gameObject.SetActive(false);
        }
    }
}
