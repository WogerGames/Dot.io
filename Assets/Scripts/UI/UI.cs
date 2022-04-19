using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using Photon.Pun;
using TMPro;
using static Settings;

public class UI : MonoBehaviour
{
    [SerializeField] Joystick joystick;
    [SerializeField] Button btnCam;
    [SerializeField] Transform healthbarParent;
    [SerializeField] ProgressBar progressBarPrefab;
    [SerializeField] ProgressBar citadelHealthBarPrefab;
    [SerializeField] ProgressBar towerHealthBarPrefab;
    [SerializeField] ProgressBar perkProgressBar;
    [SerializeField] RespawnTimerUI respawnTimer;
    [SerializeField] GameObject infoRuneSpawned;
    [SerializeField] GameObject runeEffect;
    [SerializeField] TMP_Text runeTimeLeft;

    [Space]

    [SerializeField] PerkHandlerUI perkHandler;
    [SerializeField] CompleteUI complete;

    readonly Dictionary<Player, ProgressBar> playersHealthBars = new();
    readonly Dictionary<HealthComponent, ProgressBar> healthBars = new();

    public Action<Vector2> onJoystick;
    public Action<PerkID> onPerkClick;

    bool isSleep;

    Player playerMine;
    // HOT FIX
    CompleteStatus status;

    private void Awake()
    {
        isSleep = true;

        complete.gameObject.SetActive(false);
        respawnTimer.gameObject.SetActive(false);
        infoRuneSpawned.SetActive(false);
        runeEffect.SetActive(false);

        EventsHolder.playerSpawnedMine.AddListener(PlayerMine_Spawned);
        EventsHolder.playerSpawnedAny.AddListener(PlayerAny_Spawned);
        EventsHolder.playerAnniged.AddListener(Player_Anniged);
        EventsHolder.onVictory.AddListener(Victory);
        EventsHolder.onDefeat.AddListener(Defeat);
        EventsHolder.profileSeted.AddListener(Profile_Seted);
        EventsHolder.runeSpawned.AddListener(Rune_Spawned);
        EventsHolder.runeTaked.AddListener(Rune_Taked);
        EventsHolder.runeEnded.AddListener(Rune_Ended);

        btnCam.onClick.AddListener(Cam_Clicked);

        perkHandler.onPerkClick += Perk_Clicked;
        respawnTimer.onTimerNotify += RespawnTimer;

        if (MultiplayerManager.IsMaster)
            EventsHolder.clientCompleteDataReceived.AddListener(ClientData_Received);
    }


    private void ClientData_Received()
    {
        LeanTween.delayedCall(1f, () =>
        {
            if (complete)
            {
                complete.gameObject.SetActive(true);
                complete.Init(status, perkHandler.AllPerks);
            }
        });
    }

    private void RespawnTimer()
    {
        if (GameManager.Instance.complete)
            return;

        respawnTimer.gameObject.SetActive(false);
        //joystick.enabled = true;
        joystick.gameObject.SetActive(true);
        joystick.Reset();

        MultiplayerManager.RespawnPlayer();
    }

    private void Profile_Seted(AIProfile profile, Player player)
    {
        if (playersHealthBars.ContainsKey(player))
        {
            var bar = playersHealthBars[player];
            (bar as PlayerHealthBar).nickname.text = profile.name;
        }
    }

    private void Victory(Team team)
    {
        GameComplete(CompleteStatus.Victory);
    }

    private void Defeat(Team team)
    {
        GameComplete(CompleteStatus.Defeat);
    }

    private void GameComplete(CompleteStatus status)
    {
        if (!MultiplayerManager.IsMaster)
            return;

        this.status = status;

        respawnTimer.gameObject.SetActive(false);
        joystick.gameObject.SetActive(false);
        perkHandler.Clear();

        if (PhotonNetwork.CurrentRoom.Players.Count - 1 == 0)
        {
            LeanTween.delayedCall(1f, () =>
            {
                if (complete)
                {
                    complete.gameObject.SetActive(true);
                    complete.Init(status, perkHandler.AllPerks);
                }
            });
        }
    }

    public void NetworkGameComplete(CompleteStatus status)
    {
        respawnTimer.gameObject.SetActive(false);
        joystick.gameObject.SetActive(false);
        perkHandler.Clear();

        complete.gameObject.SetActive(true);
        complete.Init(status, perkHandler.AllPerks);
            
    }

    private void Perk_Clicked(PerkID perkID)
    {
        ClosePerkPanel();

        if (playerMine)
        {
            onPerkClick?.Invoke(perkID);
        }
        else
        {
            var mineRespawnData = GameManager.Instance.respawnQueue.Find
            (
                r =>
                !r.isAI &&
                int.Parse($"{r.viewID}".Substring(0, 1)) == PhotonNetwork.LocalPlayer.ActorNumber
            );
            mineRespawnData.usedPerks.Add(perkID);
        }
    }

