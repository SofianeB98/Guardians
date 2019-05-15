using System.Collections;
using System.Collections.Generic;
using FMOD;
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

    public float AngleToRotate(Vector3 pos)
    {
        Vector3 centre = objectToRotate.position;
        
        Vector3 initDir = this.transform.position - centre;
        initDir = initDir.normalized;

        Vector3 focusDir = (pos - centre).normalized;
        
        
        //Get the dot product
        float dot = Vector3.Dot(initDir, focusDir);
        // Divide the dot by the product of the magnitudes of the vectors
        dot = dot / (initDir.magnitude * focusDir.magnitude);
        //Get the arc cosin of the angle, you now have your angle in radians 
        var acos = Mathf.Acos(dot);
        //Multiply by 180/Mathf.PI to convert to degrees
        var angle = acos * 180 / Mathf.PI;
        //Congrats, you made it really hard on yourself.

        angle = initDir.x > focusDir.x ? angle *1 : angle *-1;

        //print(angle);
        
        return angle;
    }

    public Vector3 FinalPos(float angle, Vector3 pos)
    {
        Vector3 centre = objectToRotate.position;
        centre.y = this.transform.position.y;

        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);

        Vector3 initDir = this.transform.position - centre;
        initDir = initDir.normalized;

        centre.y = pos.y;

        float distance = Vector3.Distance(centre, pos);

        Vector3 positionFinal = centre + rot * initDir * distance;
        

        return positionFinal;
    }

    private void RotationCallback()
    {
        state.Rotation = this.transform.rotation;
    }
}
