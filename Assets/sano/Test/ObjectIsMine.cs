using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;
public class ObjectIsMine : MonobitEngine.MonoBehaviour
{
    static MonobitView m_MonobitView = null;
    // Start is called before the first frame update
    void Start()
    {
        m_MonobitView = GetComponent<MonobitView>();
    }

    // Update is called once per frame
    void Update()
    {
     
    }
   
    public void SetOwnership()
    {
        m_MonobitView.RequestOwnership();
    }


}
