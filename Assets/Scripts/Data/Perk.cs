using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Perk", menuName = "GAM_XYIAM/Perk_Xyerk")]
public class Perk : ScriptableObject
{
    public PerkID id;
    public Sprite icon;
    [TextArea]
    public string description;
    public int countStack;

    private void OnEnable()
    {
        
    }
}

public enum PerkID
{
    Attack = 0,
    Health = 1,
    FireRate = 2,
    Evasion = 3,
    CritChance = 4,
    AttackPower = 5,
    CritChancePower = 6,
    Armor = 7,
}
