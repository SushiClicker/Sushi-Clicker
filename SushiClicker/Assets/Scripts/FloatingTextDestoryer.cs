using UnityEngine;

public class FloatingTextDestoryer : MonoBehaviour
{
    private float secondsToDestroy = 1f;

    //Start function is called when the script is being loaded, destroys the GameObject after 1 second
    void Start()
    {
        Destroy(gameObject, secondsToDestroy);
    }

}
