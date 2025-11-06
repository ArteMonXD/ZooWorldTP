using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

public class ReactiveUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI preyDeathsText;
    [SerializeField] private TextMeshProUGUI predatorDeathsText;
    [SerializeField] private TextMeshProUGUI populationText;

    private GameStateSystem _gameState;
    private ReactiveAnimalSpawnSystem _spawnSystem;
    private CompositeDisposable _disposables = new();

    [Inject]
    public void Construct(GameStateSystem gameState, ReactiveAnimalSpawnSystem spawnSystem)
    {
        _gameState = gameState;
        _spawnSystem = spawnSystem;
        Debug.Log("UIController constructed successfully");
    }



    private void Start()
    {
        //_gameState.PreyDeaths
        //    .CombineLatest(_gameState.PredatorDeaths, (prey, predator) => (prey, predator))
        //    .Subscribe(counts =>
        //    {
        //        preyDeathsText.text = $"Prey Deaths: {counts.prey}";
        //        predatorDeathsText.text = $"Predator Deaths: {counts.predator}";
        //    })
        //    .AddTo(_disposables);

        //_spawnSystem.ActiveAnimals.ObserveCountChanged()
        //    .Select(count => $"Population: {count}")
        //    .Subscribe(text => populationText.text = text)
        //    .AddTo(_disposables);

        InitializeUI();
    }

    private void InitializeUI()
    {
        // Проверяем ссылки
        if (preyDeathsText == null || predatorDeathsText == null || populationText == null)
        {
            Debug.LogError("UI Text references are not set in inspector!");
            return;
        }

        if (_gameState == null)
        {
            Debug.LogError("GameStateSystem is null!");
            return;
        }

        // Устанавливаем начальные значения
        preyDeathsText.text = "Prey Deaths: 0";
        predatorDeathsText.text = "Predator Deaths: 0";
        populationText.text = "Population: 0";

        // Подписка на изменения с проверкой на null
        if (_gameState.PreyDeaths != null && _gameState.PredatorDeaths != null)
        {
            _gameState.PreyDeaths
                .CombineLatest(_gameState.PredatorDeaths, (prey, predator) => (prey, predator))
                .Subscribe(counts =>
                {
                    preyDeathsText.text = $"Prey Deaths: {counts.prey}";
                    predatorDeathsText.text = $"Predator Deaths: {counts.predator}";
                    Debug.Log($"UI Updated - Prey: {counts.prey}, Predator: {counts.predator}");
                })
                .AddTo(_disposables);
        }
        else
        {
            Debug.LogError("ReactiveProperties are null in GameStateSystem!");
        }

        // Подписка на популяцию с проверкой
        if (_spawnSystem?.ActiveAnimals != null)
        {
            _spawnSystem.ActiveAnimals.ObserveCountChanged()
                .Select(count => $"Population: {count}")
                .Subscribe(text => populationText.text = text)
                .AddTo(_disposables);
        }
    }

    private void OnDestroy() => _disposables.Dispose();

    //[SerializeField] private TextMeshProUGUI preyDeathsText;
    //[SerializeField] private TextMeshProUGUI predatorDeathsText;
    //[SerializeField] private TextMeshProUGUI populationText;

    //private void Start()
    //{
    //    // Простая инициализация без реактивности
    //    preyDeathsText.text = "Prey Deaths: 0";
    //    predatorDeathsText.text = "Predator Deaths: 0";
    //    populationText.text = "Population: 0";
    //}

    //// Вызывайте этот метод из других систем при изменениях
    //public void UpdateStats(int preyDeaths, int predatorDeaths, int population)
    //{
    //    preyDeathsText.text = $"Prey Deaths: {preyDeaths}";
    //    predatorDeathsText.text = $"Predator Deaths: {predatorDeaths}";
    //    populationText.text = $"Population: {population}";
    //}
}
