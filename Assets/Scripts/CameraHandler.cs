using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraHandler : MonoBehaviour
{
    [SerializeField] UniversalRenderPipelineAsset urpSettings;
    [SerializeField] CinemachineVirtualCameraBase cam;

    [SerializeField] Vector3[] camOffsets;
    [SerializeField] float[] shadowDistances;

    int idxOffset = 0;

    private void Awake()
    {
        EventsHolder.playerSpawnedMine.AddListener(PlayerMine_Spawned);
        EventsHolder.onCamClicked.AddListener(Cam_Clicked);
    }

    private void Cam_Clicked()
    {
        if (Screen.height > Screen.width)
        {

        }

        idxOffset++;
        if(idxOffset >= camOffsets.Length)
        {
            idxOffset = 0;
        }

        var c = cam as CinemachineVirtualCamera;
        var transposer = c.GetCinemachineComponent<CinemachineTransposer>();

        var start = transposer.m_FollowOffset;
        LeanTween.value(gameObject, v =>
        {
            transposer.m_FollowOffset = v;
        }, start, camOffsets[idxOffset], 0.5f).setEaseInOutQuad();

        urpSettings.shadowDistance = shadowDistances[idxOffset];
    }

    private void PlayerMine_Spawned(Player player)
    {
        cam.Follow = player.transform;
        cam.LookAt = player.transform;
    }
}
