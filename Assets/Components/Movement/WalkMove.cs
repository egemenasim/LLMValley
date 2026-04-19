using UnityEngine;

public class WalkMove : WildMove
{
    void Update()
    {
         Debug.Log("WalkMove Update çalışıyor");
        Tick();

        MoveCharacter();
    }

    void MoveCharacter()
    {
        Vector3 direction = GetDirection();

        _transform.position += direction * speed * Time.deltaTime;
        Debug.Log(GetDirection());

    }
}
