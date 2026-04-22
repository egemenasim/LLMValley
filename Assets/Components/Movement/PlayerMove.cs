using UnityEngine;
using LLMValley.Components.Animation;

public class PlayerMove : Move
{
    private PlayerAnimationManager _animationManager;

    void Start()
    {
        Initialize(transform, 5f);
        _animationManager = GetComponent<PlayerAnimationManager>();
    }

    void Update()
    {
        Tick();
    }

    public override void Tick()
    {
        if (!CanMove()) 
        {
            if (_animationManager != null)
            {
                _animationManager.SetMovement(Vector2.zero);
            }
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, vertical, 0f).normalized;

        _transform.position += direction * speed * Time.deltaTime;

        if (_animationManager != null)
        {
            _animationManager.SetMovement(new Vector2(direction.x, direction.y));
        }
    }
}