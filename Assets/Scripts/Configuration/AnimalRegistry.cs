using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ZooWorld/AnimalRegistry")]
public class AnimalRegistry : ScriptableObject
{
    [SerializeField] private GameObject frogPrefab;
    [SerializeField] private GameObject snakePrefab;

    public GameObject GetPrefab(AnimalType type) => type switch
    {
        AnimalType.Prey => frogPrefab,
        AnimalType.Predator => snakePrefab,
        _ => throw new ArgumentOutOfRangeException()
    };
}
