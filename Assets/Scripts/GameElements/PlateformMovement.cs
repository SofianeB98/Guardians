using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateformMovement : Bolt.EntityEventListener<IMovementPlateformState>
{
    [SerializeField] private AnimationCurve curveMovement;
    [SerializeField] private float diviseurTimer = 5f;
    [SerializeField] private float maxDistance = 5f;

    private float speedMove = 0f;

    //private Rigidbody rigid;

    private enum AxisMoving{ X, Y, Z};

    [SerializeField] private AxisMoving axis = AxisMoving.X;
    private float currentTimer;
    private bool direction;
    private float startPosition;

    private void Awake()
    {
        //this.rigid = GetComponent<Rigidbody>();
        //this.rigid.isKinematic = true;
        this.currentTimer = 0f;
        this.direction = true;

        switch (axis)
        {
            case AxisMoving.X:
                this.startPosition = this.transform.position.x;
                break;

            case AxisMoving.Y:
                this.startPosition = this.transform.position.y;
                break;

            case AxisMoving.Z:
                this.startPosition = this.transform.position.z;
                break;
        }

        this.speedMove = this.maxDistance / this.diviseurTimer;
    }

    public override void Attached()
    {
        state.SetTransforms(state.Transform, transform);
        if (entity.IsOwner)
        {
            state.Direction = this.direction;
        }
        state.AddCallback("Direction", DirectionCallBack);
    }

    public override void SimulateOwner()
    {
        if (direction)
        {
            currentTimer += BoltNetwork.FrameDeltaTime / diviseurTimer;
            if (currentTimer >= 1.0f)
            {
                currentTimer = 1.0f;
                state.Direction = false;
            }
        }
        else
        {
            currentTimer -= BoltNetwork.FrameDeltaTime / diviseurTimer;
            if (currentTimer <= 0.0f)
            {
                currentTimer = 0.0f;
                state.Direction = true;
            }
        }
        UpdateDirection(this.curveMovement.Evaluate(currentTimer)*maxDistance);
        //transform.position = rigid.transform.position;
    }

    private void UpdateDirection(float dir)
    {
        switch (axis)
        {
            case AxisMoving.X:
                this.transform.position = new Vector3(startPosition + dir, this.transform.position.y, this.transform.position.z);
                break;

            case AxisMoving.Y:
                this.transform.position = new Vector3(this.transform.position.x, startPosition + dir, this.transform.position.z);
                break;

            case AxisMoving.Z:
                this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, startPosition + dir);
                break;
        }
    }

    public Vector3 VectorDirecteurPlateforme()
    {
        switch (axis)
        {
            case AxisMoving.X:
                return Vector3.right * this.speedMove * (direction ? 1 : -1);
                break;

            case AxisMoving.Y:
                return Vector3.up * this.speedMove * (direction ? 1 : -1);
                break;

            case AxisMoving.Z:
                return Vector3.forward * this.speedMove * (direction ? 1 : -1);
                break;
        }
        return Vector3.zero;
    }

    private void DirectionCallBack()
    {
        this.direction = state.Direction;
    }
}
