using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    public const int MAX_PLAYERS = 10;
    public const int MAX_PERKS = 5;
    public const int MIN_RUNE_SPAWN_DELAY = 100;
    public const int MAX_RUNE_SPAWN_DELAY = 170;
    public const int RUNE_DURATION = 30;

    public static List<AIProfile> aiProfiles = new()
    {
        new() { name = "�������" },
        new() { name = "����" },
        new() { name = "Mega Player" },
        new() { name = "�����" },
        new() { name = "����" },
        new() { name = "Shadow Master" },
        new() { name = "Smooth Blade" },
        new() { name = "Cowmarine" },
        new() { name = "��� �����?" },
        new() { name = "�������" },
        new() { name = "����������" },
        new() { name = "Mandalorian" },
        new() { name = "��������" },
        new() { name = "Spotted lynx" },
        new() { name = "���� ���������" },
        new() { name = "�� ������ �����" },
        new() { name = "plunger" },
        new() { name = "-=O_o=-" },
        new() { name = "����� ��������" },
        new() { name = "Toilet bowl - my friend" },
        new() { name = "����� � ����" },
        new() { name = "���� ����" },
        new() { name = "����� ���" },
        new() { name = "Wash your foot" },
        new() { name = "Don't eat the elbow" },
        new() { name = "Skillet" },
        new() { name = "Great Slime" },

    };
}
