using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using v2 = UnityEngine.Vector2;
using v3 = UnityEngine.Vector3;

public class pendulum : MonoBehaviour
{
    v3 ddPos = v3.zero;
    v3 dPos = v3.zero;
    public float gravity = 9.81f;

    LineRenderer lineRenderer;
    public Transform pivot;
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.useWorldSpace = true;
    }

    v3 PendulumTangential = v3.zero;
    v3 PendulumNormal = v3.zero;
    float RopeLength = 0;
    // private void FixedUpdate() {
    //     float t = Time.fixedDeltaTime;
    //     List<Vector3> pos = new List<Vector3>();
    //     pos.Add(pivot.position);
    //     pos.Add(transform.position);
    //     lineRenderer.SetPositions(pos.ToArray());
    //     v3 Rope = (transform.position - pivot.position);
    //     if (RopeLength == 0)
    //     {
    //         RopeLength = Rope.magnitude;
    //     } 
        
    //     ddPos = v3.down*gravity;
    //     v3 Step = dPos*t + 0.5f*ddPos*t*t;
    //     dPos += ddPos*t;
    //     transform.position += Step;
    //     Rope = (transform.position - pivot.position);
    //     if (Rope.sqrMagnitude > RopeLength*RopeLength)
    //     {
    //         if (PendulumNormal == v3.zero) {
    //             PendulumNormal = (v3.Cross(Rope, v3.down)).normalized;
    //         }
    //         if (PendulumTangential == v3.zero)
    //         {
    //             PendulumTangential = (v3.Cross(v3.down, PendulumNormal)).normalized;
    //         }
    //         float FCentrifugal = (dPos.sqrMagnitude*(1.0f - Mathf.Pow(v3.Dot(Rope.normalized, v3.Cross(-Rope, PendulumNormal).normalized), 2))) / RopeLength;

    //         v3 T = (pivot.position - transform.position).normalized;
    //         T *= v3.Dot(v3.up*gravity, T) + FCentrifugal;
            
    //         ddPos = T;
    //         Step = dPos*t + 0.5f*ddPos*t*t;
    //         dPos += ddPos*t;
    //         transform.position += Step;
    //     }
        

    //     //v3 StartPoint = transform.position;
    //     //// Debug.DrawLine(transform.position, transform.position+T.normalized*FCentrifugal, Color.green, 10);
    //     //Debug.DrawLine(transform.position, transform.position+ddPos*0.1f, Color.yellow, 10);
    //     //Debug.DrawLine(transform.position, transform.position+T*0.1f, Color.blue, 10);
    //     Rope = (transform.position - pivot.position).normalized * RopeLength;
    //     transform.position = pivot.position + Rope;
    //     //
    //     //Debug.DrawLine(StartPoint, transform.position, Color.red, 10);
    //     //Debug.DrawLine(pivot.position, pivot.position + PendulumTangential, Color.red, 10);
    // }

    private void FixedUpdate() {
        float t = Time.fixedDeltaTime;
        List<Vector3> pos = new List<Vector3>();
        pos.Add(pivot.position);
        pos.Add(transform.position);
        lineRenderer.SetPositions(pos.ToArray());
        
        v3 Rope = (transform.position - pivot.position);
        if (RopeLength == 0)
        {
            RopeLength = Rope.magnitude;
        }
        if (PendulumNormal == v3.zero) {
            PendulumNormal = (v3.Cross(Rope, v3.down)).normalized;
        }
        if (PendulumTangential == v3.zero)
        {
            PendulumTangential = (v3.Cross(v3.down, PendulumNormal)).normalized;
        }

        float FCentrifugal = (dPos.sqrMagnitude*(1.0f - Mathf.Pow(v3.Dot(Rope.normalized, v3.Cross(-Rope, PendulumNormal).normalized), 2))) / RopeLength;
        
        v3 T = (pivot.position - transform.position).normalized;
        T *= v3.Dot(v3.up*gravity, T) + FCentrifugal;
        ddPos = v3.down*gravity + T;

        v3 StartPoint = transform.position;
        v3 Step = dPos*t + 0.5f*ddPos*t*t;
        // Debug.DrawLine(transform.position, transform.position+T.normalized*FCentrifugal, Color.green, 10);
        Debug.DrawLine(transform.position, transform.position+ddPos*0.1f, Color.yellow, 10);
        dPos += ddPos*t;
        Debug.DrawLine(transform.position, transform.position+dPos*0.1f, Color.blue, 10);
        transform.position += Step;
        if (v3.Distance(transform.position, pivot.position) > RopeLength)
        {
            Rope = (transform.position - pivot.position).normalized * RopeLength;
            transform.position = pivot.position + Rope;
        }
        
        Debug.DrawLine(StartPoint, transform.position, Color.red, 10);
        Debug.DrawLine(pivot.position, pivot.position + PendulumTangential, Color.red, 10);
    }
}
