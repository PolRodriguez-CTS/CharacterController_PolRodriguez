using UnityEngine;

public class MeleeEnemy : Enemy, IDamageable
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Attack();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Attack()
    {
        base.Attack();
        Debug.Log("Ataque cuerpo a cuerpo");
    }

    void IDamageable.TakeDamage(float damage)
    {
        Debug.Log("Enemigo recibiendo daño");
    }
}
