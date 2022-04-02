using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System;
using static Settings;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] bool spawnCreesp;

    [SerializeField] MultiplayerManager multiplayer;
    [SerializeField] Creep creepPrefab;
    [SerializeField] public Transform[] spawnPoints;
    [SerializeField] public Transform[] spawnCreepPoints;
    [SerializeField] public Citadel[] citadels;
    [SerializeField] public Tower[] towers;

    public static GameManager Instance { get; private set; }

    
    public List<Player> allPlayers = new();
    public List<Creep> allCreeps = new();
    public List<RespawnData> respawnQueue = new();

    public bool complete;

    private Team tima;
    public Team MineTeam 
    {
        get => tima;
        set
        {
            tima = value;
            print("ёба хуёба");
        } 
    }

    private void Awake()
    {
        Instance = this;

        multiplayer.onPlayerSpawned += Player_Spawned;

        EventsHolder.playerSpawnedAny.AddListener(PlayerAny_Spawned);
        EventsHolder.playerAnniged.AddListener(Player_Anniged);
        EventsHolder.creepSpawned.AddListener(Creep_Spawned);
        EventsHolder.creepAnniged.AddListener(Creep_Anniged);
        EventsHolder.mineTeamPicked.AddListener(MineTeam_Picked);

        towers.ToList().ForEach(t => t.Health.onAnniged += Tower_Anniged);
        citadels.ToList().ForEach(t => t.Health.onAnniged += Citadel_Anniged);
    }

    private void Citadel_Anniged(int timaAnnigerID)
    {
        complete = true;

        if (!MultiplayerManager.IsMaster)
            return;

        // Hot Fix
        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(0.1f);

            var citadel = citadels.ToList().Find(c => c);

            if (citadel.Health.Team == MineTeam)
            {
                EventsHolder.onVictory?.Invoke(MineTeam);
            }
            else
            {
                EventsHolder.onDefeat?.Invoke(MineTeam);
            }
        }
    }

    private void MineTeam_Picked(Team team)
    {
        MineTeam = team;
    }

    private void Start()
    {
        StartCoroutine(CreepSpawner());
        StartCoroutine(CheckRespawnQueue());
    }

    private IEnumerator CheckRespawnQueue()
    {
        while (true && MultiplayerManager.IsMaster)
        {
            yield return new WaitForSeconds(0.7f);

            if(respawnQueue.Count > 0 && !complete) 
            {
                var respawnData = respawnQueue.Find(r => Time.time - r.timeAnniged > 10 && r.isAI);
                if (respawnData != null)
                {
                    MultiplayerManager.Instance.SpawnAI(respawnData);

                    respawnQueue.Remove(respawnData);
                }
            }
        }
    }

    private void Tower_Anniged(int annigilatorID)
    {
        print("пязда башне");
    }

    private void Creep_Anniged(Creep creep)
    {
        allCreeps.Remove(creep);
    }

    private void Creep_Spawned(Creep creep)
    {
        allCreeps.Add(creep);
    }

    IEnumerator CreepSpawner()
    {
        yield return new WaitForSeconds(7f);

        while (!complete && spawnCreesp && true)
        {
            yield return new WaitForSeconds(3f);

            if (!MultiplayerManager.IsMaster)
                continue;

            for (int i = 0; i < 4; i++)
            {
                int teamIdx = i % 2;

                var pos = spawnCreepPoints[teamIdx].position;
                pos.x += Random.value;
                pos.z += Random.value; 
                var c = MultiplayerManager.Spawn(creepPrefab, pos);
                c.Health.Team = (Team)teamIdx;
                c.Team_Seted((Team)teamIdx);
            }

            
        }
    }

    private void Player_Anniged(Player player)
    {
        allPlayers.Remove(player);

        RespawnData respawn = new()
        {
            name = player.Nickname,
            isAI = player.GetComponent<BehaviourAI>(),
            viewID = player.GetComponent<NetworkPlayer>().ViewID,
            team = player.Health.Team,
            score = player.Score,
            countAnniges = player.CountAnniges,
            countAnniged = player.CountAnniged,
            usedPerks = player.usedPerks,

            timeAnniged = Time.time,
        };

        bool isMaster = MultiplayerManager.IsMaster;
        bool isMine = !respawn.isAI && player.GetComponent<NetworkPlayer>().IsMine();

        if (isMaster || isMine)
        {
            respawnQueue.Add(respawn);
        }
        
    }

    private void PlayerAny_Spawned(Player player)
    {
        allPlayers.Add(player.GetComponent<Player>());
        print(player.Nickname + " --+-+-+-+-+--+-+");
    }

    private void Player_Spawned(GameObject player)
    {
        var p = player.GetComponent<Player>();
        EventsHolder.playerSpawnedMine?.Invoke(p);
    }

    public HealthComponent FindTarget(HealthComponent health)
    {
        var targets = GetTargets(health);

        float minDistance = float.MaxValue;
        HealthComponent potentialTarget = default;

        foreach (var target in targets)
        {
            var distance = Vector3.Distance(health.transform.position, target.transform.position);
            if (distance < minDistance)
            {
                potentialTarget = target;
                minDistance = distance;
            }
        }

        return potentialTarget;
    }

    public List<HealthComponent> GetTargets(HealthComponent health)
    {
        var targets = allPlayers.Select(p => p.Health).ToList();
        targets.AddRange(citadels.Where(c => c).Select(c => c.Health));
        targets.AddRange(allCreeps.Select(c => c.Health));
        targets.AddRange(towers.Where(t => t).Select(t => t.Health));

        return targets.Where(t => t.Team != health.Team).ToList();
    }
}

[Serializable]
public class RespawnData
{
    public string name;
    public Team team;
    public int viewID;
    public bool isAI;
    public int countAnniges;
    public int countAnniged;
    public int score;
    public List<PerkID> usedPerks = new();

    public float timeAnniged;

    //public int baseDamage;
    //public int critChance;
    //public int maxHealth;
    //public int evasion;
    //public float rateIfFire;
}

public enum CompleteStatus
{
    Victory,
    Defeat
}