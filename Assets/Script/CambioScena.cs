using UnityEngine;
using UnityEngine.SceneManagement;

public class CambioScena : MonoBehaviour
{
    public void Modello_Polmone_Pressed()
    {
        GameControl.control.patientName = "Paziente 1";
        GameControl.control.pathologyName = "Tumore Polmonare";
        GameControl.control.modelName = "Polmoni";
        GameControl.control.details = "Età: 70 - Sesso: M";
        SceneManager.LoadScene("3dModel");
    }

    public void Modello_Cervello_Pressed()
    {
        GameControl.control.patientName = "Paziente 2";
        GameControl.control.pathologyName = "Ascesso Cerebrale";
        GameControl.control.modelName = "Cervello";
        GameControl.control.details = "Età: 55 - Sesso: F";
        SceneManager.LoadScene("3dModel");
    }

    public void Modello_Gastro_Pressed() 
    {
        GameControl.control.patientName = "Paziente 3";
        GameControl.control.pathologyName = "Ascesso Epatico";
        GameControl.control.modelName = "Sistema Gastrointestinale";
        GameControl.control.details = "Età: 35 - Sesso: M";
        SceneManager.LoadScene("3dModel");
    }

    public void HomePressed()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void MRIPressed()
    {
        SceneManager.LoadScene("mriModel");
    }

    public void Model3DPressed()
    {
        SceneManager.LoadScene("3dModel");
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
