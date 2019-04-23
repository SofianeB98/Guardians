using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillierLaser : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private GameObject pillierGO;
    [SerializeField] private GameObject laserGO;
    [SerializeField] private Transform seedDrop;
    private bool destroy = false;
    [SerializeField] private SeedBomb prefabSeed;

    [Header("Rotate Laser")]
    [SerializeField] private float maxRotation = 90.0f;
    [SerializeField] private float speedRotation = 5f;
    private bool reverseRotate = false;
    private float currentAngleRotate = 45f;

    private void Update()
    {
        if (this.pillierGO.transform.localScale.y < 4.0f)
        {
            this.pillierGO.transform.localScale += BoltNetwork.FrameDeltaTime * Vector3.up * 4f;
        }
        else if (!this.laserGO.activeSelf)
        {
            this.laserGO.SetActive(true);
        }
        else
        {
            this.RotateLaser();
        }

        if (this.destroy)
        {
            SeedBomb s = Instantiate(prefabSeed, this.seedDrop.position, Quaternion.identity);
            s.Init(0, null, Quaternion.identity, false);
        }
    }

    private void RotateLaser()
    {
        if (!reverseRotate)
        {
            if (currentAngleRotate < 90)
            {
                this.transform.eulerAngles += Vector3.up * BoltNetwork.FrameDeltaTime * this.speedRotation;
                this.currentAngleRotate += BoltNetwork.FrameDeltaTime * this.speedRotation;
            }
            else
            {
                this.reverseRotate = true;
            }
        }
        else
        {
            if (currentAngleRotate > 0)
            {
                this.transform.eulerAngles -= Vector3.up * BoltNetwork.FrameDeltaTime * this.speedRotation;
                this.currentAngleRotate -= BoltNetwork.FrameDeltaTime * this.speedRotation;
            }
            else
            {
                this.reverseRotate = false;
            }
        }

    }

    private void RotatePillier(float rotationAngle)
    {
        var newRotate = this.transform.eulerAngles.y + rotationAngle;
        this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, newRotate, this.transform.eulerAngles.z);
    }

    public void DestroyPillier()
    {
        this.destroy = true;
    }
}
