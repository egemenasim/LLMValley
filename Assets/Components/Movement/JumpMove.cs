using UnityEngine;
public class JumpMove : WildMove
{
    public AnimationCurve jumpCurve;

    private float _jumpTimer;

    void Update()
    {
        Tick();
        MoveJump();
    }

    void MoveJump()
    {
        _jumpTimer += Time.deltaTime;

        float yOffset = jumpCurve.Evaluate(_jumpTimer);

     Vector3 baseDir = GetDirection();

Vector3 jitter = new Vector3(
    Random.Range(-0.3f, 0.3f),
    0,
    0
);

Vector3 finalDir = (baseDir + jitter).normalized;
_transform.position += new Vector3(finalDir.x, yOffset, 0f) * speed * Time.deltaTime;


    }
}
