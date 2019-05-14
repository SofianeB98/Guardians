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
        speedToReach = 2 * Mathf.PI * Vector3.Distance(objectToRotate.position, avatarPos.position) *
                        (this.speed / 360);
        Vector3 dirPerso = (avatarPos.position - objectToRotate.position).normalized;
        dirPerso.y = 0;

        //float angleRad = Vector3.Angle(Vector3.forward, dirPerso);
        
        //Debug.Log(angleRad);
        
        //Quaternion rotation = Quaternion.AngleAxis(angleRad, Vector3.up);

        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, dirPerso);

        Vector3 dir = rotation * Vector3.right * speedToReach;

        return dir; //this.transform.right * this.speedToReach;
    }

    private void RotationCallback()
    {
        state.Rotation = this.transform.rotation;
    }
}
