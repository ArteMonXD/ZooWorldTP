using Zenject;

public class Snake : AnimalBehaviour
{
    [Inject] private TastyTextSpawner _tastyTextSpawner;

    protected override void Start()
    {
        base.Start();
    }
}
