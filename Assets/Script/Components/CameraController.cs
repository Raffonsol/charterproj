using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        pos.y += Input.mouseScrollDelta.y ;
        if (pos.y <= -(Map.Instance.reachedDistance/3f) && pos.y > -Map.Instance.reachedDistance - 3f)
        transform.position = pos;
    }
}//- +(Map.Instance.reachedDistance/2f)
