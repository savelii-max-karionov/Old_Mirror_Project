using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    [SerializeField]
    private WheelCollider wc;
    private Transform t;
    // Start is called before the first frame update
    void Start()
    {
        t = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        var pos = Vector3.zero;
        var rot = Quaternion.identity;
        wc.GetWorldPose(out pos, out rot);
        t.position = pos;
        t.rotation = rot;
    }

    private void FixedUpdate()
    {
        
    }
}
