using System.Collections;
using System.Collections.Generic;
using Bolt;
using UnityEngine;

public class Guardian : Bolt.EntityEventListener<IGuardianState>
{
    [Header("General Info")]
    [SerializeField] private int myTeam = 0;
    
    [Header("Player Stats")]
    [SerializeField] private float health = 100f;
    private float lastHealth = 100f;
    [SerializeField] private int currentInventorySeed = 5;
    [SerializeField] private int maxSeedInInventory = 5;
    public bool IsStuned { get; private set; }
    [SerializeField] private float stunTime = 5f;
    private float currentStunTime = 0f;

    [Header("Melee Hit Info")]
    [SerializeField] private float detectionHitRadius = 1f;
    [SerializeField] private Transform meleeHitPosition;
    [SerializeField] private LayerMask meleeHitCheckLayerMask;
    [SerializeField] private float durationMeleeAttack = 1f;
    [SerializeField] private float cooldownMeleeHit = 5f;
    private float currentCooldownMeleeHit = 0f;
    public bool IsCooldown { get; private set; }
    public bool IsMeleeAttack { get; private set; }

    [Header("Seed Pick Up")]
    [SerializeField] private LayerMask seedLayerMask;
    [SerializeField] private Transform feetPosition;
    [SerializeField] private float checkRadius = 1f;

    [Header("Seed Launch")]
    [SerializeField] private Seed seedPrefab;

    public override void Attached()
    {
        SetupTeam(NetworkCallbacks.team);
        if (entity.IsOwner)
        {
            state.Team = this.myTeam;
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
                Guardian enemyGuardian = col[i].GetComponent<Guardian>();

                if (enemyGuardian != null && !this.IsStuned)
                {
                    if (currentEnemyGuardian.myTeam != enemyGuardian.myTeam && !this.IsStuned)
                    {
                        currentEnemyGuardian = enemyGuardian;
                        enemyGuardian.TakeDamage(1);
                        Debug.Log("enemy has been hit");
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

    #region Interaction
    
    public void TakeDamage(float getDamage){
        var flash = TakeDamageEvent.Create(entity, EntityTargets.OnlySelf);
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

    #endregion

    #region SeedInteraction

    public void LaunchSeed()
    {
        Seed seed = BoltNetwork.Instantiate(BoltPrefabs.Seed, this.transform.position + this.transform.forward, Quaternion.identity).GetComponent<Seed>();
        seed.Init(this.myTeam, this, this.transform.rotation);
        seed.InitVelocity(10f, this.transform.forward);
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
                    Destroy(col[i].gameObject);
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
    
    private void Death()
    {
        var spawnPosition = new Vector3(Random.Range(-8, 8), 0, Random.Range(-8, 8));
        this.health = this.lastHealth;
        this.transform.position = spawnPosition;

    }
    
}
