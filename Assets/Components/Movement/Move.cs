using UnityEngine;

public abstract class Move : MonoBehaviour
{
    protected Transform _transform;
    public float speed;
    protected bool canMove;

    public virtual void Initialize(Transform transform, float speedGet)
    {
        _transform = transform;
        speed = speedGet;
        canMove = true;
    }

public virtual bool CanMove()
    {
        // Also blocked when any dialog/UI has locked player input globally.
        return canMove && !PlayerInputLock.IsLocked;
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

