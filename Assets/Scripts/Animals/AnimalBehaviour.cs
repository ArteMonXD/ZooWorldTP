using System;
using UniRx;
using UnityEngine;
using Zenject;

public abstract class AnimalBehaviour : MonoBehaviour, IAnimal, IDisposable
{
    [SerializeField] protected AnimalType animalType;

    protected readonly ReactiveProperty<AnimalState> _currentState = new ReactiveProperty<AnimalState>(AnimalState.Alive);
    protected readonly ReactiveProperty<int> _health = new ReactiveProperty<int>(100);
    protected readonly Subject<Unit> _onDeath = new Subject<Unit>(); 
    protected readonly Subject<Unit> _onSpawn = new Subject<Unit>(); 
    protected readonly CompositeDisposable _disposables = new CompositeDisposable();

    public AnimalType Type => animalType;
    public IReadOnlyReactiveProperty<AnimalState> CurrentState => _currentState;
    public IObservable<Unit> OnDeath => _onDeath;
    public IObservable<Unit> OnSpawn => _onSpawn;
    public GameObject GameObject => gameObject;

    protected IMovable MovementController => GetComponent<IMovable>();
    [Inject] protected ICollisionResolver CollisionResolver;

    protected virtual void Start()
    {
        Debug.Log($"{GetType().Name}: Start called, MovementController: {MovementController != null}");

        // ВЫЗЫВАЕМ событие спавна ДО настройки реакций
        _onSpawn.OnNext(Unit.Default);
        Debug.Log($"{GetType().Name}: Spawn event triggered");

        SetupStateReactions();
    }

    private void SetupStateReactions()
    {
        Debug.Log($"{GetType().Name}: Setup State Reactions");

        _currentState.Pairwise()
            .Subscribe(pair => HandleStateTransition(pair.Previous, pair.Current))
            .AddTo(_disposables);

        _health.Where(health => health <= 0)
            .Subscribe(_ => _currentState.Value = AnimalState.Dead)
            .AddTo(_disposables);

        _currentState.Where(state => state == AnimalState.Dead)
            .Delay(TimeSpan.FromSeconds(1))
            .Subscribe(_ => Dispose())
            .AddTo(_disposables);

        Debug.Log($"{GetType().Name}: Setting up movement subscription");

        Observable.NextFrame()
            .Subscribe(_ =>
            {
                StartMovement();
            })
            .AddTo(_disposables);

        //Observable.Timer(TimeSpan.FromMilliseconds(100))
        //    .Subscribe(_ =>
        //    {
        //        StartMovement();
        //    })
        //    .AddTo(_disposables);
    }

    private void StartMovement()
    {
        Debug.Log($"{GetType().Name}: Starting movement directly");
        if (MovementController != null)
        {
            MovementController.StartMovement();
            Debug.Log($"{GetType().Name}: Movement started successfully");
        }
        else
        {
            Debug.LogError($"{GetType().Name}: MovementController is null!");
        }
    }

    protected virtual void HandleStateTransition(AnimalState from, AnimalState to)
    {
        Debug.Log($"{GetType().Name}: State changed from {from} to {to}");

        if (to == AnimalState.Dead)
        {
            MovementController?.StopMovement();
            _onDeath.OnNext(Unit.Default);
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"{GetType().Name}: Taking damage: {damage}");
        _health.Value -= damage;
    }

    public void Dispose()
    {
        Debug.Log($"{GetType().Name}: Dispose");
        _disposables?.Dispose();
        _onDeath?.OnCompleted();
        _onSpawn?.OnCompleted();
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
