using UnityEngine;

public class PlayerMove : Move
{
    void Start()
    {
        Initialize(transform, 5f);
    }

    void Update()
    {
        Tick();
    }

    public override void Tick()
    {
        if (!CanMove()) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");


        Vector3 direction = new Vector3(horizontal, vertical, 0f).normalized;

        _transform.position += direction * speed * Time.deltaTime;
    }
}

