using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

public class ObjectRadar : MonobitEngine.MonoBehaviour
{
    public List<GameObject> throwObjects;
    //public List<GameObject> hitObjects;
    static MonobitView m_MonobitView = null;
    private void Start()
    {
        throwObjects = new List<GameObject>();
        //hitObjects = new List<GameObject>();
        m_MonobitView = GetComponent<MonobitView>();
    }

    private void Update()
    {
        //if (!m_MonobitView.isMine)
        //{
        //    return;
        //}

        //if (hitObjects != null)
        //{
        //    for (int i = 0; i < hitObjects.Count; i++)
        //    {
        //        if (throwObjects.Count == 0)
        //        {
        //            throwObjects.Add(hitObjects[i]);
                   
        //        }
        //        else
        //        {
        //            for (int k = 0; k < throwObjects.Count; k++)
        //            {
        //                if (throwObjects[k] != hitObjects[i])
        //                {
        //                    throwObjects.Add(hitObjects[i]);
        //                }
        //            }
        //        }
        //    }
        //}

    }
    void OnTriggerStay(Collider other)
    {
     
        if (other.transform.gameObject.tag == "ThrowObject")
        {
            //リスト内に同じオブジェクトがないとリストに追加する
            if (!throwObjects.Contains(other.gameObject))  
            {
                throwObjects.Add(other.gameObject);
            }
            //hitObjects.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (throwObjects != null)
        {
            throwObjects.Clear();
            //hitObjects.Clear();
        }
    }
}
