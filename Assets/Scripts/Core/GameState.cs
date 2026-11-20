using System;
using UnityEngine;

/// <summary>
/// Serializable runtime progress. Kept separate from ScriptableObject configs.
/// Ready for future JSON save/load without changing gameplay code.
/// </summary>
[Serializable]
public class GameState
{
    public const int MinWeaponPower = 1;
    public const int MaxWeaponPower = 3;
    public const int DefaultLives = 3;
    public const int DefaultBombs = 2;

    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentRound = 1;
    [SerializeField] private int lives = DefaultLives;
    [SerializeField] private int score;
    [SerializeField] private int bombCount = DefaultBombs;
    [SerializeField] private int weaponPowerLevel = MinWeaponPower;

    public int CurrentLevel => currentLevel;
    public int CurrentRound => currentRound;
    public int Lives => lives;
    public int Score => score;
    public int BombCount => bombCount;
    public int WeaponPowerLevel => weaponPowerLevel;

    public void ResetToDefaults()
    {
        currentLevel = 1;
        currentRound = 1;
        lives = DefaultLives;
        score = 0;
        bombCount = DefaultBombs;
        weaponPowerLevel = MinWeaponPower;
    }

    public void SetLevel(int level)
    {
        currentLevel = Mathf.Max(1, level);
    }

    public void SetRound(int round)
    {
        currentRound = Mathf.Max(1, round);
    }

    public void SetLives(int value)
    {
        lives = Mathf.Max(0, value);
    }

    public void AddScore(int amount)
    {
        score = Mathf.Max(0, score + amount);
    }

    public void SetScore(int value)
    {
        score = Mathf.Max(0, value);
    }

    public void SetBombCount(int value)
    {
        bombCount = Mathf.Max(0, value);
    }

    public void SetWeaponPowerLevel(int value)
    {
        weaponPowerLevel = Mathf.Clamp(value, MinWeaponPower, MaxWeaponPower);
    }

    public GameState Clone()
    {
        return new GameState
        {
            currentLevel = currentLevel,
            currentRound = currentRound,
            lives = lives,
            score = score,
            bombCount = bombCount,
            weaponPowerLevel = weaponPowerLevel
        };
    }
}
