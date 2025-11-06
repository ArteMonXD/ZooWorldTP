using System;
using UniRx;
using UnityEngine;

public interface IAnimal
{
    AnimalType Type { get; }
    IReadOnlyReactiveProperty<AnimalState> CurrentState { get; }
    GameObject GameObject { get; }
    IObservable<Unit> OnDeath { get; }
    IObservable<Unit> OnSpawn { get; }
    void TakeDamage(int damage);
}