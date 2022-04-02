using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class HealthComponent : MonoBehaviour
{
    public int MaxValue = 100;

    public int Value { get; private set; }
    public bool IsAlive { get; set; }
    [field: SerializeField]
    public int Evasion { get; set; }
    [field: SerializeField]
    public int Armor { get; set; }
    public int OwnerID { get; set; }

    public Action<int> onAnniged;
    public Action<int, int> onDamage;
    public Action<Team> onTeamSet;

    [field: SerializeField]

    Team team;
    public Team Team 
    {
        get => team;
        set
        {
            team = value;
            onTeamSet?.Invoke(value);
        } 
    }

    public int LastDamagedOwnerID { get; set; }

    private void Start()
    {
        IsAlive = true;

        Value = MaxValue;
    }

    public void Damage(int value, int ownerID, bool silentMode = false)
    {
        if (!silentMode && Random.Range(0, 100) < Evasion)
            return;

        value -= Armor;
        Value -= value;

        LastDamagedOwnerID = ownerID;

        if (!silentMode)
        {
            onDamage?.Invoke(value, ownerID);
        }
    }

    public void Annigilaion(bool silentMode = false)
    {
        Value = 0;

        IsAlive = false;

        onAnniged?.Invoke(LastDamagedOwnerID);
    }

    private void Update()
    {
        if(IsAlive && Value <= 0)
        {
            IsAlive = false;

            onAnniged?.Invoke(LastDamagedOwnerID);
        }
    }
}
