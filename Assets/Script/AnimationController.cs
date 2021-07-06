using System.Security.Cryptography;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public float speed;

    public Vector3 destination;
    private bool start = false;

    void Start()
    {
        // Set the destination to be the object's position so it will not start off moving
        destination = gameObject.transform.position;    
    }

    void Update()
    {
        // If the object is not at the target destination
        if (destination != gameObject.transform.position)
        {
            // Move towards the destination each frame until the object reaches it
            if (start)
            {
                transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * speed);
            }
        }
        else
        {
            start = false;
        }
    }

    // Set the destination to cause the object to smoothly glide to the specified location
    public void SetDestination(Vector3 value)
    {
        start = true;
        destination = value;
    }
}