    private void Start()
    {
        var citadels = GameManager.Instance.citadels;
        for (int i = 0; i < citadels.Length; i++)
        {
            var b = Instantiate(citadelHealthBarPrefab, healthbarParent);
            healthBars.Add(citadels[i].Health, b);
        }

        var towers = GameManager.Instance.towers;
        for (int i = 0; i < towers.Length; i++)
        {
            var b = Instantiate(towerHealthBarPrefab, healthbarParent);
            healthBars.Add(towers[i].Health, b);
        }


    }

    private void Player_Anniged(Player player)
    {
        if (playersHealthBars.ContainsKey(player))
        {
            var bar = playersHealthBars[player];
            Destroy(bar.gameObject);
            playersHealthBars.Remove(player);
        }

        if (player.GetComponent<BehaviourPlayer>() && player.IsMine() && !GameManager.Instance.complete)
        {
            //joystick.enabled = false;
            joystick.gameObject.SetActive(false);
            joystick.Reset();

            respawnTimer.gameObject.SetActive(true);
            respawnTimer.Init();
        }
    }

    private void PlayerAny_Spawned(Player player)
    {
        var bar = Instantiate(progressBarPrefab, healthbarParent);
        playersHealthBars.Add(player, bar);
        (bar as PlayerHealthBar).nickname.text = player.Nickname;
        //print("ץוכחבאנ סמחהאם");
    }

    private void PlayerMine_Spawned(Player player)
    {
        player.GetComponent<BehaviourPlayer>().SetUI(this);

        playerMine = player;
    }

    private void Rune_Spawned(Rune rune)
    {
        infoRuneSpawned.SetActive(true);
    }

    private void Rune_Taked(Player player)
    {
        infoRuneSpawned.SetActive(false);

        if (player.Nickname == PhotonNetwork.NickName)
        {
            runeEffect.SetActive(true);
        }
    }

    private void Rune_Ended(Player player)
    {
        runeEffect.SetActive(false);
    }

    private void Update()
    {
        if(joystick.Direction != Vector2.zero)
        {
            isSleep = false;
            onJoystick?.Invoke(joystick.Direction);
        }
        else if(!isSleep)
        {
            isSleep = true;
            onJoystick?.Invoke(joystick.Direction);
        }

        UpdateHealthBars();
        UpdatePerkBar();

        if (GameManager.Instance.Rune)
        {
            runeTimeLeft.text = $"{RUNE_DURATION - GameManager.Instance.Rune.currentDuration:F0}";
        }
    }

    public void ShowPerkPanel(List<PerkID> usedPerks)
    {
        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(1.1f);

            var rect = perkProgressBar.GetComponent<RectTransform>();
            LeanTween.value
            (
                gameObject,
                y => 
                { 
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y); 
                },
                0,
                30,
                0.3f
            ).setEaseOutQuad();

            perkHandler.Init(usedPerks);
        }
    }

    public void ClosePerkPanel()
    {
        perkHandler.Clear();
        var rect = perkProgressBar.GetComponent<RectTransform>();
        LeanTween.value
        (
            gameObject,
            y =>
            {
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
            },
            30,
            0,
            0.3f
        ).setEaseOutQuad();
    }

    void UpdateHealthBars()
    {
        foreach (var item in playersHealthBars)
        {
            var bar = item.Value;
            var player = item.Key;

            // HOT FIX
            if (!player)
            {
                if (bar)
                {
                    Destroy(bar.gameObject);
                }
                continue;
            }

            var screenPos = Camera.main.WorldToScreenPoint(player.transform.position);

            int yOffset = Screen.height / 11;
            bar.transform.position = screenPos - new Vector3(0, yOffset, 0);
            bar.value = (float)player.Health.Value / (float)player.Health.MaxValue;

            
        }


        foreach (var item in healthBars)
        {
            var bar = item.Value;
            var health = item.Key;

            // HOT FIX
            if (!health)
            {
                if (bar)
                    Destroy(bar.gameObject);
                continue;
            }

            var screenPos = Camera.main.WorldToScreenPoint(health.transform.position);

            bar.transform.position = screenPos - new Vector3(0, 70, 0);
            bar.value = (float)health.Value / (float)health.MaxValue;
        }
    }

    void UpdatePerkBar()
    {
        if (!playerMine)
            return;

        var curProgress = playerMine.PerkProgress.Evaluate(playerMine.usedPerks.Count);

        perkProgressBar.value = playerMine.Score / curProgress;
    }

    private void Cam_Clicked()
    {
        EventsHolder.onCamClicked?.Invoke();
    }
}
