using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGameManager : MonoBehaviourPun
{
    


    private void Start()
    {
        EventsHolder.onVictory.AddListener(Game_Completed);

        photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.PlayersAnniges, Network_PlayersAnniges);
        photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.RespawnProfiles, NetworkRespawnProfile_Received);
        photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.RespawnAnniges, NetworkRespawnAnniges_Received);
        photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.RespawnAnniged, NetworkRespawnsAnniged_Received);
        photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.RespawnPerk, NetworkRespawnsPerks_Received);
        photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.PlayersAnniged, NetworkPlayersAnniged_Received);
        photonView.RegisterMethod<int>(EventCode.RespawnDataEnd, NetworkRespawnData_Ended);
    }

    private void NetworkRespawnsPerks_Received(IntOwnerNetworkData data)
    {
        var respawn = GameManager.Instance.respawnQueue.Find(r => r.viewID == data.viewID);
        respawn.usedPerks.Add((PerkID)data.value);
    }

    private void NetworkPlayersAnniged_Received(IntOwnerNetworkData data)
    {
        var player = GameManager.Instance.allPlayers.Find(p => p.photonView.ViewID == data.viewID);
        if (player)
            player.CountAnniged = data.value;
    }

    private void NetworkRespawnsAnniged_Received(IntOwnerNetworkData data)
    {
        var respawn = GameManager.Instance.respawnQueue.Find(r => r.viewID == data.viewID);
        respawn.countAnniged = data.value;
    }

    private void NetworkRespawnData_Ended(int _)
    {
        EventsHolder.onVictory?.Invoke(Team.One);
    }

    private void NetworkRespawnAnniges_Received(IntOwnerNetworkData data)
    {
        var respawn = GameManager.Instance.respawnQueue.Find(r => r.viewID == data.viewID);
        respawn.countAnniges = data.value;
    }

    private void NetworkRespawnProfile_Received(IntOwnerNetworkData data)
    {
        var p = GameManager.Instance.respawnQueue.Find(p => p.viewID == data.viewID);
        if(p == null)
        {
            RespawnData resp = new()
            {
                name = Settings.aiProfiles[data.value].name,
                viewID = data.viewID
            };
            GameManager.Instance.respawnQueue.Add(resp);
        }
    }

    private void Network_PlayersAnniges(IntOwnerNetworkData data)
    {
        var player = GameManager.Instance.allPlayers.Find(p => p.photonView.ViewID == data.viewID);
        if (player)
            player.CountAnniges = data.value;
    }

    private void Game_Completed(Team team)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SendData());
        }

        IEnumerator SendData()
        {
            StartCoroutine(SendPlayersCountAnniges());

            yield return StartCoroutine(SendRespawnProfiles());

            yield return StartCoroutine(SendPlayersCountAnniged());

            yield return StartCoroutine(SendRespawnDataAnniges());

            yield return StartCoroutine(SendRespawnsCountAnniged());

            yield return StartCoroutine(SendRespawnsUsedPerks());

            photonView.RaiseEvent(EventCode.RespawnDataEnd);
        }
    }

    IEnumerator SendPlayersCountAnniges()
    {
        List<IntOwnerNetworkData> packages = new();

        foreach (var item in GameManager.Instance.allPlayers)
        {
            packages.Add(new IntOwnerNetworkData
            {
                value = item.CountAnniges,
                viewID = item.GetComponent<NetworkPlayer>().ViewID
            });
        }

        yield return StartCoroutine(DataSender(packages, EventCode.PlayersAnniges));
    }

    IEnumerator SendPlayersCountAnniged()
    {
        List<IntOwnerNetworkData> packages = new();

        foreach (var item in GameManager.Instance.allPlayers)
        {
            packages.Add(new IntOwnerNetworkData
            {
                value = item.CountAnniged,
                viewID = item.GetComponent<NetworkPlayer>().ViewID
            });
        }

        yield return StartCoroutine(DataSender(packages, EventCode.PlayersAnniged));
    }

    IEnumerator SendRespawnProfiles()
    {
        List<IntOwnerNetworkData> respawnProfiles = new();

        foreach (var item in GameManager.Instance.respawnQueue)
        {
            var profile = Settings.aiProfiles.Find(p => p.name == item.name);
            var idx = Settings.aiProfiles.IndexOf(profile);
            var data = new IntOwnerNetworkData
            {
                value = idx,
                viewID = item.viewID,
            };
            respawnProfiles.Add(data);
        }

        yield return StartCoroutine(DataSender(respawnProfiles, EventCode.RespawnProfiles));
    }
 
    IEnumerator SendRespawnDataAnniges()
    {
        List<IntOwnerNetworkData> respawnProfiles = new();

        foreach (var item in GameManager.Instance.respawnQueue)
        {
            var data = new IntOwnerNetworkData
            {
                value = item.countAnniges,
                viewID = item.viewID,
            };
            respawnProfiles.Add(data);
        }

        yield return StartCoroutine(DataSender(respawnProfiles, EventCode.RespawnAnniges));
    }

    IEnumerator SendRespawnsCountAnniged()
    {
        List<IntOwnerNetworkData> packages = new();

        foreach (var item in GameManager.Instance.respawnQueue)
        {
            packages.Add(new IntOwnerNetworkData
            {
                value = item.countAnniged,
                viewID = item.viewID
            });
        }

        yield return StartCoroutine(DataSender(packages, EventCode.RespawnAnniged));
    }

    IEnumerator SendRespawnsUsedPerks()
    {
        foreach (var respawnPlayer in GameManager.Instance.respawnQueue)
        {
            List<IntOwnerNetworkData> packages = new();

            foreach (var perk in respawnPlayer.usedPerks)
            {
                packages.Add(new IntOwnerNetworkData
                {
                    value = (int)perk,
                    viewID = respawnPlayer.viewID
                });

                yield return StartCoroutine(DataSender(packages, EventCode.RespawnPerk));
            }
        }
    }

    IEnumerator DataSender(List<IntOwnerNetworkData> packages, EventCode code)
    {
        foreach (var item in packages)
        {
            photonView.RaiseEvent(code, item);

            yield return new WaitForSeconds(0.1f);
        }
    }
}
