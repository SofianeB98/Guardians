using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePlateformMovement : Bolt.EntityEventListener<IMovementPlateformState>
{
    [SerializeField] private Transform objectToRotate;
    [SerializeField] private bool canRotateObject = false;
    
    [SerializeField] private float speed = 10f;
    private float speedToReach = 0f;

    public override void Attached()
    {
        state.SetTransforms(state.Transform, transform);
        if (entity.IsOwner)
        {
            state.Rotation = this.transform.rotation;
        }

        speedToReach = (2 * Mathf.PI * Vector3.Distance(objectToRotate.position, this.transform.position) *
                        (this.speed / 360));

    }

    public override void SimulateOwner()
    {
        //if (canRotateObject)
        {
            this.transform.RotateAround(objectToRotate.position, Vector3.up, this.speed * BoltNetwork.FrameDeltaTime);
        }
        //else
        {
           // return;
        }
        
    }

    public Vector3 VectorDirecteurPlateforme(Transform avatarPos)
    {
        return this.transform.right * this.speedToReach;
    }

    private void RotationCallback()
    {
        state.Rotation = this.transform.rotation;
    }
}
