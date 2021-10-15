using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

public class ObjectRadar : MonobitEngine.MonoBehaviour
{
    public List<GameObject> throwObjects;
    static MonobitView m_MonobitView = null;
    private void Start()
    {
        throwObjects = new List<GameObject>();
        m_MonobitView = GetComponent<MonobitView>();
    }

    private void Update()
    {
        //if (!m_MonobitView.isMine)
        //{
        //    return;
        //}
    }
    void OnTriggerStay(Collider other)
    {
        if (other.transform.gameObject.tag == "ThrowObject")
        {
            throwObjects.Add(other.gameObject);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (throwObjects != null)
        {
            throwObjects.Clear();
        }
    }
}
