using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using v2 = UnityEngine.Vector2;
using v3 = UnityEngine.Vector3;

public class player_base : MonoBehaviour
{
    int LAYER_GROUND = 1 << 8;

    public CharacterController Controller;
    public camera Camera;
    public Transform Body;
    public Material solid_red;

    float Friction;
    bool IsGrounded;
    public float WalkingFriction = 10.0f;
    public float RunningFriction = 7.0f;
    public float Gravity = 9.81f;
    public float WalkingThrust = 120.0f;
    public float JumpVelocity = 10.0f;

    game_controls Controls;
    v2 Input;
    v3 ddPos;
    v3 dPos;
    float speed;
    
    private void Awake() {
        Friction = WalkingFriction;
        Controls = new game_controls();
        Controller = GetComponent<CharacterController>();

        Controls.InGame.Move.performed += ctx => SetForce(ctx.ReadValue<Vector2>());
        Controls.InGame.Move.canceled  += _ => {ddPos.x = 0; ddPos.z = 0;};

        Controls.InGame.Run.performed  += _ => Friction = RunningFriction;
        Controls.InGame.Run.canceled   += _ => Friction = WalkingFriction;

        Controls.InGame.Jump.performed += _ => { if (Controller.isGrounded) {dPos.y = JumpVelocity;}};
    }

    void SetForce(v2 Input)
    {
        v3 Right = Camera.transform.right;
        Right.y = 0;
        Right.Normalize();
        v3 Forward = Camera.transform.forward;
        Forward.y = 0;
        Forward.Normalize();
        v3 Direction = Input.x*Right + Input.y*Forward;
        ddPos.x = Direction.x;
        ddPos.z = Direction.z;
    }

    void Update()
    {
        if (IsGrounded)
        {
            solid_red.color = new Color(1, 1, 0, 1);
        }
        else
        {
            solid_red.color = new Color(1, 0, 0, 1);
        }

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

        if (Step.sqrMagnitude >= Controller.minMoveDistance*Controller.minMoveDistance)
        {
            Controller.Move(Step);
            if (Controller.isGrounded)
            {
                IsGrounded = true;
                dPos.y = 0;
            }
            else
            {
                IsGrounded = false;
            }
        }

        speed = dPos.magnitude / 3.6f;
    }

    private void OnEnable() {
        Controls.Enable();
    }

    private void OnDisable() {
        Controls.Disable();
    }
}
