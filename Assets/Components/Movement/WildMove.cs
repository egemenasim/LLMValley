using UnityEngine;

public class WildMove : Move
{
    protected Vector3 _currentDirection;
    protected float _timer;

    [SerializeField] protected float changeInterval = 1.5f;
    [SerializeField] protected float turnSpeed = 3f;

    void Start()
    {
        Initialize(transform, 5f);
        SetNewDirection();
    }

    void Update()
    {
        Tick();
        MoveCharacter();
    }

    public override void Tick()
    {
        _timer += Time.deltaTime;

        if (_timer >= changeInterval)
        {
            SetNewDirection();
            _timer = 0f;
        }
    }

    protected void SetNewDirection()
    {
        Vector3 target = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            0f
        ).normalized;

        _currentDirection = Vector3.Lerp(
            _currentDirection,
            target,
            turnSpeed * Time.deltaTime
        );
    }

    protected void MoveCharacter()
    {
        _transform.position += _currentDirection * speed * Time.deltaTime;
    }

    public Vector3 GetDirection()
    {
        return _currentDirection;
    }
}

