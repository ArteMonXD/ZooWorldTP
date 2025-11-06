using UnityEngine;
using Zenject;

public class CollisionHandler : MonoBehaviour
{
    [Inject] private readonly ICollisionResolver _collisionResolver;
    private IAnimal _animal;

    private void Awake() => _animal = GetComponent<IAnimal>();

    private void OnCollisionEnter(Collision collision)
    {
        var otherAnimal = collision.gameObject.GetComponent<IAnimal>();
        if (otherAnimal != null && _animal != null)
            _collisionResolver.PublishCollision(_animal, otherAnimal);
    }
}
