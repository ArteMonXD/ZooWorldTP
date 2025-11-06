using System;

public struct AnimalCollisionMessage
{
    public readonly IAnimal AnimalA;
    public readonly IAnimal AnimalB;
    public readonly DateTime Timestamp;

    public AnimalCollisionMessage(IAnimal animalA, IAnimal animalB)
    {
        AnimalA = animalA;
        AnimalB = animalB;
        Timestamp = DateTime.Now;
    }
}
