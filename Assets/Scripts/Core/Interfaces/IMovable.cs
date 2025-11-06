using UniRx;
using UnityEngine;

public interface IMovable
{
    IReadOnlyReactiveProperty<Vector3> CurrentVelocity { get; }
    IReadOnlyReactiveProperty<bool> IsMoving { get; }
    void Initialize(MovementConfig config);
    void StartMovement();
    void StopMovement();
}
