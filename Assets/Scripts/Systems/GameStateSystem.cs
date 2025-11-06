using UniRx;
using UnityEngine;
using Zenject;

public class GameStateSystem : IInitializable
{
    private readonly ReactiveProperty<int> _preyDeaths = new();
    private readonly ReactiveProperty<int> _predatorDeaths = new();

    public IReadOnlyReactiveProperty<int> PreyDeaths => _preyDeaths;
    public IReadOnlyReactiveProperty<int> PredatorDeaths => _predatorDeaths;

    public void Initialize()
    {
        _preyDeaths.Value = 0;
        _predatorDeaths.Value = 0;
    }

    public void IncrementPreyDeaths()
    {
        _preyDeaths.Value++;
        Debug.Log($"Prey deaths: {_preyDeaths.Value}");
    }

    public void IncrementPredatorDeaths()
    {
        _predatorDeaths.Value++;
        Debug.Log($"Predator deaths: {_predatorDeaths.Value}");
    }
}
