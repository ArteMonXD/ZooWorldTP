using Cysharp.Threading.Tasks;
using System;
using UniRx;
using UnityEngine;
using Zenject;

public class ReactiveAnimalSpawnSystem : IInitializable, ITickable, IDisposable
{
    private readonly DiContainer _container;
    private readonly GameConfig _config;
    private readonly ScreenBoundary _screenBoundary;
    private readonly AnimalRegistry _animalRegistry;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    private readonly ReactiveProperty<float> _spawnTimer = new ReactiveProperty<float>();
    private readonly ReactiveCollection<IAnimal> _activeAnimals = new ReactiveCollection<IAnimal>();
    private readonly Subject<IAnimal> _onAnimalSpawned = new Subject<IAnimal>();
    private readonly Subject<IAnimal> _onAnimalDespawned = new Subject<IAnimal>();
    private readonly ReactiveCommand _spawnCommand = new ReactiveCommand();

    public IReadOnlyReactiveCollection<IAnimal> ActiveAnimals => _activeAnimals;
    public IObservable<IAnimal> OnAnimalSpawned => _onAnimalSpawned;
    public IObservable<IAnimal> OnAnimalDespawned => _onAnimalDespawned;

    public ReactiveAnimalSpawnSystem(DiContainer container, GameConfig config,
        ScreenBoundary screenBoundary, AnimalRegistry animalRegistry)
    {
        _container = container;
        _config = config;
        _screenBoundary = screenBoundary;
        _animalRegistry = animalRegistry;
    }

    public void Initialize()
    {
        Debug.Log("AnimalSpawnSystem: Initializing");
        SetupSpawnTimer();
        SetupAnimalTracking();

        _spawnCommand.Subscribe(_ =>
        {
            SpawnAnimalAsync().Forget();
        });
    }

    private void SetupSpawnTimer()
    {
        Observable.EveryUpdate()
            .Subscribe(_ => _spawnTimer.Value += Time.deltaTime)
            .AddTo(_disposables);

        var spawnInterval = _spawnTimer
            .Select(_ => _activeAnimals.Count)
            .Select(population => Mathf.Lerp(_config.MinSpawnInterval, _config.MaxSpawnInterval, population / 50f))
            .ToReactiveProperty();

        _spawnTimer
            .CombineLatest(spawnInterval, (timer, interval) => (timer, interval))
            .Where(x => x.timer >= x.interval)
            .Subscribe(_ => _spawnCommand.Execute())
            .AddTo(_disposables);
    }

    private void SetupAnimalTracking()
    {
        _activeAnimals.ObserveAdd()
            .Subscribe(addEvent =>
            {
                var animal = addEvent.Value;
                _onAnimalSpawned.OnNext(animal);

                animal.OnDeath
                    .Subscribe(__ => HandleAnimalDeath(animal))
                    .AddTo(_disposables);
            })
            .AddTo(_disposables);
    }

    private async UniTask SpawnAnimalAsync()
    {
        _spawnTimer.Value = 0f;

        var spawnType = GetRandomAnimalType();
        var spawnPosition = await FindSafeSpawnPositionAsync();

        if (spawnPosition.HasValue)
        {
            await CreateAnimalAsync(spawnType, spawnPosition.Value);
        }
    }

    private async UniTask<Vector3?> FindSafeSpawnPositionAsync()
    {
        const int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            var position = _screenBoundary.GetRandomSpawnPosition();

            if (await IsPositionSafeAsync(position))
                return position;

            if (i % 3 == 0)
                await UniTask.NextFrame();
        }

        return null;
    }

    private async UniTask<bool> IsPositionSafeAsync(Vector3 position)
    {
        await UniTask.SwitchToMainThread();

        var checkRadius = 2f;
        var collisions = Physics.OverlapSphere(position, checkRadius);

        foreach (var collider in collisions)
        {
            if (collider.GetComponent<IAnimal>() != null)
                return false;
        }

        return true;
    }

    private async UniTask CreateAnimalAsync(AnimalType type, Vector3 position)
    {
        try
        {
            var prefab = _animalRegistry.GetPrefab(type);

            var animal = _container.InstantiatePrefabForComponent<IAnimal>(
                prefab,
                position,
                Quaternion.identity,
                null,
                new object[0]
            );

            await UniTask.Delay(TimeSpan.FromMilliseconds(100));

            if (!_activeAnimals.Contains(animal))
            {
                _activeAnimals.Add(animal);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to spawn animal: {ex.Message}");
        }
    }

    private void HandleAnimalDeath(IAnimal animal)
    {
        _activeAnimals.Remove(animal);
        _onAnimalDespawned.OnNext(animal);
    }

    private AnimalType GetRandomAnimalType()
    {
        var random = UnityEngine.Random.value;
        return random < _config.PreySpawnWeight ? AnimalType.Prey : AnimalType.Predator;
    }

    public void Tick()
    {
        // Handled by Reactive streams
    }

    public void Dispose()
    {
        _disposables?.Dispose();

        foreach (var animal in _activeAnimals)
        {
            if (animal is IDisposable disposable)
                disposable.Dispose();
        }

        _activeAnimals.Clear();
    }
}
