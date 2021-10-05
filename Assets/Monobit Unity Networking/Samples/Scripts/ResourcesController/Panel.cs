using UnityEngine;
using System.Collections;

public class Panel : MonoBehaviour {

	// Update is called once per frame
	void Update () {
        gameObject.transform.Rotate(0, 50.0f * Time.deltaTime, 0);
    }
}
