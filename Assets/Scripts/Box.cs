using UnityEngine;

public class Box : MonoBehaviour, IGrabeable
{
    public void Grab()
    {
        Debug.Log("Recogiendo caja");
    }
}
