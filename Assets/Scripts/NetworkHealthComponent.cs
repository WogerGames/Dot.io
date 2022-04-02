using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class NetworkHealthComponent : MonoBehaviourPunCallbacks
{
    HealthComponent health;

    private void Awake()
    {
        health = GetComponent<HealthComponent>();
        health.onDamage += Damaged;

    }

    private void Start()
    {
        photonView.RegisterMethod<IntIntOwnerNetworkData>(EventCode.Damage, Network_Damaged, true);
        photonView.RegisterMethod<IntOwnerNetworkData>(EventCode.TeamIdx, NetworkTeamIdx_Received);

        RaiseTeamIdx();
    }

    private void NetworkTeamIdx_Received(IntOwnerNetworkData data)
    {
        if(photonView.ViewID == data.viewID)
        {
            health.Team = (Team)data.value;
            
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        RaiseTeamIdx();
    }

    void RaiseTeamIdx()
    {
        if (!health.IsAlive)
            return;

        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(0.7f);

            if (gameObject)
            {
                var data = new IntOwnerNetworkData
                {
                    value = (int)health.Team,
                    viewID = photonView.ViewID
                };
                photonView.RaiseEvent(EventCode.TeamIdx, data);
            }
        }

        //LeanTween.delayedCall(gameObject, 0.1f, () =>
        //{
            
        //});
    }

    private void Network_Damaged(IntIntOwnerNetworkData data)
    {
        if(data.viewID == photonView.ViewID)
        {
            health.Damage(data.value_1, data.value_2, true);
        }
    }

    private void Damaged(int value, int ownerID)
    {
        var data = new IntIntOwnerNetworkData 
        { 
            value_1 = value, 
            value_2 = ownerID,
            viewID = photonView.ViewID 
        };

        photonView.RaiseEventAll(EventCode.Damage, data);
    }
}
