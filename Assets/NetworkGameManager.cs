using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NetworkGameManager : MonoBehaviourPun
{
    int countClientReceivedAnniges = 0;

    private void Start()
    {
        EventsHolder.onVictory.AddListener(Game_Completed);
        EventsHolder.onDefeat.AddListener(Game_Completed);
        EventsHolder.clientGameCompleted.AddListener(ClientGame_Completed);

        photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.PlayersAnniges, Network_PlayersAnniges);
        photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.RespawnProfiles, NetworkRespawnProfile_Received);
        photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.RespawnPlayer, NetworkRespawnPlayer_Received);
        photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.RespawnAnniges, NetworkRespawnAnniges_Received);
        photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.RespawnAnniged, NetworkRespawnsAnniged_Received);
        photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.RespawnPerk, NetworkRespawnsPerks_Received);
        photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.RespawnTeam, NetworkRespawnsTeam_Received);
        photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.PlayersAnniged, NetworkPlayersAnniged_Received);
        photonView.RegisterMethod<int>(EventCode.RespawnDataEnd, NetworkRespawnData_Ended);

        photonView.RegisterMethod<IntIntOwnerNetworkData>(EventCode.ClientCompleteData, NetworkClientAnniges_Received, true);
    }

    private void ClientGame_Completed()
    {
        var player = GameManager.Instance.allPlayers.Find(p => p.Nickname == PhotonNetwork.NickName);
        if (player)
        {
            IntIntOwnerNetworkData data = new()
            {
                value_1 = player.CountAnniges,
                value_2 = player.CountAnniged,
                viewID = player.photonView.ViewID,
            };

            StartCoroutine(SendClientData(data, EventCode.ClientCompleteData));
        }
        else
        {
            var respawnData = GameManager.Instance.respawnQueue.Find(r => r.name == PhotonNetwork.NickName);
            // MEGA HOT FIX
            if (respawnData == null)
                return;

            IntIntOwnerNetworkData data = new()
            {
                value_1 = respawnData.countAnniges,
                value_2 = respawnData.countAnniged,
                viewID = respawnData.viewID,
            };

            StartCoroutine(SendClientData(data, EventCode.ClientCompleteData));
        }

        IEnumerator SendClientData(IntIntOwnerNetworkData data, EventCode eventCode)
        {
            yield return new WaitForSeconds(3f);

            photonView.RaiseEventAll(eventCode, data);
            print("Зарайзил");
        }
    }

    private void NetworkClientAnniges_Received(IntIntOwnerNetworkData data)
    {
        var player = GameManager.Instance.allPlayers.Find(p => p.photonView.ViewID == data.viewID);
        if (player)
        {
            player.CountAnniges = data.value_1;
            player.CountAnniged = data.value_2;
        }
        else
        {
            var respawn = GameManager.Instance.respawnQueue.Find(r => r.viewID == data.viewID);
            respawn.countAnniges = data.value_1;
            respawn.countAnniged = data.value_2;
        }

        countClientReceivedAnniges++;

        var leftMsg = PhotonNetwork.CurrentRoom.Players.Count - 1 - countClientReceivedAnniges;
        print($"осталось получить {leftMsg} месажей");

        if(leftMsg == 0)
        {
            EventsHolder.clientCompleteDataReceived?.Invoke();
        }
    }

    private void NetworkRespawnsTeam_Received(IntOwnerNetworkData data)
    {
        var respawnData = GameManager.Instance.respawnQueue.Find(r => r.viewID == data.viewID);
        respawnData.team = (Team)data.value;
    }

    private void NetworkRespawnPlayer_Received(IntOwnerNetworkData data)
    {
        var player = GameManager.Instance.allPlayers.Find(p => p.photonView.ViewID == data.viewID);
        if(player == null)
        {
            foreach (var item in PhotonNetwork.CurrentRoom.Players)
            {
                if (item.Value.ActorNumber == data.value)
                {
                    var respawn = new RespawnData
                    {
                        name = item.Value.NickName,
                        viewID = data.viewID,
                    };
                    GameManager.Instance.respawnQueue.Add(respawn);
                }
            }
        }
    }

    private void NetworkRespawnsPerks_Received(IntOwnerNetworkData data)
    {
        var respawn = GameManager.Instance.respawnQueue.Find(r => r.viewID == data.viewID);
        respawn.usedPerks.Add((PerkID)data.value);
    }

    private void NetworkPlayersAnniged_Received(IntOwnerNetworkData data)
    {
        var player = GameManager.Instance.allPlayers.Find(p => p.photonView.ViewID == data.viewID);
        if (player && player.Nickname != PhotonNetwork.LocalPlayer.NickName)
            player.CountAnniged = data.value;

        
    }

    private void NetworkRespawnsAnniged_Received(IntOwnerNetworkData data)
    {
        var respawn = GameManager.Instance.respawnQueue.Find(r => r.viewID == data.viewID);
        respawn.countAnniged = data.value;
    }

    private void NetworkRespawnData_Ended(int _)
    {
        var citadel = GameManager.Instance.citadels.ToList().Find(c => c);

        var ui = FindObjectOfType<UI>();

        if (citadel.Health.Team == GameManager.Instance.MineTeam)
        {
            EventsHolder.onVictory?.Invoke(GameManager.Instance.MineTeam);
            ui.NetworkGameComplete(CompleteStatus.Victory);
        }
        else
        {
            EventsHolder.onDefeat?.Invoke(GameManager.Instance.MineTeam);
            ui.NetworkGameComplete(CompleteStatus.Defeat);
        }


        EventsHolder.onVictory?.Invoke(Team.One);
    }

    private void NetworkRespawnAnniges_Received(IntOwnerNetworkData data)
    {
        var respawn = GameManager.Instance.respawnQueue.Find(r => r.viewID == data.viewID);
        respawn.countAnniges = data.value;
    }

    private void NetworkRespawnProfile_Received(IntOwnerNetworkData data)
    {
        print("профиль id " + data.value);
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
        if (player && player.Nickname != PhotonNetwork.LocalPlayer.NickName)
            player.CountAnniges = data.value;
    }

    private void Game_Completed(Team team)
    {
        print("=============================");
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SendData());
        }

        IEnumerator SendData()
        {
            yield return StartCoroutine(SendRespawnProfiles());

            yield return StartCoroutine(SendRespawnPlayer());

            yield return StartCoroutine(SendPlayersCountAnniged());

            yield return StartCoroutine(SendRespawnDataAnniges());

            yield return StartCoroutine(SendRespawnsCountAnniged());

            yield return StartCoroutine(SendRespawnsUsedPerks());

            yield return StartCoroutine(SendRespawnsTeam());

            yield return StartCoroutine(SendPlayersCountAnniges());

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
            if (!item.isAI)
                continue;

            var profile = Settings.aiProfiles.Find(p => p.name == item.name);
            var idx = Settings.aiProfiles.IndexOf(profile);
            var data = new IntOwnerNetworkData
            {
                value = idx,
                viewID = item.viewID,
            };
            respawnProfiles.Add(data);
        }

        yield return StartCoroutine(DataSender(respawnProfiles, EventCode.RespawnProfiles, 0.3f));
    }

    IEnumerator SendRespawnPlayer()
    {
        var player = GameManager.Instance.respawnQueue.Find(r => r.name == PhotonNetwork.NickName);

        if(player != null)
        {
            var data = new IntOwnerNetworkData
            {
                value = PhotonNetwork.LocalPlayer.ActorNumber,
                viewID = player.viewID,
            };

            yield return StartCoroutine(DataSender(new() { data }, EventCode.RespawnPlayer));
        }
    }

    IEnumerator SendRespawnDataAnniges()
    {
        List<IntOwnerNetworkData> respawnProfiles = new();

        foreach (var respawnPlayer in GameManager.Instance.respawnQueue)
        {
            if (!respawnPlayer.isAI && respawnPlayer.name != PhotonNetwork.NickName)
                continue;

            var data = new IntOwnerNetworkData
            {
                value = respawnPlayer.countAnniges,
                viewID = respawnPlayer.viewID,
            };
            respawnProfiles.Add(data);
        }

        yield return StartCoroutine(DataSender(respawnProfiles, EventCode.RespawnAnniges));
    }

    IEnumerator SendRespawnsCountAnniged()
    {
        List<IntOwnerNetworkData> packages = new();

        foreach (var respawnPlayer in GameManager.Instance.respawnQueue)
        {
            if (!respawnPlayer.isAI && respawnPlayer.name != PhotonNetwork.NickName)
                continue;

            packages.Add(new IntOwnerNetworkData
            {
                value = respawnPlayer.countAnniged,
                viewID = respawnPlayer.viewID
            });
        }

        yield return StartCoroutine(DataSender(packages, EventCode.RespawnAnniged));
    }

    IEnumerator SendRespawnsUsedPerks()
    {
        foreach (var respawnPlayer in GameManager.Instance.respawnQueue)
        {
            if (!respawnPlayer.isAI && respawnPlayer.name != PhotonNetwork.NickName)
                continue;

            List<IntOwnerNetworkData> packages = new();

            foreach (var perk in respawnPlayer.usedPerks)
            {
                packages.Add(new IntOwnerNetworkData
                {
                    value = (int)perk,
                    viewID = respawnPlayer.viewID
                });
            }

            yield return StartCoroutine(DataSender(packages, EventCode.RespawnPerk));
        }
    }

    IEnumerator SendRespawnsTeam()
    {
        List<IntOwnerNetworkData> packages = new();

        foreach (var respawnData in GameManager.Instance.respawnQueue)
        {
            packages.Add(new()
            {
                value = (int)respawnData.team,
                viewID = respawnData.viewID
            });
        }

        yield return StartCoroutine(DataSender(packages, EventCode.RespawnTeam));
    }

    IEnumerator DataSender(List<IntOwnerNetworkData> packages, EventCode code, float delayBetweenRequest = 0.1f)
    {
        foreach (var item in packages)
        {
            photonView.RaiseEvent(code, item);

            yield return new WaitForSeconds(delayBetweenRequest);
        }
    }
}
