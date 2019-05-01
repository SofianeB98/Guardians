using System.Collections;
using System.Collections.Generic;
using Bolt;
using TMPro;
using UnityEngine;

public class Guardian : Bolt.EntityEventListener<IGuardianState>
{
    [Header("General Info")]
    [SerializeField] private int myTeam = 0;
    private int currentKill = 0;
    public int CurrentKill
    {
        get { return currentKill; }
    }
    public int CurrentScore { get; private set; }
    [SerializeField] private CompleteCharacterController characterController;
    [SerializeField] private LayerMask videLayerMask;
    public string guardianName { get; private set; }

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
    [SerializeField] private TextMeshProUGUI killText;

    [Header("Melee Hit Info")]
    [SerializeField] private float detectionHitRadius = 1f;
    [SerializeField] private Transform meleeHitPosition;
    [SerializeField] private LayerMask meleeHitCheckLayerMask;
    [SerializeField] private float durationMeleeAttack = 1f;
    [SerializeField] private float cooldownLaunchSeed = 5f;
    private float currentCooldownLaunchSeed = 0f;
    public bool IsCooldown { get; private set; }
    public bool IsMeleeAttack { get; private set; }

    [Header("Axe Launch")]
    [SerializeField] private Axe myAxe;
    public Axe MyAxe
    {
        get { return myAxe; }
    }
    public bool IsLaunchAxe { get; private set; }

    [Header("Seed Pick Up")]
    [SerializeField] private LayerMask seedLayerMask;
    [SerializeField] private Transform feetPosition;
    [SerializeField] private float checkRadius = 1f;

    [Header("Seed Launch")]
    [SerializeField] private Transform cameraRef;
    public Transform CameraRef
    {
        get { return cameraRef; }
    }
    [SerializeField] private float forceLaunch = 10f;
    [SerializeField] private Vector3 dirLaunch;
    public bool IsPreLaunchSeed { get; private set; }
    [SerializeField] private List<Pillier> myPillier = new List<Pillier>();
    [SerializeField] private int maxPillier = 20;
    [SerializeField] private bool destroyAllPillierwhenIDie = true;

    [Header("Audio")]
    [FMODUnity.EventRef]
    [SerializeField] private string deathAudioMeEvent = "";
    [SerializeField] private FMOD.Studio.EventInstance deathAudioMe;
    [FMODUnity.EventRef]
    [SerializeField] private string deathAudioOtherEvent = "";
    [SerializeField] private FMOD.Studio.EventInstance deathAudioOther;
    [FMODUnity.EventRef]
    [SerializeField] private string launchAxeAudioEvent = "";
    [SerializeField] private FMOD.Studio.EventInstance launchAxeAudio;
    [FMODUnity.EventRef]
    [SerializeField] private string launchSeedAudioEvent = "";
    [SerializeField] private FMOD.Studio.EventInstance launchSeedAudio;
    [FMODUnity.EventRef]
    [SerializeField] private string axeIsBackEvent = "";
    [SerializeField] private FMOD.Studio.EventInstance axeIsBack;

    public override void Attached()
    {
        SetupTeam(NetworkCallbacks.team);
        this.currentKill = 0;
        if (entity.IsOwner)
        {
            if (PlayerPrefs.HasKey("PlayerName"))
            {
                state.GuardianName = PlayerPrefs.GetString("PlayerName");
            }
            else
            {
                state.GuardianName = "New Player";
            }
            
            state.MyColor = new Color(Random.value, Random.value, Random.value);
            
        }
        state.AddCallback("MyColor", ColorChanged);
        state.AddCallback("GuardianName", PlayerName);

        deathAudioMe = FMODUnity.RuntimeManager.CreateInstance(deathAudioMeEvent);
        deathAudioOther = FMODUnity.RuntimeManager.CreateInstance(deathAudioOtherEvent);
        launchAxeAudio = FMODUnity.RuntimeManager.CreateInstance(launchAxeAudioEvent);
        launchSeedAudio = FMODUnity.RuntimeManager.CreateInstance(launchSeedAudioEvent);
        axeIsBack = FMODUnity.RuntimeManager.CreateInstance(axeIsBackEvent);
    }
    
    private void Update()
    {
        this.killText.text = "My Score : " + this.currentKill.ToString();
        if (this.currentStunTime < Time.time)
        {
            this.IsStuned = false;
        }

        if (this.currentCooldownLaunchSeed < Time.time)
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
        var lnch = LaunchAxeEvent.Create(entity);
        lnch.Launch = launch;
        lnch.Send();
    }

    public void BackToBucheron()
    {
        var bck = BackToBucheronEvent.Create(entity);
        bck.Send();
    }

    public override void OnEvent(BackToBucheronEvent evnt)
    {
        this.myAxe.ActiveBackToBucheron();
    }

    public override void OnEvent(LaunchAxeEvent evnt)
    {
        
        
        this.IsLaunchAxe = evnt.Launch;
        if (evnt.Launch)
        {
            /////Son
            launchAxeAudio.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
            launchAxeAudio.start();
            /////Son
            this.myAxe.isCanLaunchAxe(!evnt.Launch);
        }
        else
        {
            if (entity.IsOwner)
            {
                /////Son
                axeIsBack.start();
                /////Son
            }
        }
    }

    #endregion

