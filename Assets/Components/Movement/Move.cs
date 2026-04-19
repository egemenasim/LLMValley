using UnityEngine;

public abstract class Move : MonoBehaviour
{
    protected Transform _transform;
    protected float _speed;
    protected bool canMove;

    public virtual void Initialize(Transform transform, float speed)
    {
        _transform = transform;
        _speed = speed;
        canMove = true;
    }

    public virtual bool CanMove()
    {
        return canMove;
    }

    public virtual void SetMoveState(bool state)
    {
        canMove = state;
    }

    public abstract void Tick();

    protected Vector3 GetPosition()
{
    return _transform.position;
}

protected float GetPosX()
{
    return _transform.position.x;
}

protected float GetPosY()
{
    return _transform.position.y;
}



}

