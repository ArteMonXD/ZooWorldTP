using Cysharp.Threading.Tasks;
using System;
using UniRx;
using UnityEngine;
using Zenject;

public class FrogMovement : MonoBehaviour, IMovable
{
    [SerializeField] private Rigidbody _rigidbody;
    
    private MovementConfig _config;
    private CompositeDisposable _disposables = new CompositeDisposable();
    private ReactiveProperty<Vector3> _currentVelocity = new ReactiveProperty<Vector3>();
    private ReactiveProperty<bool> _isMoving = new ReactiveProperty<bool>();

    public IReadOnlyReactiveProperty<Vector3> CurrentVelocity => _currentVelocity;
    public IReadOnlyReactiveProperty<bool> IsMoving => _isMoving;

    [Inject]
    public void Construct(GameConfig gameConfig)
    {
        Debug.Log($"FrogMovement: Construct called, config: {gameConfig != null}");
        
        _config = gameConfig.FrogMovementConfig;
        
        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();
            
        Debug.Log($"FrogMovement: Rigidbody: {_rigidbody != null}, Config: {_config != null}");
    }

    public void Initialize(MovementConfig config)
    {
        _config = config;
        Debug.Log("FrogMovement: Initialized with config");
    }

    public void StartMovement()
    {
        Debug.Log("FrogMovement: StartMovement called");
        
        if (_isMoving.Value) 
        {
            Debug.Log("FrogMovement: Already moving");
            return;
        }
        
        _isMoving.Value = true;
        Debug.Log("FrogMovement: Movement started");
        
        StartJumpMovement();
        SetupBoundaryChecking();
    }

    private void StartJumpMovement()
    {
        Debug.Log($"FrogMovement: Starting jump movement with interval: {_config.JumpInterval}");
        
        Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(_config.JumpInterval))
            .Where(_ => _isMoving.Value)
            .Subscribe(async _ => 
            {
                Debug.Log("FrogMovement: Executing jump");
                await ExecuteJumpAsync();
            })
            .AddTo(_disposables);
    }

    private async UniTask ExecuteJumpAsync()
    {
        if (!_isMoving.Value) return;

        var direction = new Vector3(
            UnityEngine.Random.Range(-1f, 1f), 
            0, 
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized;

        var jumpVector = direction * _config.JumpForce;
        
        Debug.Log($"FrogMovement: Jumping with force: {jumpVector}");
        
        await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate);
        _rigidbody.AddForce(jumpVector, ForceMode.Impulse);
    }

    private void SetupBoundaryChecking()
    {
        Observable.EveryUpdate()
            .Where(_ => _isMoving.Value)
            .Subscribe(_ => CheckBoundaries())
            .AddTo(_disposables);
    }

    private void CheckBoundaries()
    {
        var position = transform.position;
        if (Mathf.Abs(position.x) > 9f || Mathf.Abs(position.z) > 9f)
        {
            var directionToCenter = (Vector3.zero - position).normalized;
            _rigidbody.AddForce(directionToCenter * 1f, ForceMode.Impulse);
        }
    }

    public void StopMovement()
    {
        Debug.Log("FrogMovement: StopMovement called");
        _isMoving.Value = false;
        _disposables.Clear();
        if (_rigidbody != null)
            _rigidbody.linearVelocity = Vector3.zero;
    }

    private void OnDestroy()
    {
        Debug.Log("FrogMovement: OnDestroy");
        _disposables?.Dispose();
    }
}
