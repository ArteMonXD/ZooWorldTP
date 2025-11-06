using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [Header("Configuration")]
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private AnimalRegistry animalRegistry;

    [Header("Scene References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TastyTextSpawner tastyTextSpawner;
    [SerializeField] private ReactiveUIController uiController;

    public override void InstallBindings()
    {
        Debug.Log("=== Installing Dependencies ===");

        // Системы
        Container.BindInterfacesAndSelfTo<ReactiveAnimalSpawnSystem>().AsSingle();
        Container.Bind<GameStateSystem>().AsSingle();
        Container.BindInterfacesAndSelfTo<ReactiveCollisionResolver>().AsSingle();
        Container.Bind<ScreenBoundary>().AsSingle();
        Container.Bind<AnimalFactory>().AsSingle();

        // Конфиги и ссылки
        Container.BindInstance(gameConfig);
        Container.BindInstance(animalRegistry);
        Container.BindInstance(mainCamera);
        Container.BindInstance(tastyTextSpawner);
        Container.BindInstance(uiController);

        Debug.Log("=== Dependencies Installed ===");
    }
}
