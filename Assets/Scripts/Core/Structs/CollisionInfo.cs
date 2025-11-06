using System;
using UnityEngine;

public struct CollisionInfo
{
    public readonly IAnimal AnimalA;
    public readonly IAnimal AnimalB;
    public readonly Vector3 ImpactPoint;
    public readonly Vector3 ImpactNormal;
    public readonly float ImpactForce;
    public readonly DateTime Timestamp;
    public readonly CollisionType CollisionType;

    public CollisionInfo(IAnimal animalA, IAnimal animalB, Vector3 impactPoint,
                        Vector3 impactNormal, float impactForce, CollisionType collisionType)
    {
        AnimalA = animalA;
        AnimalB = animalB;
        ImpactPoint = impactPoint;
        ImpactNormal = impactNormal;
        ImpactForce = impactForce;
        CollisionType = collisionType;
        Timestamp = DateTime.Now;
    }

    public static CollisionType DetermineCollisionType(IAnimal animalA, IAnimal animalB)
    {
        if (animalA?.Type == AnimalType.Prey && animalB?.Type == AnimalType.Prey)
            return CollisionType.PreyToPrey;
        if (animalA?.Type != animalB?.Type)
            return CollisionType.PreyToPredator;
        if (animalA?.Type == AnimalType.Predator && animalB?.Type == AnimalType.Predator)
            return CollisionType.PredatorToPredator;
        return CollisionType.PreyToPrey;
    }
}
