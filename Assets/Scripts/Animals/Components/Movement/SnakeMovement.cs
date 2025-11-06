using System;
using UniRx;
using UnityEngine;
using Zenject;

public class SnakeMovement : MonoBehaviour, IMovable
{
    [SerializeField] private Rigidbody _rigidbody;

    private MovementConfig _config;
    private CompositeDisposable _disposables = new CompositeDisposable();
    private ReactiveProperty<Vector3> _currentVelocity = new ReactiveProperty<Vector3>();
    private ReactiveProperty<bool> _isMoving = new ReactiveProperty<bool>();
    private Vector3 _currentDirection;

    public IReadOnlyReactiveProperty<Vector3> CurrentVelocity => _currentVelocity;
    public IReadOnlyReactiveProperty<bool> IsMoving => _isMoving;

    [Inject]
    public void Construct(GameConfig gameConfig)
    {
        _config = gameConfig.SnakeMovementConfig;

        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();

        _currentDirection = GetRandomDirection();
    }

    public void Initialize(MovementConfig config)
    {
        _config = config;
    }

    public void StartMovement()
    {
        if (_isMoving.Value) return;

        _isMoving.Value = true;
        StartLinearMovement();
        SetupDirectionChanges();
        SetupBoundaryChecking(); // ДОБАВЛЕНО: проверка границ
    }

    private void StartLinearMovement()
    {
        Observable.EveryUpdate()
            .Where(_ => _isMoving.Value)
            .Subscribe(_ => Move())
            .AddTo(_disposables);
    }

    private void SetupDirectionChanges()
    {
        Observable.Interval(TimeSpan.FromSeconds(_config.DirectionChangeInterval))
            .Where(_ => _isMoving.Value)
            .Subscribe(_ => _currentDirection = GetRandomDirection())
            .AddTo(_disposables);
    }

    private void SetupBoundaryChecking()
    {
        Observable.EveryUpdate()
            .Where(_ => _isMoving.Value)
            .Subscribe(_ => CheckBoundaries())
            .AddTo(_disposables);
    }

    private void Move()
    {
        var newPosition = transform.position + _currentDirection * _config.MoveSpeed * Time.deltaTime;
        _rigidbody.MovePosition(newPosition);
        _currentVelocity.Value = _currentDirection * _config.MoveSpeed;
    }

    private void CheckBoundaries()
    {
        var position = transform.position;
        if (Mathf.Abs(position.x) > 9f || Mathf.Abs(position.z) > 9f)
        {
            Debug.Log($"Snake out of bounds at {position}, redirecting to center");
            RedirectToCenter();
        }
    }

    private void RedirectToCenter()
    {
        // Вычисляем направление к центру
        var directionToCenter = (Vector3.zero - transform.position).normalized;

        // Меняем направление движения на направление к центру
        _currentDirection = directionToCenter;

        // Добавляем дополнительный импульс к центру
        _rigidbody.AddForce(directionToCenter * 1f, ForceMode.Impulse);

        Debug.Log($"Snake redirected to center with direction {_currentDirection}");

        // Через 2 секунды возвращаем случайное направление
        Observable.Timer(TimeSpan.FromSeconds(2))
            .Where(_ => _isMoving.Value)
            .Subscribe(_ => _currentDirection = GetRandomDirection())
            .AddTo(_disposables);
    }

    public void StopMovement()
    {
        _isMoving.Value = false;
        _disposables.Clear();
        if (_rigidbody != null)
            _rigidbody.linearVelocity = Vector3.zero;
    }

    private Vector3 GetRandomDirection()
    {
        return new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            0,
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized;
    }

    private void OnDestroy()
    {
        _disposables?.Dispose();
    }
}
