using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGameData", menuName = "Data/Game/SettingsData", order = 0)]
public class SO_GameSettings : ScriptableObject
{
    [Header("Game Settings")]
    [field: SerializeField] public int maxLives;
    [field: SerializeField] public float[] parallaxSpeed;


    [Header("Paddle Settings")]
    [field: SerializeField] public float paddleSpeed;


    [Header("Ball Settings")]
    [field: SerializeField] public GameObject ballPrefab;
    [field: SerializeField] public float ballSpeed;
    [field: SerializeField, Range(5, 50)] public int ballPoolStartAmount;


    [Header("Brick Settings")]
    [field: SerializeField] public GameObject brickPrefab;
    [field: SerializeField] public BrickStats[] brickStats;


    [Header("PowerUp Settings")]
    [field: SerializeField] public GameObject powerUpPrefab;
    [field: SerializeField] public int powerUpQuantity;

    [field: SerializeField] public MultiballStats multiballStats;
}
