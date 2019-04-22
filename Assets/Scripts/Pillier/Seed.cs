using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : Bolt.EntityBehaviour<ISeedState>
{
    [SerializeField] private Rigidbody rigid;
    [SerializeField] private Quaternion pillierRotate;
    [SerializeField] private Guardian myGuardian;
    [SerializeField] private int myTeam;
    [SerializeField] private string groundTag = "Ground";

    public override void Attached()
    {
        state.SetTransforms(state.Transform, this.transform);
    }

    public void Init(int team, Guardian guardian, Quaternion rotation)
    {
        this.myTeam = team;
        this.myGuardian = guardian;
        this.pillierRotate = rotation;
    }

    public void InitVelocity(float force, Vector3 dir)
    {
        this.rigid.isKinematic = false;
        this.rigid.AddForce(dir * force, ForceMode.Impulse);
    }

    public override void SimulateOwner()
    {
        this.transform.position = this.rigid.position;
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.transform.tag.Contains(this.groundTag))
        {
            BoltNetwork.Instantiate(BoltPrefabs.Pillier, this.transform.position, this.pillierRotate);
            BoltNetwork.Destroy(this.gameObject);
        }
    }
}
