using System.Collections;
using System.Collections.Generic;
using Bolt;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GuardianTraining : MonoBehaviour
{
    [Header("General Info")]
    [SerializeField] private int myTeam = 0;
    private int currentKill = 0;
    public int CurrentKill
    {
        get { return currentKill; }
    }
    public int CurrentScore { get; private set; }
    [SerializeField] private CompleteCharacterControllerTraining characterController;
    [SerializeField] private LayerMask videLayerMask;
    public string guardianName { get; private set; }
    Color lastColor = Color.black;
    [SerializeField] private Canvas myCanvas;
    [SerializeField] private GameObject winLosePointPrefab;
    [field:SerializeField] public Transform NamePosition { get; private set; }

    [Header("Player Stats")]
    [SerializeField] private float health = 100f;
    private float lastHealth = 100f;
    //[SerializeField] private int currentInventorySeed = 5;
    //[SerializeField] private int maxSeedInInventory = 5;
    public bool IsStuned { get; private set; }
    [SerializeField] private float stunTime = 5f;
    private float currentStunTime = 0f;
    public bool IsInvinsible { get; private set; }
    [SerializeField] private float invinsibleTime = 2f;
    private float currentInvinsibleTime = 0f;
    //[SerializeField] private TextMeshProUGUI scoreText;
    //[SerializeField] private TextMeshProUGUI bestEnemyScoreText;
    public bool IsDie { get; private set; }
    [SerializeField] private float dietime = 5f;
    private float currentDietime = 0f;
    [SerializeField] private GameObject deathParticulePrefab;

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
    [SerializeField] private AxeTraining myAxe;
    public AxeTraining MyAxe
    {
        get { return myAxe; }
    }
    public bool IsLaunchAxe { get; private set; }

    [Header("Seed Pick Up")]
    [SerializeField] private LayerMask seedLayerMask;
    [SerializeField] private Transform feetPosition;
    [SerializeField] private float checkRadius = 1f;

    [Header("Seed Launch")]
    [SerializeField] private SeedTraining seedT;
    [SerializeField] private Transform cameraRef;
    public Transform CameraRef
    {
        get { return cameraRef; }
    }
    [SerializeField] private float forceLaunch = 10f;
    [SerializeField] private Vector3 dirLaunch;
    public bool IsPreLaunchSeed { get; private set; }
    [SerializeField] private List<PillierTraining> myPillier = new List<PillierTraining>();
    [SerializeField] private int maxPillier = 20;
    private int currentPillier = 0;
    [SerializeField] private bool destroyAllPillierwhenIDie = true;
    [SerializeField] private Transform myHand;
    [SerializeField] private Image seedReadyImage;
    private int currentDir = 1;

    [Header("Pillier Sens")]
    [SerializeField] private Image sensPillierImage;
    [SerializeField] private Sprite sensHoraireSprite;
    [SerializeField] private Sprite sensAntiHoraireSprite;

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
    [SerializeField] private string axeIsBackEvent = "";
    [SerializeField] private FMOD.Studio.EventInstance axeIsBack;

    private void Start()
    {
        IsDie = false;
        IsInvinsible = false;
        IsCooldown = false;
        IsStuned = false;
        this.currentKill = 0;
        this.lastHealth = this.health;
        deathAudioMe = FMODUnity.RuntimeManager.CreateInstance(deathAudioMeEvent);
        deathAudioOther = FMODUnity.RuntimeManager.CreateInstance(deathAudioOtherEvent);
        launchAxeAudio = FMODUnity.RuntimeManager.CreateInstance(launchAxeAudioEvent);
        axeIsBack = FMODUnity.RuntimeManager.CreateInstance(axeIsBackEvent);
    }
    

    private void Update()
    {
        if (this.currentStunTime < Time.time)
        {
            this.IsStuned = false;
        }

        if (this.currentCooldownLaunchSeed < Time.time)
        {
            this.IsCooldown = false;
        }
        else
        {
            this.seedReadyImage.color = Color.Lerp(this.seedReadyImage.color, Color.green,
                Time.deltaTime);
        }

        if (this.currentInvinsibleTime < Time.time)
        {
            this.IsInvinsible = false;
        }
        
    }

    #region MeleeAttack
    GuardianTraining currentEnemyGuardian = null;

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
            /////Son
            launchAxeAudio.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
            launchAxeAudio.start();
            /////Son
            this.myAxe.isCanLaunchAxe(!launch, this.cameraRef.rotation);
        }
        else
        {
            /////Son
            axeIsBack.start();
            /////Son

        }
    }
    
    public void BackToBucheron()
    {
        this.myAxe.ActiveBackToBucheron();
    }
    #endregion

    #region Interaction

    public void TakeDamage(float getDamage)
    {
        this.health -= getDamage;
        this.IsInvinsible = true;
        this.currentInvinsibleTime = Time.time + invinsibleTime;
        if (this.health <= 0)
        {
            Debug.Log("Death");

            deathAudioMe.start();
            
            IsDie = true;
            this.currentDietime = Time.time + this.dietime;
            StartCoroutine(Death());
        }
    }

    public void SetStun(Vector3 dir, float force)
    {
        this.IsStuned = true;
        this.currentStunTime = Time.time + this.stunTime;
        if (this.IsStuned)
        {
            this.characterController.AddForce(dir, force);
        }
    }

    public void SetCooldown()
    {
        this.IsCooldown = true;
        this.currentCooldownLaunchSeed = Time.time + cooldownLaunchSeed;
    }

    private void Respawn()
    {
        if (this.destroyAllPillierwhenIDie)
        {
            foreach (var pillier in this.myPillier)
            {
                Destroy(pillier.gameObject);
            }
            this.myPillier = new List<PillierTraining>();
            this.currentPillier = 0;
        }
        
        var spawnPosition = RespawnPoint();

        this.health = this.lastHealth;

        this.transform.position = spawnPosition;

        IsDie = false;
    }

    IEnumerator Death()
    {
        yield return new WaitForEndOfFrame();
        while (Time.time < this.currentDietime)
        {
            yield return new WaitForEndOfFrame();
            //if (entity.IsOwner)
            {
               // state.MyColor = Color.Lerp(state.MyColor, Color.black, Time.deltaTime * this.dietime);
            }
            
        }
        GameObject go = Instantiate(deathParticulePrefab, this.transform.position, Quaternion.identity);
        Destroy(go, 0.9f);
        Respawn();
        yield break;
    }
    
    private Vector3 RespawnPoint()
    {
        return new Vector3(0, 50, 0);
    }

    public void AddPillierToMyList(PillierTraining pillierToAdd)
    {
        if (this.currentPillier < this.maxPillier)
        {
            this.myPillier.Add(pillierToAdd);
            this.currentPillier++;
            if (this.myPillier.Count > this.maxPillier)
            {
                Destroy(this.myPillier[0].gameObject);
                this.myPillier.RemoveAt(0);
            }
        }
        
    }

    public void RemovePillier(PillierTraining pToRemove)
    {
        this.myPillier.Remove(pToRemove);
        this.currentPillier--;
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
        if (this.currentPillier < this.maxPillier)
        {
            this.IsPreLaunchSeed = false;
            //if (this.currentInventorySeed > 0)
            if (!IsCooldown)
            {
                SeedTraining s = Instantiate(seedT, this.myHand.position + this.transform.forward, Quaternion.identity);
                s.Init(this.myTeam, this, this.transform.rotation, true, this.currentDir);
                s.InitVelocity(this.forceLaunch, this.dirLaunch);

                this.seedReadyImage.color = Color.red;

            }
            else
            {
                // return;
            }
        }
        
    }
    
    public void CheckVide()
    {
        //if (this.currentInventorySeed < this.maxSeedInInventory)
        {
            Collider[] col = Physics.OverlapSphere(this.feetPosition.position, 1.25f, this.videLayerMask);

            if (col.Length > 0)
            {
                this.TakeDamage(1);
                
            }
        }
        //else
        {
           // return;
        }

    }

    public void ChangePillierDir()
    {
        if (this.currentDir == 1)
        {
            this.currentDir = -1;
            this.sensPillierImage.sprite = this.sensAntiHoraireSprite;
        }
        else
        {
            this.currentDir = 1;
            this.sensPillierImage.sprite = this.sensHoraireSprite;
        }
    }
    #endregion
    
    private void SetupTeam(int team)
    {
        this.myTeam = team;
        this.lastHealth = health;
    }

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
