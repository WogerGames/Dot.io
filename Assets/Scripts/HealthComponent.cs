using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class HealthComponent : MonoBehaviour
{
    public int MaxValue = 100;
    [field: SerializeField]
    public int Value { get; private set; }
    public bool IsAlive { get; set; }
    [field: SerializeField]
    public int Evasion { get; set; }
    [field: SerializeField]
    public int Armor { get; set; }
    public int OwnerID { get; set; }

    public Action<int> onAnniged;
    public Action<int> onIncreaseHealth;
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
    public int AnnigedTime { get; set; }

    private void Start()
    {
        IsAlive = true;

        Value = MaxValue;
    }

    public void Damage(int value, int ownerID)
    {
        if (Random.Range(0, 100) < Evasion)
            return;

        value -= Armor;
        Value -= value;

        LastDamagedOwnerID = ownerID;

        onDamage?.Invoke(value, ownerID);        
    }

    public void DamageNetworkReceived(int value, int ownerID, int damageTime)
    {
        Value -= value;
        LastDamagedOwnerID = ownerID;
    }

    public void IncreaseMaxHealth(int value, bool networkMode = false)
    {
        MaxValue += value;
        Value += value;
        Value = Mathf.Clamp(Value, 0, MaxValue);
    }

    public void DecreaseMaxHealth(int value)
    {
        MaxValue -= value;
        if (Value > MaxValue)
        {
            Value = MaxValue;
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
        if (IsAlive && Value <= 0)
        {
            IsAlive = false;

            AnnigedTime = MultiplayerManager.ServerTime;


            StartCoroutine(WaitClientAnnigedData());

            IEnumerator WaitClientAnnigedData()
            {
                // � ������� ������������� ����� ������ ���� �����
                // ������� �������� ���� �� ����������� � ������������
                // ��������� ����� ����� �������
                yield return new WaitForSeconds(0.3f);

                onAnniged?.Invoke(LastDamagedOwnerID);
            }

        }
    }
}
