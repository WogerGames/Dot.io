using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class BehaviourAI : MonoBehaviour
{
    [SerializeField] List<Perk> allPerks;

    NavMeshAgent agent;
    Transform target;
    Player player;

    public System.Action<int> onProfileSeted;

    public int idxProfile;

    float timerRefrashTarget;

    private void Awake()
    {
        player = GetComponent<Player>();
        agent = GetComponent<NavMeshAgent>();

        player.onPerkAvailable += Perk_Available;
    }

    private void Perk_Available(List<PerkID> usedPerks)
    {
        if (!MultiplayerManager.IsMaster)
            return;

        bool perkFound = false;
        var perk = allPerks[Random.Range(0, allPerks.Count)];
        int countAttempt = 0;

        while (!perkFound)
        {
            var countSamePerk = usedPerks.FindAll(p => p == perk.id).Count;
            if (countSamePerk > perk.countStack)
            {
                perk = allPerks[Random.Range(0, allPerks.Count)];
            }
            else
            {
                perkFound = true;
            }

            countAttempt++;
            if (countAttempt > 10)
                return;
        }

        player.PickPerk(perk.id);
    }

    private void Update()
    {
        UpdateTarget();

        if (target && MultiplayerManager.IsMaster)
        {
            var dist = Vector3.Distance(transform.position, target.position);

            if (dist > player.DistanceToAttack * 0.9f)
            {
                agent.SetDestination(target.position);
            }
            else
            {
                agent.SetDestination(transform.position);
                player.Fire();
            }
        }
        else
        {
            target = GameManager.Instance.FindTarget(player.Health)?.transform;
            agent.SetDestination(transform.position);
        }
    }

    void UpdateTarget()
    {
        timerRefrashTarget += Time.deltaTime;
        if (timerRefrashTarget > 1.5f)
        {
            target = GameManager.Instance.FindTarget(player.Health)?.transform;
            timerRefrashTarget = 0;
        }
    }

    public void SetProfile(AIProfile profile, int idx, bool silentMode = false)
    {
        if (!player || profile == null)
            return;

        player.Nickname = profile.name;

        idxProfile = idx;

        EventsHolder.profileSeted?.Invoke(profile, player);

        if (!silentMode)
            onProfileSeted?.Invoke(idx);
    }
    
}
