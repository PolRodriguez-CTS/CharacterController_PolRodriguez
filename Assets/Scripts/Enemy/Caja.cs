using UnityEngine;

public class Caja : MonoBehaviour, IDamageable, IInteractable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void IDamageable.TakeDamage(float damage)
    {
        Debug.Log("Caja recibiendo daño");
    }

    void IInteractable.Interact()
    {
        Debug.Log("Interactuado con caja");
    }
}
