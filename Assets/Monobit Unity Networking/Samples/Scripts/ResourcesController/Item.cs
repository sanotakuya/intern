using UnityEngine;
using System.Collections;

namespace MonobitEngine.Sample
{
    public class Item : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            gameObject.transform.Rotate(0, 10.0f * Time.deltaTime, 0);
        }

        void OnDestroy()
        {
            if (Sample.ClientSide.RakeupGame.itemObject.Contains(gameObject))
            {
                Sample.ClientSide.RakeupGame.itemObject.Remove(gameObject);
            }
            if (Sample.ServerSide.RakeupGame.itemObject.Contains(gameObject))
            {
                Sample.ServerSide.RakeupGame.itemObject.Remove(gameObject);
            }
        }

        // プレハブがインスタンス化されたときのコールバック
        public void OnMonobitInstantiate(MonobitMessageInfo info)
        {
            ClientSide.RakeupGame.itemObject.Add(gameObject);
        }
    }
}
