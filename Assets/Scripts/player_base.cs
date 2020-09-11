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

    public float Acceleration = 120.0f;
    float Friction;
    public float WalkingFriction = 10.0f;
    public float RunningFriction = 7.0f;
    public float Gravity = 26f;

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
        Controls.InGame.Move.canceled  += _ => ddPos = Vector2.zero;

        Controls.InGame.Run.performed += _ => Friction = RunningFriction;
        Controls.InGame.Run.canceled  += _ => Friction = WalkingFriction;

        //Controls.InGame.Jump.performed += _ => { ForceDir.y = Controller.isGrounded ? JumpForce : 0; };
        //Controls.InGame.Jump.canceled  += _ => ForceDir.y = 0;
        Controls.InGame.Jump.performed += _ => {dPos = v2.zero; ddPos = v2.zero;};
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
        v3 dd = ddPos*Acceleration - dPos*Friction;
        Body.LookAt(Body.position + new Vector3(dd.x, 0, dd.z));
        

        float t = Time.deltaTime;

        v3 Step = dPos*t + 0.5f*dd*t*t;
        dPos += dd*t;
        //if (Controller.isGrounded)
        //{
        //    ddPos -= dPos;
        //}
        if (dPos.magnitude < 0.01)
        {
            dPos = Vector2.zero;
        }

        Controller.Move(Step);
        speed = dPos.magnitude / 3.6f;
    }

    private void OnEnable() {
        Controls.Enable();
    }

    private void OnDisable() {
        Controls.Disable();
    }
}
