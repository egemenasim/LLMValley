using UnityEngine;

public abstract class WildMove : Move
{
    protected Vector3 _currentDirection;
    protected float _timer;
    protected float _changeInterval = 0.5f; // daha sık değişim

    void Start()
    {
        Initialize(transform, 5f);
        SetNewDirection();
    }

    void Update()
    {
        Tick();
    }

    public override void Tick()
    {
        if (!CanMove()) return;

        _timer += Time.deltaTime;

        if (_timer >= _changeInterval)
        {
            SetNewDirection();
        }
    }

    protected void SetNewDirection()
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);

        _currentDirection = new Vector3(x, y, 0f).normalized;

        _timer = 0f;
    }

    public Vector3 GetDirection()
    {
        return _currentDirection;
    }
}
