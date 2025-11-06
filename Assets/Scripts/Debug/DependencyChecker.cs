using UnityEngine;
using Zenject;

public class DependencyChecker : MonoBehaviour
{
    [Inject] private GameStateSystem _gameState;
    [Inject] private ReactiveAnimalSpawnSystem _spawnSystem;
    [Inject] private ReactiveCollisionResolver _collisionResolver;

    private void Start()
    {
        Debug.Log("=== Dependency Check ===");
        Debug.Log($"GameStateSystem: {_gameState != null}");
        Debug.Log($"SpawnSystem: {_spawnSystem != null}");
        Debug.Log($"CollisionResolver: {_collisionResolver != null}");

        if (_gameState != null)
        {
            Debug.Log($"PreyDeaths Property: {_gameState.PreyDeaths != null}");
            Debug.Log($"PredatorDeaths Property: {_gameState.PredatorDeaths != null}");
        }
    }
}
