using System;

public struct CollisionEvent
{
    public readonly IAnimal Initiator;
    public readonly IAnimal Target;
    public readonly CollisionOutcome Outcome;
    public readonly DateTime Timestamp;

    public CollisionEvent(IAnimal initiator, IAnimal target, CollisionOutcome outcome = CollisionOutcome.None)
    {
        Initiator = initiator;
        Target = target;
        Outcome = outcome;
        Timestamp = DateTime.Now;
    }
}
