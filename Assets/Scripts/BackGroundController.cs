using UnityEngine;
using System.Collections;

public class BackGroundController : MonoBehaviour
{
    [SerializeField]
    float posInitialY;
    [SerializeField]
    float posLimitY;//下限
    float speed;
    Vector3 pos;

    // Use this for initialization
    void Start()
    {
        pos = transform.localPosition;
        speed = -0.02f;
    }

    // Update is called once per frame
    void Update()
    {
        pos.y += speed;
        if (pos.y <= posLimitY)
        {
            pos.y = posInitialY;
        }
        transform.localPosition = pos;
    }
}
