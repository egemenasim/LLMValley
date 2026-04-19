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

        Vector3 dir = GetDirection();
        Debug.Log(GetDirection());
        //  flat movement (XZ düzlemi)
        Vector3 movement = new Vector3(dir.x, 0f, 0f);

        //  jump sadece Y ekseni
        float yOffset = jumpCurve.Evaluate(_jumpTimer);

        _transform.position += (movement * speed * Time.deltaTime);
        _transform.position += new Vector3(0f, yOffset, 0f) * Time.deltaTime;
    }
}
