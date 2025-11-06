using Cysharp.Threading.Tasks;
using System;
using UniRx;

public interface ICollisionResolver : IDisposable
{
    IObservable<CollisionEvent> OnCollisionResolved { get; }
    IReadOnlyReactiveDictionary<IAnimal, int> KillCounts { get; }
    UniTask ResolveCollisionAsync(IAnimal animalA, IAnimal animalB);
    void PublishCollision(IAnimal animalA, IAnimal animalB);
}
