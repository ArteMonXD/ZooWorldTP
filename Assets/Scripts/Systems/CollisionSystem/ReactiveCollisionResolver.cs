using Cysharp.Threading.Tasks;
using System;
using UniRx;
using UnityEngine;

public class ReactiveCollisionResolver : ICollisionResolver, IDisposable
{
    private readonly GameStateSystem _gameState;
    private readonly TastyTextSpawner _tastyTextSpawner;
    private readonly CompositeDisposable _disposables = new();

    private readonly Subject<CollisionEvent> _onCollisionResolved = new();
    private readonly ReactiveDictionary<IAnimal, int> _killCounts = new();
    private readonly MessageBroker _messageBroker = new();

    public IObservable<CollisionEvent> OnCollisionResolved => _onCollisionResolved;
    public IReadOnlyReactiveDictionary<IAnimal, int> KillCounts => _killCounts;

    public ReactiveCollisionResolver(GameStateSystem gameState, TastyTextSpawner tastyTextSpawner)
    {
        _gameState = gameState;
        _tastyTextSpawner = tastyTextSpawner;
        SetupCollisionHandling();
    }

    private void SetupCollisionHandling()
    {
        _messageBroker.Receive<AnimalCollisionMessage>()
            .Subscribe(message =>
            {
                UniTask.Void(async () => await ResolveCollisionAsync(message.AnimalA, message.AnimalB));
            })
            .AddTo(_disposables);
    }

    public async UniTask ResolveCollisionAsync(IAnimal animalA, IAnimal animalB)
    {
        if (animalA?.CurrentState.Value == AnimalState.Dead || animalB?.CurrentState.Value == AnimalState.Dead)
            return;

        try
        {
            if (animalA.Type == AnimalType.Prey && animalB.Type == AnimalType.Prey)
            {
                await HandlePreyPreyCollision(animalA, animalB);
            }
            else if (animalA.Type != animalB.Type)
            {
                await HandlePreyPredatorCollision(animalA, animalB);
            }
            else if (animalA.Type == AnimalType.Predator)
            {
                await HandlePredatorPredatorCollision(animalA, animalB);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Collision resolution error: {ex.Message}");
        }
    }

    private async UniTask HandlePreyPredatorCollision(IAnimal animalA, IAnimal animalB)
    {
        Debug.Log("Handle Prey Predator Collision");
        var prey = animalA.Type == AnimalType.Prey ? animalA : animalB;
        var predator = animalA.Type == AnimalType.Predator ? animalA : animalB;

        _tastyTextSpawner.ShowTastyText(predator.GameObject.transform.position);
        await PlayEatingSequenceAsync(prey, predator);

        prey.TakeDamage(100);
        _gameState.IncrementPreyDeaths();

        var collisionEvent = new CollisionEvent(predator, prey, CollisionOutcome.Eating);
        _onCollisionResolved.OnNext(collisionEvent);
    }

    private async UniTask HandlePredatorPredatorCollision(IAnimal predatorA, IAnimal predatorB)
    {
        Debug.Log("Handle Predator Predator Collision");
        var loser = UnityEngine.Random.Range(0, 2) == 0 ? predatorA : predatorB;
        var winner = loser == predatorA ? predatorB : predatorA;

        _tastyTextSpawner.ShowTastyText(winner.GameObject.transform.position);
        await PlayFightSequenceAsync(winner, loser);

        loser.TakeDamage(100);
        _gameState.IncrementPredatorDeaths();

        var collisionEvent = new CollisionEvent(winner, loser, CollisionOutcome.Fight);
        _onCollisionResolved.OnNext(collisionEvent);
    }

    private async UniTask PlayEatingSequenceAsync(IAnimal prey, IAnimal predator)
    {
        var preyMovable = prey.GameObject.GetComponent<IMovable>();
        var predatorMovable = predator.GameObject.GetComponent<IMovable>();

        preyMovable?.StopMovement();
        predatorMovable?.StopMovement();

        var startScale = prey.GameObject.transform.localScale;
        var duration = 0.5f;
        var startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            var progress = (Time.time - startTime) / duration;
            prey.GameObject.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            await UniTask.NextFrame();
        }

        await UniTask.Delay(300);
        predatorMovable?.StartMovement();
    }

    private async UniTask PlayFightSequenceAsync(IAnimal winner, IAnimal loser)
    {
        await UniTask.Delay(500);
    }

    private async UniTask HandlePreyPreyCollision(IAnimal preyA, IAnimal preyB)
    {
        Debug.Log("Handle Prey Prey Collision");
        await ApplyBounceForceAsync(preyA, preyB);

        var collisionEvent = new CollisionEvent(preyA, preyB, CollisionOutcome.Bounce);
        _onCollisionResolved.OnNext(collisionEvent);
    }

    private async UniTask ApplyBounceForceAsync(IAnimal animalA, IAnimal animalB)
    {
        var rbA = animalA.GameObject.GetComponent<Rigidbody>();
        var rbB = animalB.GameObject.GetComponent<Rigidbody>();

        if (rbA == null || rbB == null)
        {
            Debug.LogWarning("One of the animals doesn't have Rigidbody for bounce");
            return;
        }

        // Вычисляем направление отталкивания
        var directionAtoB = (animalB.GameObject.transform.position - animalA.GameObject.transform.position).normalized;

        // Применяем силы в противоположных направлениях
        var bounceForce = 2f; // Сила отталкивания

        rbA.AddForce(-directionAtoB * bounceForce, ForceMode.Impulse);
        rbB.AddForce(directionAtoB * bounceForce, ForceMode.Impulse);

        Debug.Log($"Applied bounce force: {bounceForce}");

        // Короткая пауза чтобы силы успели примениться
        await UniTask.Delay(100);
    }

    public void PublishCollision(IAnimal animalA, IAnimal animalB)
    {
        var message = new AnimalCollisionMessage(animalA, animalB);
        _messageBroker.Publish(message);
    }

    public void Dispose() => _disposables?.Dispose();
}
