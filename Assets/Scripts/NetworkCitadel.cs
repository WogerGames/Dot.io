using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCitadel : MonoBehaviourPun
{
    Citadel citadel;

    private void Awake()
    {
        citadel = GetComponent<Citadel>();
    }

    private void Start()
    {
        
    }

    private void OnDestroy()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            citadel.Health.Annigilaion();
        }
    }
}
