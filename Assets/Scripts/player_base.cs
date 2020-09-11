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

    float WalkStrength = 3500.0f;
    float RunMultiplier = 3.0f;
    public float Mass = 70;
    public float FrictionCoefficientGround = 0.9f;
    public float JumpForce = 13000f;
    public float Gravity = 26f;

    game_controls Controls;
    v2 Input;
    v3 ForceDir;
    float ForceMagnitude;
    v3 ddPos;
    v3 dPos;
    
    private void Awake() {
        ForceMagnitude = WalkStrength;

        Controls = new game_controls();
        Controller = GetComponent<CharacterController>();

        Controls.InGame.Move.performed += ctx => SetForce(ctx.ReadValue<Vector2>());
        Controls.InGame.Move.canceled  += _ => ForceDir = Vector2.zero;

        Controls.InGame.Run.performed += _ => ForceMagnitude = WalkStrength*RunMultiplier;
        Controls.InGame.Run.canceled  += _ => ForceMagnitude = WalkStrength;

        Controls.InGame.Jump.performed += _ => { ForceDir.y = Controller.isGrounded ? JumpForce : 0; };
        Controls.InGame.Jump.canceled  += _ => ForceDir.y = 0;
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
        ForceDir.x = Direction.x;
        ForceDir.z = Direction.z;
    }

    void Update()
    {
        v3 Force = ForceDir*ForceMagnitude;
        Force.y = ForceDir.y;
        Body.LookAt(Body.position + new Vector3(Force.x, 0, Force.z));
        
        float Weight = Mass * 9.81f;
        float FrictionCoefficient = FrictionCoefficientGround;
        v3 Friction = -dPos * Weight * FrictionCoefficient;
        if (Controller.isGrounded)
        {
            Friction.y = 0;
        }
        else
        {
            Friction.y = -Mass*Gravity;
            ForceDir.y += Friction.y;
            Force.y += Friction.y;
        }
        v3 TotalForce = Force + Friction;
        ddPos = TotalForce / Mass;
    }

    private void FixedUpdate() {
        float t = Time.fixedDeltaTime;

        v3 Step = dPos*t + 0.5f*ddPos*t*t;

        //transform.position += Step;
        Controller.Move(Step);
        dPos += ddPos*t;

        if (dPos.magnitude < 0.01)
        {
            dPos = Vector2.zero;
        }
    }

    private void OnEnable() {
        Controls.Enable();
        //Controls.KeyboardMouseScheme.
    }

    private void OnDisable() {
        Controls.Disable();
    }
}
