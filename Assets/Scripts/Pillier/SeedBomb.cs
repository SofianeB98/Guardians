using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedBomb : MonoBehaviour
{
    [SerializeField] private PillierLaser prefabPillier;
    [SerializeField] private Rigidbody rigid;
    [SerializeField] private Quaternion pillierRotate;
    [SerializeField] private Guardian myGuardian;
    [SerializeField] private int myTeam;
    [SerializeField] private string groundTag = "Ground";
    private bool isLaunchPlayer = false;

    public void Init(int team, Guardian guardian, Quaternion rotation, bool launchPlayer)
    {
        this.myTeam = team;
        this.myGuardian = guardian;
        this.pillierRotate = rotation;
        this.isLaunchPlayer = launchPlayer;
        this.rigid.isKinematic = false;
    }

    public void InitVelocity(float force, Vector3 dir)
    {
        this.rigid.isKinematic = false;
        this.rigid.AddForce(dir * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision col)
    {
        if (this.isLaunchPlayer)
        {
            if (col.transform.tag.Contains(this.groundTag))
            {
                PillierLaser pillier = Instantiate(prefabPillier, this.transform.position, this.pillierRotate);
                Destroy(this.gameObject);
            }
        }
        else
        {
            if (col.transform.tag.Contains(this.groundTag))
            {
                this.rigid.isKinematic = true;
            }
        }
    }
}
