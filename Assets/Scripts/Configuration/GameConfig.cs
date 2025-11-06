using UnityEngine;

[CreateAssetMenu(menuName = "ZooWorld/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Spawning")]
    public float MinSpawnInterval = 1f;
    public float MaxSpawnInterval = 2f;
    public float PreySpawnWeight = 0.7f;

    [Header("Movement")]
    public MovementConfig FrogMovementConfig;
    public MovementConfig SnakeMovementConfig;
}
