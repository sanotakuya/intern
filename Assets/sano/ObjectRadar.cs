using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRadar : MonobitEngine.MonoBehaviour
{
    public List<GameObject> throwObjects;

    private void Start()
    {
        throwObjects = new List<GameObject>();
    }

    private void Update()
    {
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
