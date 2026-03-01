using UnityEngine;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;
    private PlayerMovement movement;

   
    private Vector2 lastDir = Vector2.down;

    void Awake()
    {
        anim     = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
      
        if (!movement.IsStanding)
        {
            anim.Play("IdleUp");
            return;
        }

        Vector2 input = movement.InputDirection;

        if (input != Vector2.zero)
        {
            lastDir = input;

            if (Mathf.Abs(lastDir.x) > Mathf.Abs(lastDir.y))
                anim.Play(lastDir.x > 0 ? "RunRight" : "RunLeft");
            else
                anim.Play(lastDir.y > 0 ? "RunUp" : "RunDown");
        }
        else
        {
            if (Mathf.Abs(lastDir.x) > Mathf.Abs(lastDir.y))
                anim.Play(lastDir.x > 0 ? "IdleRight" : "IdleLeft");
            else
                anim.Play(lastDir.y > 0 ? "IdleUp" : "IdleDown");
        }
    }



}
