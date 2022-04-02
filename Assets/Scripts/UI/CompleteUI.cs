using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class CompleteUI : MonoBehaviour
{
    [SerializeField] TMP_Text labelVictory;
    [SerializeField] PlayerComplateEntry entryPrefab;
    [SerializeField] Transform parent;
    [SerializeField] Transform lineSeparate;


    readonly List<PlayerEntry> players = new();


    public void Init(CompleteStatus status, List<Perk> allPerks)
    {
        print(status + " ===========================");
        Clear();

        labelVictory.text = status == CompleteStatus.Victory ? "Victory" : "Defeat";

        foreach (var item in GameManager.Instance.allPlayers)
        {
            var entry = new PlayerEntry
            {
                name = item.Nickname,
                team = item.Health.Team,
                countAnniges = item.CountAnniges,
                countAnniged = item.CountAnniged,
                perks = item.usedPerks,
            };
            players.Add(entry);
        }

        foreach (var item in GameManager.Instance.respawnQueue)
        {
            var entry = new PlayerEntry
            {
                name = item.name,
                team = item.team,
                countAnniges = item.countAnniges,
                countAnniged = item.countAnniged,
                perks = item.usedPerks,
            };
            players.Add(entry);
        }

        var teamOne = players.FindAll(p => p.team == Team.One);
        var teamTwo = players.FindAll(p => p.team == Team.Two);

        CreateUIEntryes(teamOne, allPerks);
        CreateUIEntryes(teamTwo, allPerks);
        lineSeparate.SetSiblingIndex(5);
    }

    void CreateUIEntryes(List<PlayerEntry> players, List<Perk> allPerks)
    {
        foreach (var item in players)
        {
            var e = Instantiate(entryPrefab, parent);
            e.Init(item, allPerks);
        }
    }

    void Clear()
    {
        foreach (Transform item in parent)
        {
            if (item != lineSeparate)
                Destroy(item.gameObject);
        }
    }

    public class PlayerEntry
    {
        public string name;
        public Team team;
        public int countAnniges;
        public int countAnniged;
        public List<PerkID> perks;
    }
}


