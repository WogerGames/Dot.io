using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using System;

public class Player : MonoBehaviourPun
{
    [SerializeField] AnimationCurve perkProgress;
    [SerializeField] float speed = 7f;
    [SerializeField] public float rateOfFire = 0.5f;
    [SerializeField] float distToAtttack = 5f;
    [SerializeField] Projectile projectilePrefab;
    [SerializeField] HealthComponent health;
    [SerializeField] TMPro.TMP_Text testo;

    [Space]

    [SerializeField] MeshRenderer teamIndicator;
    [SerializeField] Material friendly;
    [SerializeField] Material enemy;

    public string Nickname { get; set; }

    public Action<List<PerkID>> onPerkAvailable;
    public Action<PerkID> onPerkPick;

    public List<PerkID> usedPerks = new();
    [field: SerializeField]
    public int Score { get; set; }
    [field: SerializeField]
    public int CountAnniges { get; set; }
    public int CountAnniged { get; set; }
    public float DistanceToAttack => distToAtttack;
    public HealthComponent Health => health;
    public AnimationCurve PerkProgress => perkProgress;
    [field: SerializeField]
    public int CritChance { get; set; } = 5;

    public int baseDamage = 1;

    HealthComponent target;
    float currentRate;
    float findTargetTimer;
    bool annigedNotifed;
    
    private void Start()
    {
        EventsHolder.playerSpawnedAny?.Invoke(this);

        name += photonView.ViewID;

        Health.onAnniged += Anniged;
        Health.onTeamSet += Team_Seted;
        Health.OwnerID = photonView.ViewID;

        EventsHolder.playerAnniged.AddListener(OtherPlayer_Anniged);
    }

    public void Team_Seted(Team team)
    {
        //print($"���� ������� {team}");
        if(GameManager.Instance.MineTeam == team)
        {
            teamIndicator.material = friendly;
        }
        else
        {
            teamIndicator.material = enemy;
        }
    }

    private void OtherPlayer_Anniged(Player annigedPlayer)
    {
        var isMeAnnigilator = photonView.ViewID == annigedPlayer.Health.LastDamagedOwnerID;
        
        if (isMeAnnigilator && photonView.IsMine)
        {
            CountAnniges++;
            Score += 5;

            CheckAvailablePerk();
        }
    }

    void CheckAvailablePerk()
    {
        var thresold = perkProgress.Evaluate(usedPerks.Count);

        if(Score >= thresold && !GameManager.Instance.complete && usedPerks.Count < Settings.MAX_PERKS)
        {
            Score = 0;

            onPerkAvailable?.Invoke(usedPerks);
        }
    }

    public void PickPerk(PerkID perk, bool silentMode = false)
    {
        switch (perk)
        {
            case PerkID.Attack:
                baseDamage += 3;
                break;
            case PerkID.AttackPower:
                baseDamage += 5;
                break;
            case PerkID.Health:
                Health.MaxValue += 50;
                Health.Damage(-50, photonView.ViewID);
                break;
            case PerkID.FireRate:
                rateOfFire -= 0.08f;
                break;
            case PerkID.Evasion:
                Health.Evasion += 30;
                break;
            case PerkID.CritChance:
                CritChance += 10;
                break;
            case PerkID.CritChancePower:
                CritChance += 30;
                break;
            case PerkID.Armor:
                Health.Armor += 3;
                break;
            default:
                break;
        }

        usedPerks.Add(perk);
       
        if (!silentMode)
            onPerkPick?.Invoke(perk);

        print($"�� ����� ������ {perk}");
    }

    private void Anniged(int annigilatorID)
    {
        CountAnniged++;
        EventsHolder.playerAnniged?.Invoke(this);
        annigedNotifed = true;

        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(photonView);
        }
    }

    public void Move(Vector2 value)
    {
        transform.position += speed * Time.deltaTime * new Vector3(value.x, 0, value.y);
    }

    private void Update()
    {
        currentRate += Time.deltaTime;
        findTargetTimer += Time.deltaTime;

        CheckNearestTarget();

        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                print(target + " =====================");
            }
        }

        if(testo)
        testo.text = CritChance.ToString();
    }

    public void Fire()
    {
        if (target && currentRate > rateOfFire)
        {
            var dir = (target.transform.position - transform.position).normalized;
            var pos = transform.position - new Vector3(0, 0.75f, 0);
            var go = PhotonNetwork.Instantiate(projectilePrefab.name, pos, Quaternion.identity);

            var projectileData = new ProjectileData
            {
                direction = dir,
                baseDamege = baseDamage,
                critChance = CritChance,
                ownerID = photonView.ViewID,
                team = Health.Team,
            };
            go.GetComponent<Projectile>().Init(projectileData);
            currentRate = 0;
            
        }
        else
        {
            FindTarget();
        }
    }

    void FindTarget()
    {
        var targets = GameManager.Instance.allPlayers.Select(p => p.health).ToList();
        var citadel = GameManager.Instance.citadels.ToList().Find(c => c.Health.Team != Health.Team);
        var tower = GameManager.Instance.towers.ToList().Find(c => c.Health.Team != Health.Team);
        targets.Add(citadel.Health);
        targets.AddRange(GameManager.Instance.allCreeps.Select(c => c.Health));
        targets.Add(tower.Health);

        float minDist = float.MaxValue;
        HealthComponent potentialTarget = default;
        
        foreach (var h in targets)
        {
            if (health == h)
                continue;
            
            if (h.Team == Health.Team)
                continue;

            //HOT FIX
            if (!h)
                continue;

            var dist = Vector3.Distance(h.transform.position, transform.position);
            
            if (dist > distToAtttack)
                continue;

            if (dist < minDist)
            {
                potentialTarget = h;
                minDist = dist;
            }
            
        }
        
        target = potentialTarget;

    }

    public void SetNickname(string value)
    {
        Nickname = value;
        //onSetNickname?.Invoke
    }

    private void OnDestroy()
    {
        if (!photonView.IsMine && !annigedNotifed)
        {
            EventsHolder.playerAnniged?.Invoke(this);
        }
    }

    void CheckNearestTarget()
    {
        if(findTargetTimer > 1f)
        {
            FindTarget();

            findTargetTimer = 0;
        }
    }
}

public enum Team
{
    One = 0, 
    Two = 1,
}
