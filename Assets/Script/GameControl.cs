using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControl : MonoBehaviour
{
    //Static reference
    public static GameControl control;

    //Data to persist
    public String patientName;
    public String pathologyName;
    public String modelName;
    public String details;

    void Awake()
    {
        //Let the gameobject persist over the scenes
        DontDestroyOnLoad(gameObject);

        if (control == null)
        {
            //This instance becomes the single instance available
            control = this;
        }
        //Otherwise check if the control instance is not this one
        else if (control != this)
        {
            //In case there is a different instance destroy this one.
            Destroy(gameObject);
        }
    }
}
