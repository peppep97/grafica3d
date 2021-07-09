using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//classe usata per condividere dei parametri tra le scene. Tra questi, nome del paziente, patologia, nome del modello corrispondente, età e sesso del paziente
public class GameControl : MonoBehaviour
{
    //Static reference alla classe
    public static GameControl control;

    //dati da memorizzare
    public String patientName;
    public String pathologyName;
    public String modelName;
    public String details;

    void Awake()
    {
        //con questo metodo il GameObject a cui è associato lo script GameControl persiste quando la scena cambia
        DontDestroyOnLoad(gameObject);

		//se control è null, impostalo alla classe corrente
        if (control == null)
        {
            control = this;
        }
		//se control non si riferisce a questa classe, distruggi il gameObject associato a questa classe
        else if (control != this)
        {
            Destroy(gameObject);
        }
    }
}
