using UnityEngine;
using Zenject;

public class AnimalFactory
{
    private readonly DiContainer _container;
    private readonly AnimalRegistry _registry;

    public AnimalFactory(DiContainer container, AnimalRegistry registry)
    {
        _container = container;
        _registry = registry;
    }

    public IAnimal Create(AnimalType type, Vector3 position, Quaternion rotation, Transform parent)
    {
        var prefab = _registry.GetPrefab(type);
        var gameObject = _container.InstantiatePrefab(prefab, position, rotation, parent);
        return gameObject.GetComponent<IAnimal>();
    }
}
