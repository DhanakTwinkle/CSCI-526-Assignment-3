using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{

    public Transform target;
    public float lag = 50.0f;

    Vector3 offset;

    void Start()
    {
        offset = target.position - transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mean = Grid.instance.mean();
        Vector3 newTargetPos = target.position + new Vector3(mean.x, mean.y, 0.0f);
        transform.position = Vector3.Lerp(transform.position, newTargetPos - offset, lag * Time.deltaTime);
    }
}
