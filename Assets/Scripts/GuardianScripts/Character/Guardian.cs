﻿using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Bolt;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class Guardian : Bolt.EntityEventListener<IGuardianState>
{
    [Header("General Info")]
    [SerializeField] private int myTeam = 0;
    private int currentKill = 0;
    public int CurrentKill
    {
        get { return currentKill; }
    }
    public int CurrentSerieKill { get; private set; }
    public int CurrentScore { get; private set; }
    [SerializeField] private CompleteCharacterController characterController;
    [SerializeField] private LayerMask videLayerMask;
    public string guardianName { get; private set; }
    Color lastColor = Color.black;
    [SerializeField] private Canvas myCanvas;
    [SerializeField] private GameObject winLosePointPrefab;
    [field:SerializeField] public Transform NamePosition { get; private set; }
    [SerializeField] private Transform winPointPanel;
    [SerializeField] private GameObject[] killSeries;
    private int currentIndexKillSeries = 0;
    [SerializeField] private int everyXKillIWinMedal = 5;
    private int scoreAdditionel = 0;
    [SerializeField] private GameObject guardianExplode;

    [Header("Renderer")]
    [SerializeField] private Renderer[] renderersAvatar;

    [Header("Player Stats")]
    [SerializeField] private float health = 100f;
    //[field: SerializeField] public int Life { get; private set; }
    private float lastHealth = 100f;
    [SerializeField] private GameObject invinsibleFB;
    //[SerializeField] private int currentInventorySeed = 5;
    //[SerializeField] private int maxSeedInInventory = 5;
    public bool IsStuned { get; private set; }
    [SerializeField] private float stunTime = 5f;
    private float currentStunTime = 0f;
    public bool IsInvinsible { get; private set; }
    [SerializeField] private float invinsibleTime = 2f;
    private float currentInvinsibleTime = 0f;
    [SerializeField] private TextMeshProUGUI myNameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI bestEnemyNameText;
    [SerializeField] private TextMeshProUGUI bestEnemyScoreText;
    public bool IsDie { get; private set; }
    [SerializeField] private float dietime = 5f;
    private float currentDietime = 0f;
    [SerializeField] private GameObject deathParticulePrefab;
    [SerializeField] private GameObject respawnParticulePrefab;
    [SerializeField] private ParticleSystem masterKillPrefab;
    private ParticleSystem currentMK;

    [Header("Fus Ro Dah")]
    [SerializeField] private FusRoDaMode mode = FusRoDaMode.Cone;
    [SerializeField] private Transform startPointFus;
    [SerializeField] private float coolDownFus = 0.5f;
    [SerializeField] private float distanceCheck = 10.0f;
    private float finalDistance = 0f;
    [SerializeField] [Range(1.0f, 7.0f)] private float diviseurDistance = 1.0f;
    [SerializeField] private float fusDetectionRadius = 50.0f;
    [SerializeField] [Range(0.0f,90.0f)] private float angleMaxToCheckCone = 45.0f;
    [SerializeField] private Transform pointStartLaser;
    [SerializeField] private Transform pointEndLaser;
    [SerializeField] private float forcePush = 50.0f;
    [SerializeField] private LayerMask fusRoDahLayerMask;
    [SerializeField] private LayerMask fusIgnoreLayerMask;
    public bool IsFusRoDah { get; private set; }
    [SerializeField] private ParticleSystem fusRoDaFeedback;
    [SerializeField] private TextMeshProUGUI pushDispoText;
    [SerializeField] private Image pushDispoImage;
    [SerializeField] private Image pushDispoViseur;
    [SerializeField] private Color pushReadyColor;

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
    private int currentPillier = 0;
    public bool PillierReadyToLaunch
    {
        get { return currentPillier < maxPillier; }
    }
    [SerializeField] private TextMeshProUGUI seedDispo_Text;
    [SerializeField] private bool destroyAllPillierwhenIDie = true;
    [SerializeField] private Transform myHand;
    [SerializeField] private Image seedReadyImage;
    [SerializeField] private Image[] seedReadyImagesViseur;
    private int currentDir = 1;
    [SerializeField] private float cooldownLaunchSeed = 5f;
    private float currentCooldownLaunchSeed = 0f;
    public bool IsCooldown { get; private set; }

    [Header("UI Seed Recharge Gestion")]
    [SerializeField] private Image[] seedReadyUI;
    [SerializeField] private float maxHeightMask = 122f;
    [SerializeField] private RectTransform maskPanel;
    [SerializeField] private Color colorReady;
    [SerializeField] private Color colorNoReady;
    /*[Header("Pillier Sens")]
    [SerializeField] private Image sensPillierImage;
    [SerializeField] private Sprite sensHoraireSprite;
    [SerializeField] private Sprite sensAntiHoraireSprite;*/

    [Header("Guardian Enemy")]
    [SerializeField] private Guardian lastGuardianWhoHitMe = null;
    [SerializeField] private float timeToNullLastEnemy = 5f;
    private float currentTimerNullEnemy = 0f;

    [Header("Audio")]
    [FMODUnity.EventRef]
    [SerializeField] private string deathAudioMeEvent = "";
    [SerializeField] private FMOD.Studio.EventInstance deathAudioMe;
    [FMODUnity.EventRef]
    [SerializeField] private string deathAudioOtherEvent = "";
    [SerializeField] private FMOD.Studio.EventInstance deathAudioOther;
    [FMODUnity.EventRef]
    [SerializeField] private string respawnAudioOtherEvent = "";
    [SerializeField] private FMOD.Studio.EventInstance respawnAudioOther;
    [FMODUnity.EventRef]
    [SerializeField] private string respawnAudioMeEvent = "";
    [SerializeField] private FMOD.Studio.EventInstance respawnAudioMe;


    [SerializeField] private GameObject bwok;
    [SerializeField] private GameObject spike;
    [SerializeField] private GameObject bouc;

    public override void Attached()
    {
        SetupTeam(NetworkCallbacks.team);
        this.currentKill = 0;
        CurrentSerieKill = this.currentKill;

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
            state.ActiveEE = true;
            state.Invinsible = false;

            if (state.GuardianName.Contains("Sofiane"))
            {
                state.MyColor = Color.blue;
            }
            else if (state.GuardianName.Contains("Arthur"))
            {
                state.MyColor = Color.green;
            }
            else if (state.GuardianName.Contains("Scotty"))
            {
                state.MyColor = Color.magenta;
            }
            else if (state.GuardianName.Contains("Matthias"))
            {
                state.MyColor = Color.red;
            }
            else if (state.GuardianName.Contains("Morgane"))
            {
                state.MyColor = Color.yellow;
            }
            else if (state.GuardianName.Contains("Jordan"))
            {
                state.MyColor = Color.white;
            }
            else
            {
                state.MyColor = new Color(Random.value, Random.value, Random.value);
            }

            
            lastColor = state.MyColor;
        }
        
        state.AddCallback("MyColor", ColorChanged);
        state.AddCallback("GuardianName", PlayerName);
        //state.AddCallback("Invinsible", InvinsibleCallBack);

        state.AddCallback("BoucRot", RotateBoucCallBack);
        state.AddCallback("ActiveEE", ActiveEasterE);

        deathAudioMe = FMODUnity.RuntimeManager.CreateInstance(deathAudioMeEvent);
        deathAudioOther = FMODUnity.RuntimeManager.CreateInstance(deathAudioOtherEvent);
        respawnAudioOther = FMODUnity.RuntimeManager.CreateInstance(respawnAudioOtherEvent);
        respawnAudioMe = FMODUnity.RuntimeManager.CreateInstance(respawnAudioMeEvent);

        StartCoroutine(BestEnemyCheck());
        
    }

    private void Start()
    {
        if (entity.IsOwner)
        {
            GameSystem.GSystem.AssignCamToWorldCanvas(this.cameraRef.GetComponent<Camera>(), this);
            this.pushDispoViseur.color = Color.blue;
        }
    }

    private Guardian bestEnemy = null;
    private void Update()
    {
        //bestEnemy = GameSystem.GSystem.BestEnemyGuardian(this);
        this.myNameText.text = this.guardianName.ToString();
        this.scoreText.text = this.CurrentScore.ToString();
        this.seedDispo_Text.text = "x" + (this.maxPillier - this.currentPillier).ToString();
        if (bestEnemy != null)
        {
            this.bestEnemyNameText.text = bestEnemy.guardianName;
            this.bestEnemyScoreText.text = bestEnemy.CurrentScore.ToString();
        }
        else
        {
            this.bestEnemyScoreText.text = "";
        }

        if (this.currentStunTime < Time.time)
        {
            this.IsStuned = false;
            
        }

        /*if (this.currentCooldownLaunchSeed < Time.time)
        {
            this.IsCooldown = false;
            this.currentPillier = this.currentPillier > 0 ? this.currentPillier - 1 : 0;
            this.seedReadyImagesViseur[this.currentPillier].color = Color.green;

            if (currentPillier > 0)
            {
                this.SetCooldown();
                this.seedReadyImage.color = this.currentPillier > 0 ? Color.red : Color.green;
            }
        }
        else
        {
            this.seedReadyImage.color = Color.Lerp(this.seedReadyImage.color, Color.green,
                Time.deltaTime / this.cooldownLaunchSeed);
        }*/

        if (this.currentPillier > 0)
        {
            this.seedReadyImage.color = Color.Lerp(this.seedReadyImage.color, Color.green,
                Time.deltaTime / this.cooldownLaunchSeed);
            this.maskPanel.sizeDelta += new Vector2(0, Time.deltaTime * (this.maxHeightMask/this.maxPillier) / this.cooldownLaunchSeed);
        }

        if (this.currentInvinsibleTime < Time.time)
        {
            this.IsInvinsible = false;
        }

        if (this.currentTimerNullEnemy < Time.time)
        {
            this.lastGuardianWhoHitMe = null;
        }
        
    }

    public override void SimulateOwner()
    {
        state.Invinsible = this.IsInvinsible;
        state.Stuned = this.IsStuned;
        state.Death = this.IsDie;
        if (bouc.activeSelf) state.BoucRot *= Quaternion.AngleAxis(50 * Time.deltaTime, bouc.transform.up);

        if (GameSystem.GSystem.EndGame)
        {
            foreach (var pill in myPillier)
            {
                BoltNetwork.Destroy(pill.gameObject);
            }

            myPillier = new List<Pillier>();
            StartCoroutine(EndGameDeleteGuardian());
        }
    }

    IEnumerator BestEnemyCheck()
    {
        //yield return new WaitForSeconds(3.5f);
        while (!GameSystem.GSystem.GameStart)
        {
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForEndOfFrame();
        bestEnemy = GameSystem.GSystem.BestEnemyGuardian(this);
        while (!GameSystem.GSystem.EndGame)
        {
            yield return new WaitForSeconds(0.5f);
            bestEnemy = GameSystem.GSystem.BestEnemyGuardian(this);
        }

        yield break;
    }

    #region MeleeAttack
    //Guardian currentEnemyGuardian = null;

    /*public IEnumerator LaunchMeleeAttack()
    {
        this.IsFusRoDah = true;
        StartCoroutine(this.StopMeleeAttack());
        yield return new WaitForEndOfFrame();
        while (this.IsFusRoDah)
        {
            yield return new WaitForSeconds(12.0f/60.0f);
            MeleeAttack();
        }

        this.IsFusRoDah = false;
        yield break;
    }
    
    private void MeleeAttack()
    {
        Collider[] col = Physics.OverlapSphere(this.meleeHitPosition.position, this.detectionHitRadius, this.fusRoDahLayerMask);
        
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
                    this.IsFusRoDah = false;
                }
            }
        }
    }

    IEnumerator StopMeleeAttack()
    {
        yield return new WaitForSeconds(this.durationMeleeAttack);
        this.IsFusRoDah = false;
        this.IsCooldown = true;
        currentEnemyGuardian = null;
        SetCooldown();
        yield break;
    }
    */
    #endregion

    #region LancerDeHache

    public void SetLaunchAxe(bool launch)
    {
        var lnch = LaunchAxeEvent.Create(entity);
        lnch.Launch = launch;
        lnch.Orientation = this.cameraRef.rotation;
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
            /////Son
            this.myAxe.isCanLaunchAxe(!evnt.Launch, evnt.Orientation);
        }
        else
        {
            if (entity.IsOwner)
            {
                /////Son
                /////Son
            }
        }
    }

    #endregion

    #region Interaction

    public void TakeDamage(float getDamage)
    {
        //if (!this.IsDie && !this.IsInvinsible)
        {
            var flash = TakeDamageEvent.Create(entity);
            flash.GetDamage = getDamage;
            flash.Respawn = false;
            flash.Send();
        }
        
        return;
    }

    public void SetStun(Vector3 dir, float force, BoltEntity enemy)
    {
        var stun = SetStunEvent.Create(entity);
        stun.IsStuned = true;
        stun.Direction = dir;
        stun.Force = force;
        stun.GuardianEnemy = enemy;
        stun.Send();
        
    }
    
    public void SetCooldown()
    {
        /*var coolD = CooldownEvent.Create(entity, EntityTargets.OnlySelf);
        coolD.Durantion = this.cooldownLaunchSeed;
        coolD.Send();*/
        if (!this.IsCooldown)
        {
            this.IsCooldown = true;
            StartCoroutine(SeedCooldown());
        }
    }

    IEnumerator SeedCooldown()
    {
        yield return new WaitForSeconds(this.cooldownLaunchSeed);
        this.currentPillier = this.currentPillier > 0 ? this.currentPillier - 1 : 0;

        this.seedReadyImagesViseur[this.currentPillier].color = this.colorReady;
        this.seedReadyUI[this.currentPillier].color = this.colorReady;

        if (currentPillier > 0)
        {
            this.seedReadyImage.color = Color.red;
            StartCoroutine(SeedCooldown());
        }
        else
        {
            this.seedReadyImage.color = Color.green;
            this.IsCooldown = false;
        }

        yield break;
    }

    public override void OnEvent(TakeDamageEvent evnt)
    {
        if (!evnt.Respawn)
        {
            this.health -= evnt.GetDamage;
            this.IsInvinsible = true;
            this.currentInvinsibleTime = Time.time + invinsibleTime;
            if (this.health <= 0)
            {
                Debug.Log("Death");

                if (entity.IsOwner)
                {
                    deathAudioMe.start();
                }
                else
                {
                    deathAudioOther.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
                    deathAudioOther.start();
                }

                IsDie = true;

                GameObject go = Instantiate(deathParticulePrefab, this.transform.position, Quaternion.identity);
                go.transform.SetParent(this.transform);
                Destroy(go, 1.9f);

                if (currentMK != null)
                {
                    ParticleSystem.MainModule main = currentMK.main;
                    main.loop = false;
                    Destroy(currentMK.gameObject, 1.2f);
                }
                

                this.currentDietime = Time.time + this.dietime;
                StartCoroutine(Death());

                /*Life--;
                if (Life > 0)
                {
                    
                }
                else
                {
                    IsDie = true;
                    this.currentDietime = Time.time + this.dietime;
                    if(entity.IsOwner) StartCoroutine(EndLife());
                    //GameSystem.GSystem.GuardianDie();

                    var killFeed = KillFeedEvent.Create(GameSystem.GSystem.entity);
                    killFeed.Message = this.guardianName + " has no more Life";
                    killFeed.RemoveFeed = false;
                    killFeed.Send();
                }*/
                this.CurrentSerieKill = 0;
                this.currentIndexKillSeries = 0;
            }
        }
        else
        {
            GameObject go = Instantiate(this.guardianExplode, this.feetPosition.position, this.transform.rotation);
            Destroy(go.gameObject, 4f);

            Respawn();

        }
        
    }

    public override void OnEvent(SetStunEvent evnt)
    {
        this.currentStunTime = Time.time + this.stunTime;
        this.currentTimerNullEnemy = Time.time + this.timeToNullLastEnemy;

        this.IsStuned = evnt.IsStuned;
        state.Stuned = this.IsStuned;
        this.lastGuardianWhoHitMe = evnt.GuardianEnemy.GetComponent<Guardian>();

        if (this.IsStuned)
        {
            this.characterController.AddForce(evnt.Direction, evnt.Force);
        }
            
    }

    public override void OnEvent(CooldownEvent evnt)
    {
        if (!this.IsCooldown)
        {
            this.IsCooldown = true;
            this.currentCooldownLaunchSeed = Time.time + evnt.Durantion;
        }
    }

    private void Respawn()
    {
        IsDie = false;

        if (this.destroyAllPillierwhenIDie)
        {
            foreach (var pillier in this.myPillier)
            {
                //BoltNetwork.Destroy(pillier.gameObject);
                pillier.ActiveDestroy();
            }
            this.myPillier = new List<Pillier>();

            foreach (var seedOk in this.seedReadyImagesViseur)
            {
                seedOk.color = this.colorReady;
            }

            foreach (var seedOk in this.seedReadyUI)
            {
                seedOk.color = this.colorReady;
            }

            this.maskPanel.sizeDelta = new Vector2(this.maskPanel.rect.width, this.maxHeightMask);

            this.currentPillier = 0;
            this.seedReadyImage.color = Color.green;
            this.currentCooldownLaunchSeed = Time.time + 1;
            this.IsCooldown = false;
            
        }
        
        if (entity.IsOwner)
        {
            state.MyColor = lastColor;
            respawnAudioMe.start();
        }
        else
        {
            respawnAudioOther.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
            respawnAudioOther.start();
        }

        this.lastGuardianWhoHitMe = null;
        var spawnPosition = RespawnPoint();

        this.health = this.lastHealth;

        this.transform.position = spawnPosition;

        this.scoreAdditionel = 0;
        StopAllCoroutines();

        GameObject go = Instantiate(respawnParticulePrefab, this.feetPosition.position, Quaternion.identity);
        go.transform.SetParent(this.transform);
        go.transform.rotation = Quaternion.AngleAxis(-90, Vector3.right);
        Destroy(go, 2.5f);

        //respawnParticulePrefab

    }

    IEnumerator Death()
    {
        yield return new WaitForEndOfFrame();
        while (Time.time < this.currentDietime)
        {
            yield return new WaitForEndOfFrame();
            if (entity.IsOwner)
            {
                state.MyColor = Color.Lerp(state.MyColor, Color.black, Time.deltaTime * this.dietime);
            }
            
        }
        
        
        var flash = TakeDamageEvent.Create(entity);
        flash.GetDamage = 0;
        flash.Respawn = true;
        flash.Send();
        yield break;
    }
    
    private Vector3 RespawnPoint()
    {
        int[] point = new int[NetworkCallbacks.SpawnPointsTransforms.Length];
        int currentPoint = 9999999;
        int currentIndex = 0;
        for (int i = 0; i < point.Length; i++)
        {
            Collider[] col = Physics.OverlapSphere(NetworkCallbacks.SpawnPointsTransforms[i].transform.position, 8f);
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
    
    public void CheckVide()
    {
        //if (this.currentInventorySeed < this.maxSeedInInventory)
        {
            Collider[] col = Physics.OverlapSphere(this.feetPosition.position, 1.25f, this.videLayerMask);

            if (col.Length > 0)
            {
                this.TakeDamage(1);
                string s = guardianName + " is lost in the Void !";

                if (this.lastGuardianWhoHitMe != null)
                {
                    s = lastGuardianWhoHitMe.guardianName + " push " + guardianName + " in the void !";
                    lastGuardianWhoHitMe.UpdateScore(false, "Enemy pushed", false);
                }
                
                var evnt = KillFeedEvent.Create(GameSystem.GSystem.entity);
                evnt.Message = s;
                evnt.RemoveFeed = false;
                evnt.Send();
            }
        }

    }

    #endregion

    #region SeedInteraction

    public void SetupLaunchSeed()
    {
        //if (this.currentInventorySeed > 0)
        //if(!this.IsCooldown)
        {
            IsPreLaunchSeed = true;
            this.dirLaunch = this.cameraRef.forward;
        }
        //else
        {
         //   return;
        }
    }

    public void LaunchSeed()
    {
        if (this.currentPillier < this.maxPillier)
        {
            this.IsPreLaunchSeed = false;
            this.currentPillier = this.currentPillier < this.maxPillier ? this.currentPillier + 1 : this.maxPillier;
            this.seedReadyImagesViseur[this.currentPillier - 1].color = this.colorNoReady;
            this.seedReadyUI[this.currentPillier - 1].color = this.colorNoReady;

            if (entity.IsOwner)
            {
                StartCoroutine(LaunchSeedTrueFalse());
            }
            

            this.maskPanel.sizeDelta = new Vector2(this.maskPanel.rect.width, this.maskPanel.rect.height > 0 ? this.maskPanel.rect.height - (this.maxHeightMask / this.maxPillier) : 0);
            //if (this.currentInventorySeed > 0)
            //if (!IsCooldown)
            {
                

                //this.currentInventorySeed--;
            }
            //else
            {
                // return;
            }
        }
            
    }

    IEnumerator LaunchSeedTrueFalse()
    {
        state.LaunchSeed = true;
        yield return new WaitForSeconds(0.15f);

        {
            Seed s = BoltNetwork.Instantiate(BoltPrefabs.Seed, this.myHand.position + this.transform.forward + Vector3.up, Quaternion.identity).GetComponent<Seed>();
            s.Init(this.myTeam, this, this.transform.rotation, true, entity, state.MyColor, this.currentDir);
            s.InitVelocity(this.forceLaunch, this.dirLaunch);

            this.seedReadyImage.color = Color.red;
            
            var evnt = AudioStartEvent.Create(entity);
            evnt.Position = transform.position;
            evnt.AudioID = 1;
            evnt.Send();
        }

        state.LaunchSeed = false;
        yield break;
    }

    public void AddPillierToMyList(Pillier pillierToAdd)
    {
        //if (this.CurrentPillier < this.maxPillier)
        //{
        this.myPillier.Add(pillierToAdd);
        
        if (this.myPillier.Count > this.maxPillier)
            {
                //Destroy(this.myPillier[0].gameObject);
                this.myPillier[0].ActiveDestroy();
                this.myPillier.RemoveAt(0);
            }
        //}
    }

    public void RemovePillier(Pillier pToRemove)
    {
        if(this.myPillier.Contains(pToRemove)) this.myPillier.Remove(pToRemove);
        //this.currentPillier = this.currentPillier > 0 ? this.currentPillier - 1 : 0;
    }

    public void SeedLostInSpace()
    {
        this.currentPillier = this.currentPillier > 0 ? this.currentPillier - 1 : 0;
        this.seedReadyImagesViseur[this.currentPillier].color = this.colorReady;
        this.seedReadyUI[this.currentPillier].color = this.colorReady;
        this.maskPanel.sizeDelta = new Vector2(this.maskPanel.rect.width, this.maskPanel.rect.height < this.maxHeightMask ? this.maskPanel.rect.height + (this.maxHeightMask / this.maxPillier) : this.maxHeightMask);
    }

    public void ChangePillierDir()
    {
        if (this.currentDir == 1)
        {
            this.currentDir = -1;
            //this.sensPillierImage.sprite = this.sensAntiHoraireSprite;
        }
        else
        {
            this.currentDir = 1;
            //this.sensPillierImage.sprite = this.sensHoraireSprite;
        }
    }
    #endregion

    #region FusRoDa

    public void FusRoDa()
    {
        this.IsFusRoDah = true;

        state.FusRoDa = true;

        var evntAudio = AudioStartEvent.Create(entity);
        evntAudio.Position = transform.position;
        evntAudio.AudioID = 2;
        evntAudio.Send();

        switch (mode)
        {
            case FusRoDaMode.Cone:
                Vector3 position = this.startPointFus.position + (this.cameraRef.forward * (this.distanceCheck / 2));
                Vector3 direction = this.CameraRef.rotation * Vector3.forward;
                
                Collider[] guardianColliders = Physics.OverlapSphere(position, this.fusDetectionRadius, this.fusRoDahLayerMask);
               
                if (guardianColliders.Length > 0)
                {
                    foreach (var guard in guardianColliders)
                    {
                        float distance = Vector3.Distance(this.transform.position, guard.transform.position);
                        float angle = Vector3.Angle(this.transform.forward, guard.transform.position - this.transform.position);

                        if (angle <= this.angleMaxToCheckCone && distance <= this.distanceCheck)
                        {
                            if (!Physics.Raycast(this.transform.position, guard.transform.position - this.transform.position,
                                distance, ~this.fusIgnoreLayerMask))
                            {
                                float force = this.forcePush;
                                force = force * (1 - ((Vector3.Distance(this.transform.position, guard.transform.position)) / this.distanceCheck) / this.diviseurDistance);


                                Guardian guardian = guard.GetComponent<Guardian>();
                                if (guardian != null && guardian != this)
                                {
                                    guardian.SetStun(direction.normalized, force, entity);
                                }
                            }

                        }
                    }
                }

                StartCoroutine(CoolDownFus());
                break;

            case FusRoDaMode.Laser:
                this.pointStartLaser.rotation = cameraRef.rotation;
                this.pointEndLaser.localPosition = new Vector3(this.pointEndLaser.localPosition.x, this.pointEndLaser.localPosition.y, this.distanceCheck);
                
                Collider[] laserCol = Physics.OverlapCapsule(this.pointStartLaser.position, this.pointEndLaser.position,
                    this.fusDetectionRadius, this.fusRoDahLayerMask);

                Vector3 dir = this.cameraRef.rotation * Vector3.forward;

                if (laserCol.Length > 0)
                {
                    foreach (var guard in laserCol)
                    {
                        float distance = Vector3.Distance(this.startPointFus.position, guard.transform.position);
                        //float angle = Vector3.Angle(this.transform.forward, guard.transform.position - this.transform.position);

                        if (distance <= this.distanceCheck)
                        {
                            if (!Physics.Raycast(this.startPointFus.position, guard.transform.position - this.transform.position,
                                distance, ~this.fusIgnoreLayerMask))
                            {
                                float force = this.forcePush;
                                force = force * (1 - ((Vector3.Distance(this.startPointFus.position, guard.transform.position)) / this.distanceCheck) / this.diviseurDistance);
                                
                                Guardian guardian = guard.GetComponent<Guardian>();
                                if (guardian != null && guardian != this)
                                {
                                    if (!guardian.IsDie && !guardian.IsStuned && !guardian.IsInvinsible)
                                    {
                                        guardian.SetStun(dir.normalized, force, entity);
                                    }
                                }
                            }

                        }
                    }
                }
                StartCoroutine(CoolDownFus());
                break;
        }

        var evnt = FusRoDaFBEvent.Create(entity);
        evnt.Rotation = this.cameraRef.rotation;
        evnt.Send();
    }

    IEnumerator CoolDownFus()
    {
        //this.pushDispoImage.color = Color.red;
        this.pushDispoViseur.color = this.colorNoReady;
        //this.pushDispoText.text = "x0";
        yield return new WaitForSeconds(0.1f);
        state.FusRoDa = false;
        //this.pushDispoImage.color = Color.Lerp(this.pushDispoImage.color, this.pushReadyColor, Time.deltaTime);
        yield return new WaitForSeconds(this.coolDownFus);
        this.IsFusRoDah = false;
        //this.pushDispoText.text = "x1";
        //this.pushDispoImage.color = this.pushReadyColor;
        this.pushDispoViseur.color = this.pushReadyColor;
        //SetCooldown();
        yield break;
    }

    public override void OnEvent(FusRoDaFBEvent evnt)
    {
        //Switch mode
        ParticleSystem frdParticleSystem =
            Instantiate(this.fusRoDaFeedback, this.startPointFus.position, evnt.Rotation);
        ParticleSystem.ShapeModule shape = frdParticleSystem.shape;
        Destroy(frdParticleSystem.gameObject, 1.2f);
        ParticleSystem.MainModule main = frdParticleSystem.main;
        main.startColor = state.MyColor;
        main = frdParticleSystem.GetComponent<ParticleSystem>().main;
        main.startColor = state.MyColor;

        switch (mode)
        {
            case FusRoDaMode.Cone:
                shape.length = this.distanceCheck;
                shape.angle = this.angleMaxToCheckCone;
                break;

            case FusRoDaMode.Laser:
                shape.length = this.distanceCheck;
                shape.angle = 0f;
                shape.radius = this.fusDetectionRadius;
                main.startSize = this.fusDetectionRadius;
                break;
        }
        
    }

    #endregion

    #region EE

    private void ActiveEasterE()
    {
        if (this.state.GuardianName.Contains("Captain") || this.state.GuardianName.Contains("Dreaa") || this.state.GuardianName.Contains("Slenzy"))
        {
            bwok.SetActive(state.ActiveEE);
            spike.SetActive(state.ActiveEE);
            bouc.SetActive(state.ActiveEE);
        }
        else if (this.state.GuardianName.Contains("Bwok"))
        {
            bwok.SetActive(state.ActiveEE);
        }
        else if (this.state.GuardianName.Contains("Spike"))
        {
            spike.SetActive(state.ActiveEE);
        }
        else if (this.state.GuardianName.Contains("Ves"))
        {
            bouc.SetActive(state.ActiveEE);
        }
        else
        {
            if(entity.IsOwner) state.ActiveEE = false;
        }
    }

    private void RotateBoucCallBack()
    {
        this.bouc.transform.rotation = state.BoucRot;
    }

    #endregion

    public void UpdateScore(bool isMe, string message, bool killLaser)
    {
        if (killLaser && !isMe)
        {
            this.currentPillier = this.currentPillier > 0 ? this.currentPillier - 1 : 0;
            this.seedReadyImagesViseur[this.currentPillier].color = this.colorReady;
            this.seedReadyUI[this.currentPillier].color = this.colorReady;
            this.maskPanel.sizeDelta = new Vector2(this.maskPanel.rect.width, this.maskPanel.rect.height < this.maxHeightMask ? this.maskPanel.rect.height + (this.maxHeightMask / this.maxPillier) : this.maxHeightMask);
        }

        var evnt = UpdateScoreEvent.Create(entity);
        evnt.IsMe = isMe;
        evnt.Message = message;
        evnt.KillLaser = killLaser;
        evnt.ScoreAdd = this.scoreAdditionel;
        evnt.Send();
        
    }

    public override void OnEvent(UpdateScoreEvent evnt)
    {
        if (!evnt.IsMe)
        {
            this.currentKill++;
            this.CurrentSerieKill++;


            int score = 10;//evnt.KillLaser ? 10 : 10;
            


            score = score + evnt.ScoreAdd;

            this.CurrentScore += score;
            //if (entity.IsOwner)
            {
                GameObject go = Instantiate(winLosePointPrefab, this.myCanvas.transform);
                go.transform.parent = winPointPanel;
                go.GetComponent<TextMeshProUGUI>().text = "+ "+ score.ToString()+ " - " + evnt.Message;
                go.GetComponent<TextMeshProUGUI>().color = Color.yellow;
                Destroy(go, 1f);

                if (this.CurrentSerieKill % this.everyXKillIWinMedal == 0 && this.CurrentSerieKill <= this.everyXKillIWinMedal * 6)
                {
                    this.scoreAdditionel = this.CurrentSerieKill <= this.everyXKillIWinMedal * 6 ? this.CurrentSerieKill : 20;
                    GameObject serie = Instantiate(killSeries[this.currentIndexKillSeries], this.myCanvas.transform);
                    Destroy(serie, 1.5f);
                    this.currentIndexKillSeries = this.currentIndexKillSeries+1 < killSeries.Length ? this.currentIndexKillSeries+1 : killSeries
                        .Length - 1;
                }
                else if(this.CurrentSerieKill > this.everyXKillIWinMedal * 6)
                {
                    this.scoreAdditionel = this.CurrentSerieKill <= this.everyXKillIWinMedal * 6 ? this.CurrentSerieKill : 20;
                    GameObject serie = Instantiate(killSeries[this.currentIndexKillSeries], this.myCanvas.transform);
                    Destroy(serie, 1.5f);
                    this.currentIndexKillSeries = this.currentIndexKillSeries + 1 < killSeries.Length ? this.currentIndexKillSeries + 1 : killSeries
                                                                                                                                              .Length - 1;
                    if (this.currentMK == null)
                    {
                        ParticleSystem mk = Instantiate(masterKillPrefab, this.feetPosition.position,
                            Quaternion.identity, this.transform);
                        ParticleSystem.MainModule main = mk.main;
                        main.startColor = state.MyColor;
                        this.currentMK = mk;
                    }
                }
            }

            
        }
        return;

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
        foreach (var rd in renderersAvatar)
        {
            rd.material.SetColor("_ColorEmissive", state.MyColor);
            rd.material.SetFloat("_EmissiveIntensity", 1.5f);
        }


        //GetComponent<Renderer>().material.color = state.MyColor;
    }

    void PlayerName()
    {
        this.guardianName = state.GuardianName;
    }

    void InvinsibleCallBack()
    {
        this.invinsibleFB.SetActive(state.Invinsible);
    }

    IEnumerator EndGameDeleteGuardian()
    {
        yield return new WaitForSeconds(3f);
        if(this.cameraRef != null) Destroy(this.cameraRef.gameObject);
        BoltNetwork.Destroy(this.gameObject);
        yield break;
    }

    /*IEnumerator EndLife()
    {
        yield return new WaitForSeconds(1f);

        if (entity.IsOwner)
        {
            if (this.Life <= 0)
            {
                GameSystem.GSystem.CameraFinal.SetActive(true);
            }

            this.myCanvas.gameObject.SetActive(false);

            this.GetComponent<CharacterControllerManager>().enabled = false;
            this.GetComponent<CameraController>().enabled = false;
            this.GetComponent<CameraInputDetector>().enabled = false;
            this.GetComponent<CharacterInputDetector>().enabled = false;
            this.characterController.enabled = false;

            Destroy(this.cameraRef.gameObject);
            this.cameraRef = null;
            this.transform.position = new Vector3(0, 50, 0);
        }
        
        
        yield break;
    }*/
}

/*private void Death()
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
       IsDie = false;
   }*/
