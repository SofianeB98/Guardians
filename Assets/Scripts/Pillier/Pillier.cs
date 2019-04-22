using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pillier : Bolt.EntityEventListener<IPillierState>
{
    [Header("Base")]
    [SerializeField] private GameObject pillierGO;
    [SerializeField] private GameObject laserGO;

    [Header("Rotate Laser")]
    [SerializeField] private float maxRotation = 90.0f;
    [SerializeField] private float speedRotation = 5f;
    private bool reverseRotate = false;
    private float currentRotationObject = 0f;

    public override void Attached()
    {
        state.SetTransforms(state.Transform, this.transform);
    }

    private void RotateLaser()
    {
        if (!reverseRotate)
        {
            if (this.transform.eulerAngles.y <= this.transform.eulerAngles.y + 45)
            {
                this.transform.eulerAngles = Vector3.Lerp(this.transform.eulerAngles,
                    new Vector3(0, this.currentRotationObject + this.maxRotation / 2, 0),
                    BoltNetwork.FrameDeltaTime * this.speedRotation);
            }
            else
            {
                this.reverseRotate = true;
            }
            
        }
        else
        {
            if (this.transform.eulerAngles.y >= this.transform.eulerAngles.y - 45)
            {
                this.transform.eulerAngles = Vector3.Lerp(this.transform.eulerAngles,
                    new Vector3(0, this.currentRotationObject - this.maxRotation / 2, 0),
                    BoltNetwork.FrameDeltaTime * this.speedRotation);
            }
            else
            {
                this.reverseRotate = false;
            }
        }
        
    }

    private void RotatePillier(float rotationAngle)
    {
        this.currentRotationObject = rotationAngle;
        this.transform.eulerAngles = Vector3.Lerp(this.transform.eulerAngles,
            new Vector3(0, this.currentRotationObject, 0),
            BoltNetwork.FrameDeltaTime * this.speedRotation);
    }
}