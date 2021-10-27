using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------
//! [制作者]		小野龍大
//! [内容]		Titleでブロックを落とす
//-----------------------------------------------------------------------------
public class DropBlock : MonoBehaviour
{
    Transform trans;
    int cnt = 0;

    // Start is called before the first frame update
    void Start()
    {
        trans = this.gameObject.transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(cnt == 0)
        {
            trans.localRotation = Quaternion.Euler(new Vector3(0, 0, Random.value * 360.0f)) ;

            trans.localPosition = new Vector3(-900 + Random.value * 1800, Random.value * 300.0f + 800, 0);
        }
        else
        {
            Vector3 pos = trans.localPosition;
            pos.y -= 10;
            trans.localPosition = pos;
        }

        cnt++;

        if (trans.localPosition.y < -800)
        {
            cnt = 0;
        }
    }
}
