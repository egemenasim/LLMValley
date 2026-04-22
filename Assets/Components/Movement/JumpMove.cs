using UnityEngine;
public class JumpMove : WildMove
{
    public AnimationCurve jumpCurve;

    private float _jumpTimer;

    [SerializeField] private float jumpDuration = 1f;

    private Vector3 _basePosition;

    void Start()
    {
        Initialize(transform, speed);
        _basePosition = transform.position;
    }

    void Update()
    {
        Tick();
        MoveJump();
    }

    void MoveJump()
    {
        _jumpTimer += Time.deltaTime;

        if (_jumpTimer > jumpDuration)
            _jumpTimer = 0f;

        Vector3 dir = GetDirection();

        //  sadece horizontal movement
        _basePosition += dir * speed * Time.deltaTime;

        //  jump sadece offset
        float t = _jumpTimer / jumpDuration;
        float jumpOffset = jumpCurve.Evaluate(t);

        _transform.position = new Vector3(
            _basePosition.x,
            _basePosition.y + jumpOffset,
            0f
        );
    }
}
