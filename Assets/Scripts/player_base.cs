using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using v2 = UnityEngine.Vector2;
using v3 = UnityEngine.Vector3;

public class player_base : MonoBehaviour
{
    public CharacterController Controller;
    public camera Camera;
    public Transform Body;
    public Material solid_red;
    Animator PlayerAnimator;
    input Input;

    float Friction;
    bool IsGrounded;
    public float WalkingFriction = 10.0f;
    public float RunningFriction = 7.0f;
    public float Gravity = 9.81f;
    public float WalkingThrust = 120.0f;
    public float JumpVelocity = 10.0f;

    game_controls Controls;
    v3 ddPos;
    v3 dPos;
    bool IsSwinging;
    float speed_kmph;
    
    LineRenderer lineRenderer;
    private void Awake() {
        Friction = WalkingFriction;
        Controls = new game_controls();
        Controller = GetComponent<CharacterController>();
        PlayerAnimator = GetComponentInChildren<Animator>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.useWorldSpace = true;

        Controls.InGame.Move.performed += ctx => Input.Move = ctx.ReadValue<v2>();
        Controls.InGame.Move.canceled  += _ => Input.Move = v2.zero;

        Controls.InGame.Run.performed  += _ => Input.Run = true;
        Controls.InGame.Run.canceled   += _ => Input.Run = false;

        Controls.InGame.Jump.performed += _ => Input.Jump = true;
        Controls.InGame.Jump.canceled  += _ => Input.Jump = false;
    }

    void SetForce(v2 Dir)
    {
        if (Dir == v2.zero)
        {
            ddPos.x = 0;
            ddPos.z = 0;
        }
        else
        {
            v3 Right = Camera.transform.right;
            Right.y = 0;
            Right.Normalize();
            v3 Forward = Camera.transform.forward;
            Forward.y = 0;
            Forward.Normalize();
            v3 Force = Dir.x*Right + Dir.y*Forward;
            ddPos.x = Force.x;
            ddPos.z = Force.z;
        }
    }

    void Update()
    {
        // ================================
        // Processing input
        // ================================
        SetForce(Input.Move);
        if (Input.Run) {
            if (IsGrounded) {
                Friction = RunningFriction;
            }
            else {
                IsSwinging = true;
            }
        }
        else {
            if (IsSwinging) {
                ddPos.y = -Gravity;
            }
            Friction = WalkingFriction;
            IsSwinging = false;
            PendulumTangential = v3.zero;
            PendulumNormal = v3.zero;
            RopeLength = 0;
            SwingAnchor = v3.zero;
        }
        if (Input.Jump && IsGrounded)
        {
            dPos.y = JumpVelocity;
        }

        float t = Time.deltaTime;

        Body.LookAt(Body.position + new Vector3(ddPos.x, 0, ddPos.z));
        v3 Step = v3.zero;
        if (IsSwinging)
        {
            Step += Swing(SwingAnchor, t);
        }
        else
        {
            Step += Walk(t);
        }

        if (Step.sqrMagnitude >= Controller.minMoveDistance * Controller.minMoveDistance)
        {
            Controller.Move(Step);
            if (Controller.isGrounded)
            {
                IsGrounded = true;
                dPos.y = -Gravity;
            }
            else
            {
                IsGrounded = false;
                if (IsSwinging) {
                    if (v3.Distance(transform.position, SwingAnchor) > RopeLength)
                    {
                        v3 Rope = (transform.position - SwingAnchor).normalized * RopeLength;
                        transform.position = SwingAnchor + Rope;
                    }
                }
            }
        }

        speed_kmph = dPos.magnitude / 3.6f;
        PlayerAnimator.SetFloat("Speed", (dPos.x * dPos.x + dPos.z * dPos.z));
    }

    private v3 Walk(float dt)
    {
        if (ddPos.y > -Gravity)
        {
            ddPos.y -= Gravity * dt;
        }
        if (ddPos.y < -Gravity)
        {
            ddPos.y = -Gravity;
        }
        v3 Acceleration = ddPos * WalkingThrust - dPos * Friction;
        Acceleration.y = ddPos.y;

        v3 Step = dPos * dt + 0.5f * Acceleration * dt * dt;
        dPos += Acceleration * dt;

        if (Mathf.Abs(dPos.x) < 0.01)
        {
            dPos.x = 0;
        }
        if (Mathf.Abs(dPos.z) < 0.01)
        {
            dPos.z = 0;
        }

        return Step;
    }

    v3 PendulumTangential = v3.zero;
    v3 PendulumNormal = v3.zero;
    float RopeLength = 0;
    v3 SwingAnchor = v3.zero;
    private v3 Swing(v3 Pivot, float dt) {
        if (Pivot == v3.zero) {
            SwingAnchor = transform.position + Body.forward*10.0f + Body.up*15.0f;
            Pivot = SwingAnchor;
        }
        List<Vector3> pos = new List<Vector3>();
        pos.Add(Pivot);
        pos.Add(transform.position);
        lineRenderer.SetPositions(pos.ToArray());
        
        v3 Rope = (transform.position - Pivot);
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
        
        v3 T = (Pivot - transform.position).normalized;
        T *= v3.Dot(v3.up*Gravity, T) + FCentrifugal;
        ddPos = v3.down*Gravity + T;

        v3 StartPoint = transform.position;
        v3 Step = dPos*dt + 0.5f*ddPos*dt*dt;
        dPos += ddPos*dt;

        return Step;
    }

    private void OnEnable() {
        Controls.Enable();
    }

    private void OnDisable() {
        Controls.Disable();
    }
}
