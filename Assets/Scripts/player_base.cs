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
    float speed_kmph;
    
    private void Awake() {
        Friction = WalkingFriction;
        Controls = new game_controls();
        Controller = GetComponent<CharacterController>();
        PlayerAnimator = GetComponentInChildren<Animator>();

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
        Friction = Input.Run ? RunningFriction : WalkingFriction;
        if (Input.Jump && Controller.isGrounded)
        {
            dPos.y = JumpVelocity;
        }

        // ================================
        // Updating player
        // ================================
        //if (IsGrounded)
        //{
        //    solid_red.color = new Color(1, 1, 0, 1);
        //}
        //else
        //{
        //    solid_red.color = new Color(1, 0, 0, 1);
        //}

        float t = Time.deltaTime;

        Body.LookAt(Body.position + new Vector3(ddPos.x, 0, ddPos.z));
        if (ddPos.y > -Gravity)
        {
            ddPos.y -= Gravity*t;
        }
        if (ddPos.y < -Gravity)
        {
            ddPos.y = -Gravity;
        }
        v3 Acceleration = ddPos*WalkingThrust - dPos*Friction;
        Acceleration.y = ddPos.y;

        v3 Step = dPos*t + 0.5f*Acceleration*t*t;
        dPos += Acceleration*t;
        
        if (Mathf.Abs(dPos.x) < 0.01)
        {
            dPos.x = 0;
        }
        if (Mathf.Abs(dPos.z) < 0.01)
        {
            dPos.z = 0;
        }

        if ( false && Step.sqrMagnitude >= Controller.minMoveDistance*Controller.minMoveDistance)
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
            }
        }

        speed_kmph = dPos.magnitude / 3.6f;
        PlayerAnimator.SetFloat("Speed", (dPos.x*dPos.x + dPos.z*dPos.z));
    }

    private void OnEnable() {
        Controls.Enable();
    }

    private void OnDisable() {
        Controls.Disable();
    }
}
