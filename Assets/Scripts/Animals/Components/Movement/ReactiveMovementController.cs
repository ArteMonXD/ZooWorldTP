using Cysharp.Threading.Tasks;
using System;
using UniRx;
using UnityEngine;

public class ReactiveMovementController : IMovable, IDisposable
{
    private readonly Transform _transform;
    private readonly Rigidbody _rigidbody;
    private readonly MovementConfig _config;
    private readonly ScreenBoundary _screenBoundary;
    private readonly CompositeDisposable _disposables = new();

    private readonly ReactiveProperty<Vector3> _currentVelocity = new();
    private readonly ReactiveProperty<bool> _isMoving = new();
    private readonly Subject<Unit> _onMovementStarted = new();
    private readonly Subject<Unit> _onMovementStopped = new();

    public IReadOnlyReactiveProperty<Vector3> CurrentVelocity => _currentVelocity;
    public IReadOnlyReactiveProperty<bool> IsMoving => _isMoving;
    public IObservable<Unit> OnMovementStarted => _onMovementStarted;
    public IObservable<Unit> OnMovementStopped => _onMovementStopped;

    public ReactiveMovementController(Transform transform, Rigidbody rigidbody,
        MovementConfig config, ScreenBoundary screenBoundary)
    {
        _transform = transform;
        _rigidbody = rigidbody;
        _config = config;
        _screenBoundary = screenBoundary;
    }

    public void Initialize(MovementConfig config) { }

    public void StartMovement()
    {
        if (_isMoving.Value) return;
        _isMoving.Value = true;
        _onMovementStarted.OnNext(Unit.Default);
        StartJumpMovement();
        SetupBoundaryChecking();
    }

    private void StartJumpMovement()
    {
        Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(_config.JumpInterval))
            .Where(_ => _isMoving.Value)
            .Subscribe(async _ => await ExecuteJumpAsync())
            .AddTo(_disposables);
    }

    private async UniTask ExecuteJumpAsync()
    {
        var direction = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized;
        var jumpForce = direction * _config.JumpForce;

        await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate);
        _rigidbody.AddForce(jumpForce, ForceMode.Impulse);
        await UniTask.Delay(TimeSpan.FromSeconds(_config.JumpCooldown));
    }

    private void SetupBoundaryChecking()
    {
        Observable.EveryUpdate()
            .Where(_ => _isMoving.Value)
            .Select(_ => _transform.position)
            .DistinctUntilChanged()
            .Subscribe(position => HandleBoundaryCheck(position))
            .AddTo(_disposables);
    }

    private async void HandleBoundaryCheck(Vector3 position)
    {
        if (!_screenBoundary.IsWithinBounds(position))
        {
            var directionToCenter = (_screenBoundary.Center - position).normalized;
            _currentVelocity.Value = directionToCenter * _config.MoveSpeed;

            await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate);
            _rigidbody.linearVelocity = _currentVelocity.Value;

            await UniTask.Delay(TimeSpan.FromSeconds(1));
            if (_isMoving.Value) _currentVelocity.Value = GetRandomDirection() * _config.MoveSpeed;
        }
    }

    public void StopMovement()
    {
        if (!_isMoving.Value) return;
        _isMoving.Value = false;
        _currentVelocity.Value = Vector3.zero;
        _rigidbody.linearVelocity = Vector3.zero;
        _disposables.Clear();
        _onMovementStopped.OnNext(Unit.Default);
    }

    private Vector3 GetRandomDirection() => new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized;
    public void Dispose() => StopMovement();
}
