﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotBehaviour : Bolt.EntityBehaviour<IRobotState>
{
    public override void Attached()
    {
        state.SetTransforms(state.Transform, transform);
        state.SetAnimator(GetComponent<Animator>());

        state.Animator.applyRootMotion = entity.isOwner;
    }

    public override void SimulateOwner()
    {
        var speed = state.Speed;
        var angularSpeed = state.AngularSpeed;

        if (Input.GetKey(KeyCode.W))
        {
            speed += 0.025f;
        }
        else
        {
            speed -= 0.025f;
        }

        if (Input.GetKey(KeyCode.A))
        {
            angularSpeed -= 0.025f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            angularSpeed += 0.025f;
        }
        else
        {
            if (angularSpeed < 0)
            {
                angularSpeed += 0.025f;
                angularSpeed = Mathf.Clamp(angularSpeed, -1f, 0);
            }
            else if (angularSpeed > 0)
            {
                angularSpeed -= 0.025f;
                angularSpeed = Mathf.Clamp(angularSpeed, 0, +1f);
            }
        }

        state.Speed = Mathf.Clamp(speed, 0f, 1.5f);
        state.AngularSpeed = Mathf.Clamp(angularSpeed, -1f, +1f);
    }
}
