using System.Collections;
using System.Collections.Generic;
using Bolt;
using UnityEngine;

public class Guardian : Bolt.EntityEventListener<IGuardianState>
{
    [Header("General Info")]
    [SerializeField] private int myTeam = 0;
    private int currentKill = 0;

    [Header("Player Stats")]
    [SerializeField] private float health = 100f;
    private float lastHealth = 100f;
    [SerializeField] private int currentInventorySeed = 5;
    [SerializeField] private int maxSeedInInventory = 5;
    public bool IsStuned { get; private set; }
    [SerializeField] private float stunTime = 5f;
    private float currentStunTime = 0f;
    public bool IsInvinsible { get; private set; }
    [SerializeField] private float invinsibleTime = 2f;
    private float currentInvinsibleTime = 0f;

    [Header("Melee Hit Info")]
    [SerializeField] private float detectionHitRadius = 1f;
    [SerializeField] private Transform meleeHitPosition;
    [SerializeField] private LayerMask meleeHitCheckLayerMask;
    [SerializeField] private float durationMeleeAttack = 1f;
    [SerializeField] private float cooldownMeleeHit = 5f;
    private float currentCooldownMeleeHit = 0f;
    public bool IsCooldown { get; private set; }
    public bool IsMeleeAttack { get; private set; }

    [Header("Axe Launch")]
    [SerializeField] private AxeLaunch myAxe;
    public AxeLaunch MyAxe
    {
        get { return myAxe; }
    }
    public bool IsLaunchAxe { get; private set; }

    [Header("Seed Pick Up")]
    [SerializeField] private LayerMask seedLayerMask;
    [SerializeField] private Transform feetPosition;
    [SerializeField] private float checkRadius = 1f;
    

    public override void Attached()
    {
        SetupTeam(NetworkCallbacks.team);
        this.currentKill = 0;
        if (entity.IsOwner)
        {
            entity.TakeControl();
        }
    }
    
    private void Update()
    {
        if (this.currentStunTime < Time.time)
        {
            this.IsStuned = false;
        }

        if (this.currentCooldownMeleeHit < Time.time)
        {
            this.IsCooldown = false;
        }

        if (this.currentInvinsibleTime < Time.time)
        {
            this.IsInvinsible = false;
        }
    }

    #region MeleeAttack
    Guardian currentEnemyGuardian = null;

    public IEnumerator LaunchMeleeAttack()
    {
        this.IsMeleeAttack = true;
        StartCoroutine(this.StopMeleeAttack());
        yield return new WaitForEndOfFrame();
        while (this.IsMeleeAttack)
        {
            yield return new WaitForSeconds(12.0f/60.0f);
            MeleeAttack();
        }

        this.IsMeleeAttack = false;
        yield break;
    }
    
    private void MeleeAttack()
    {
        Collider[] col = Physics.OverlapSphere(this.meleeHitPosition.position, this.detectionHitRadius, this.meleeHitCheckLayerMask);
        
        if (col != null)
        {
            for (int i = 0; i < col.Length; i++)
            {
                Pillier pillier = col[i].GetComponent<Pillier>();

                if (!this.IsStuned)
                {
                    if (pillier != null)
                    {
                        Debug.Log("Pillier hit");
                        pillier.DestroyPillier();
                        i = col.Length;
                        return;
                    }
                }

                if (this.IsStuned)
                {
                    this.IsMeleeAttack = false;
                }
            }
        }
    }

    IEnumerator StopMeleeAttack()
    {
        yield return new WaitForSeconds(this.durationMeleeAttack);
        this.IsMeleeAttack = false;
        this.IsCooldown = true;
        currentEnemyGuardian = null;
        SetCooldown();
        yield break;
    }

    #endregion

    #region LancerDeHache

    public void SetLaunchAxe(bool launch)
    {
        this.IsLaunchAxe = launch;
        if (launch)
        {
            this.myAxe.isCanLaunchAxe(false, this.transform.rotation);
        }
    }

    #endregion

    #region Interaction

    public void TakeDamage(float getDamage)
    {
        var flash = TakeDamageEvent.Create(entity, EntityTargets.OnlyOwner);
        flash.GetDamage = getDamage;
        flash.Send();
        return;
    }

    public void SetStun()
    {
        var stun = SetStunEvent.Create(entity);
        stun.IsStuned = true;
        stun.StunTime = this.stunTime;
        stun.Send();
    }

    private void SetCooldown()
    {
        var coolD = CooldownEvent.Create(entity, EntityTargets.OnlySelf);
        coolD.Durantion = this.cooldownMeleeHit;
        coolD.Send();
    }

    public override void OnEvent(TakeDamageEvent evnt)
    {
        this.health -= evnt.GetDamage;
        this.IsInvinsible = true;
        this.currentInvinsibleTime = Time.time + invinsibleTime;
        if (this.health <= 0)
        {
            Debug.Log("Death");
            Death();
        }
    }

    public override void OnEvent(SetStunEvent evnt)
    {
        this.IsStuned = evnt.IsStuned;
        this.currentStunTime = Time.time + evnt.StunTime;
    }

    public override void OnEvent(CooldownEvent evnt)
    {
        this.currentCooldownMeleeHit = Time.time + evnt.Durantion;
    }

    private void Death()
    {
        var spawnPosition = new Vector3(Random.Range(-8, 8), 0, Random.Range(-8, 8));
        this.health = this.lastHealth;
        this.transform.position = spawnPosition;

    }

    #endregion

    #region SeedInteraction

    public void LaunchSeed()
    {
        if (this.currentInventorySeed > 0)
        {
            Seed s = BoltNetwork.Instantiate(BoltPrefabs.Seed, this.transform.position + this.transform.forward, Quaternion.identity).GetComponent<Seed>();
            s.Init(this.myTeam, this, this.transform.rotation, true, entity);
            s.InitVelocity(10f, this.transform.forward);
            this.currentInventorySeed--;
        }
        else
        {
            return;
        }
        
    }

    public void CheckBombSeed()
    {
        if (this.currentInventorySeed < this.maxSeedInInventory)
        {
            Collider[] col = Physics.OverlapSphere(this.feetPosition.position, 1f, this.seedLayerMask);

            if (col.Length > 0)
            {
                for (int i = 0; i < col.Length; i++)
                {
                    col[i].GetComponent<Seed>().DestroyOnPickUp();
                    this.currentInventorySeed++;
                }
            }
        }
        else
        {
            return;
        }
        
    }

    #endregion
    
    private void SetupTeam(int team)
    {
        this.myTeam = team;
        this.lastHealth = health;
    }
    
    public override void OnEvent(DestroyEvent evnt)
    {
        BoltNetwork.Destroy(evnt.EntityDestroy.gameObject);
    }

}
