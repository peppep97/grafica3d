using UnityEngine;
using UnityEngine.SceneManagement;

public class CambioScena : MonoBehaviour
{
	//ogni metodo imposta i parametri riferiti al modello selezionato nell'oggetto GameControl (singleton class)
	//usato per passare i parametri alla scena successiva
    public void Modello_Polmone_Pressed()
    {
        GameControl.control.patientName = "Paziente 1";
        GameControl.control.pathologyName = "Tumore Polmonare";
        GameControl.control.modelName = "Polmoni";
        GameControl.control.details = "Et�: 70 - Sesso: M";
        SceneManager.LoadScene("3dModel");
    }

    public void Modello_Cervello_Pressed()
    {
        GameControl.control.patientName = "Paziente 2";
        GameControl.control.pathologyName = "Ascesso Cerebrale";
        GameControl.control.modelName = "Cervello";
        GameControl.control.details = "Et�: 55 - Sesso: F";
        SceneManager.LoadScene("3dModel");
    }

    public void Modello_Gastro_Pressed() 
    {
        GameControl.control.patientName = "Paziente 3";
        GameControl.control.pathologyName = "Ascesso Epatico";
        GameControl.control.modelName = "Sistema Gastrointestinale";
        GameControl.control.details = "Et�: 35 - Sesso: M";
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
        //application.Quit() non funziona nell'editor. In tal caso imposta isPlaying a false per chiudere l'applicazione
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

}
