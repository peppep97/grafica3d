using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CambioScena : MonoBehaviour
{
    // Start is called before the first frame update

    public void Modello_Polmone_Pressed()   //per assegnare al pulsante "button" una funzione
    {
        GameControl.control.patientName = "Paziente 1";
        GameControl.control.pathologyName = "Tumore Polmonare";
        GameControl.control.modelName = "Polmoni";
        SceneManager.LoadScene("3dModel");
    }

    public void Modello_Cervello_Pressed()   //per assegnare al pulsante "button" una funzione
    {
        GameControl.control.patientName = "Paziente 2";
        GameControl.control.pathologyName = "Ascesso Cerebrale";
        GameControl.control.modelName = "Cervello";
        SceneManager.LoadScene("3dModel");
    }

    public void Modello_Gastro_Pressed()   //per assegnare al pulsante "button" una funzione
    {
        GameControl.control.patientName = "Paziente 3";
        GameControl.control.pathologyName = "...";
        GameControl.control.modelName = "Gastro";
        SceneManager.LoadScene("3dModel");
    }

    public void Tc_Cervello_Pressed()   //per assegnare al pulsante "button" una funzione
    {
        SceneManager.LoadScene("TC_Cervello");
    }
    public void Tc_Gastro_Pressed()   //per assegnare al pulsante "button" una funzione
    {
        SceneManager.LoadScene("TC_Gastro");
    }
    public void Tc_Polmone_Pressed()   //per assegnare al pulsante "button" una funzione
    {
        SceneManager.LoadScene("TC_Polmone");
    }

    public void HomePressed()   //per assegnare al pulsante "button" una funzione
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitPressed()
    {
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

}
