using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float movementSpeed = 5;
    public float attackDamage = 10;

    void Movement()
    {
        Debug.Log("Movimiento base");
    }

    public virtual void Attack()
    {
        Debug.Log("Ataque base");
    }
}
