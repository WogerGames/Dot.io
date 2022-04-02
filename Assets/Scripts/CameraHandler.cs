using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCameraBase cam;

    private void Awake()
    {
        EventsHolder.playerSpawnedMine.AddListener(PlayerMine_Spawned);
    }

    private void PlayerMine_Spawned(Player player)
    {
        cam.Follow = player.transform;
        cam.LookAt = player.transform;
    }
}
