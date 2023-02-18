using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate( new Vector3(0,0,Time.deltaTime * 160));
        transform.localScale = new Vector3(transform.localScale.x + (Time.deltaTime/4),transform.localScale.y + (Time.deltaTime/4) ,1);
        if (transform.localScale.x > 0.5f) Destroy(gameObject);
    }
}
