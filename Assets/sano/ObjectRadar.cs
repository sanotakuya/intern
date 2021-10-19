﻿using System.Collections;
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

        // 親オブジェクトのMonobitViewを取得する
        if (GetComponentInParent<MonobitEngine.MonobitView>() != null)
        {
            m_MonobitView = GetComponentInParent<MonobitEngine.MonobitView>();
        }
       
    }

    private void Update()
    {
        if (!m_MonobitView.isMine)
        {
            return;
        }
        
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
