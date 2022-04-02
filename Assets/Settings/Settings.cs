using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    public const int MAX_PLAYERS = 10;
    public const int MAX_PERKS = 5;

    public static List<AIProfile> aiProfiles = new()
    {
        new() { name = "Васявик" },
        new() { name = "Гачи" },
        new() { name = "Mega Player" },
        new() { name = "Армен" },
        new() { name = "Тень" },
        new() { name = "Shadow Master" },
        new() { name = "Smooth Blade" },
        new() { name = "Cowmarine" },
        new() { name = "Кто здесь?" },
        new() { name = "Капуста" },
        new() { name = "Светильник" },
        new() { name = "Mandalorian" },
        new() { name = "Сумчатый" },
    };
}