    #region Interaction

    public void TakeDamage(float getDamage)
    {
        var flash = TakeDamageEvent.Create(entity);
        flash.GetDamage = getDamage;
        flash.Send();
        return;
    }

    public void SetStun(Vector3 dir, float force)
    {
        var stun = SetStunEvent.Create(entity);
        stun.IsStuned = true;
        stun.Direction = dir;
        stun.Force = force;
        stun.Send();
    }

    public void SetCooldown()
    {
        var coolD = CooldownEvent.Create(entity, EntityTargets.OnlySelf);
        coolD.Durantion = this.cooldownLaunchSeed;
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
        this.currentStunTime = Time.time + this.stunTime;
        if (this.IsStuned)
        {
            this.characterController.AddForce(evnt.Direction, evnt.Force);
        }
            
    }

    public override void OnEvent(CooldownEvent evnt)
    {
        this.IsCooldown = true;
        this.currentCooldownLaunchSeed = Time.time + evnt.Durantion;
    }

    private void Death()
    {
        if (this.destroyAllPillierwhenIDie)
        {
            foreach (var pillier in this.myPillier)
            {
                BoltNetwork.Destroy(pillier.gameObject);
            }
            this.myPillier = new List<Pillier>();
        }

        if (entity.IsOwner)
        {
            deathAudioMe.start();
        }
        else
        {
            deathAudioOther.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
            deathAudioOther.start();
        }

        var spawnPosition = RespawnPoint();
        this.health = this.lastHealth;
        this.transform.position = spawnPosition;
    }

    private Vector3 RespawnPoint()
    {
        int[] point = new int[NetworkCallbacks.SpawnPointsTransforms.Length];
        int currentPoint = 9999999;
        int currentIndex = 0;
        for (int i = 0; i < point.Length; i++)
        {
            Collider[] col = Physics.OverlapSphere(NetworkCallbacks.SpawnPointsTransforms[i].transform.position, 5f);
            if (col.Length > 0)
            {
                for (int j = 0; j < col.Length; j++)
                {
                    Guardian g = col[j].GetComponent<Guardian>();
                    Pillier p = col[j].GetComponent<Pillier>();
                    if (g != null)
                    {
                        point[i] += 2;
                    }
                    else if (p != null)
                    {
                        point[i] += 1;
                    }
                }
            }
            else
            {
                point[i] = 0;
            }
        }

        for (int i = 0; i < point.Length; i++)
        {
            if (point[i] <= currentPoint)
            {
                currentPoint = point[i];
                currentIndex = i;
            }
        }

        return NetworkCallbacks.SpawnPointsTransforms[currentIndex].transform.position + Vector3.up*2;
    }

    public void AddPillierToMyList(Pillier pillierToAdd)
    {
        this.myPillier.Add(pillierToAdd);
        if (this.myPillier.Count > this.maxPillier)
        {
            BoltNetwork.Destroy(this.myPillier[0].gameObject);
            this.myPillier.RemoveAt(0);
        }
    }

    #endregion

    #region SeedInteraction

    public void SetupLaunchSeed()
    {
        //if (this.currentInventorySeed > 0)
        if(!this.IsCooldown)
        {
            IsPreLaunchSeed = true;
            this.dirLaunch = this.cameraRef.forward;
        }
        else
        {
            return;
        }
    }

    public void LaunchSeed()
    {
        this.IsPreLaunchSeed = false;
        //if (this.currentInventorySeed > 0)
        if(!IsCooldown)
        {
            Seed s = BoltNetwork.Instantiate(BoltPrefabs.Seed, this.transform.position + this.transform.forward * 2, Quaternion.identity).GetComponent<Seed>();
            s.Init(this.myTeam, this, this.transform.rotation, true, entity, state.MyColor);
            s.InitVelocity(this.forceLaunch, this.dirLaunch);

            /////Son
            launchSeedAudio.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
            launchSeedAudio.start();
            /////Son

            //this.currentInventorySeed--;
        }
        else
        {
           // return;
        }
    }
    
    public void CheckVide()
    {
        if (this.currentInventorySeed < this.maxSeedInInventory)
        {
            Collider[] col = Physics.OverlapSphere(this.feetPosition.position, 1.25f, this.videLayerMask);

            if (col.Length > 0)
            {
                this.TakeDamage(1);
                UpdateScore(true);
            }
        }
        else
        {
            return;
        }

    }

    #endregion

    public void UpdateScore(bool isMe)
    {
        var evnt = UpdateScoreEvent.Create(entity);
        evnt.IsMe = isMe;
        evnt.Send();
    }

    public override void OnEvent(UpdateScoreEvent evnt)
    {
        if (!evnt.IsMe)
        {
            this.currentKill++;
            this.CurrentScore += 10;
        }
        else
        {
            this.CurrentScore -= 5;
        }
            
    }

    private void SetupTeam(int team)
    {
        this.myTeam = team;
        this.lastHealth = health;
    }
    
    public override void OnEvent(DestroyEvent evnt)
    {
        BoltNetwork.Destroy(evnt.EntityDestroy.gameObject);
    }

    void ColorChanged()
    {
        GetComponent<Renderer>().material.color = state.MyColor;
    }

    void PlayerName()
    {
        this.guardianName = state.GuardianName;
    }

}
