using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using Leap.Unity.Infix;
using System;
using UnityEngine.UI;

//classe per gestire la scena del modello 3D. E' associata alla mainCamera.

public class GesturesScript : MonoBehaviour
{
	//per impostare gli oggetti 3d e i loro materiali
    public GameObject polmoniObject;
    public GameObject cervelloObject;
    public GameObject gastroObject;
    public List<Material> polmoniMaterials;
    public List<Material> cervelloMaterials;
    public List<Material> gastroMaterials;

	//per impostare l'oggetto 3d selezionato
    public GameObject mainModel;

	//reference alle label del canvas, per impostare la status bar
    public Text patientText;
    public Text pathologyText;
    public Text dateText;
    public Text detailText;
    public Text label;

	//oggetto principale per accedere al leap motion
    Controller controller;

	//oggetto selezionato
    GameObject selectedModel;
	//sotto parte selezionata
    GameObject selectedChild;
	//sotto parte evidenziata
    GameObject highlightedModel;

	//flag per memorizzare lo stato di opacità del modello (false -> trasparente)
    bool isOpaque = true;

	//materiali del modello selezionato
    List<Material> materials;

	//posizione e rotazione di default dell'oggetto (caricati nella fase di setup, quindi la prima volta che lo script viene eseguito)
    Vector3 defaultPosition;
    Quaternion defaultRotation;

	//variabili usate per memorizzare i valori letti dal leap motion nel frame corrente, così da poterli confrontare con quelli letti nel frame successivo
    float lastDistanceBetweenHands = -1; //per lo zoom
    float lastYDirectionLeftHand = -1;  //per spostamento/rotazione
    float lastXDirectionLeftHand = -1;
    Vector3 lastPalmPosition = new Vector3(-1, -1, -1); //per selezione oggetto
	
	//tiene traccia del tempo trascorso
    float time = 0f;

	//reference al modello della mano destra presente nella scena
    public HandModelBase HandModel = null;

    void Start()
    {
        //leggi i parametri condivisi relativi al modello selezionato
        String patientName = GameControl.control.patientName;
        String pathologyName = GameControl.control.pathologyName;
        String modelName = GameControl.control.modelName;
        String details = GameControl.control.details;

        patientText.text = patientName;
        pathologyText.text = pathologyName;
        detailText.text = details;

		//disattiva tutti i modelli
        polmoniObject.SetActive(false);
        cervelloObject.SetActive(false);
        gastroObject.SetActive(false);

		//in base al nome del modello selezioanto, questo si attiva e vengono impostati l'oggetto 3d (mainModel) e i materiali
        if (modelName.Equals("Polmoni"))
        {
            polmoniObject.SetActive(true);
            mainModel = polmoniObject;
            materials = polmoniMaterials;
        }else if (modelName.Equals("Cervello"))
        {
            cervelloObject.SetActive(true);
            mainModel = cervelloObject;
            materials = cervelloMaterials;
        }
        else
        {
            gastroObject.SetActive(true);
            mainModel = gastroObject;
            materials = gastroMaterials;
        }

        selectedModel = mainModel;
        defaultPosition = selectedModel.transform.position;
        defaultRotation = selectedModel.transform.rotation;

        selectedChild = selectedModel;

		//imposta la label (in basso a sinistra) con il nome del modello selezionato
        label.text = selectedModel.transform.name;
    }

    void Update()
    {
		//mostra la data corrente nella status bar
        DateTime dt = DateTime.Now;
        dateText.text = dt.ToString("dd/MM/yyyy - HH:mm");

        controller = new Controller();
		//si accede al Frame del leapmotion, che contiene gli attributi e metodi per accedere alle mani riconosciute nel frame corrente
        Frame frame = controller.Frame();
		//leggi tutte le mani riconoscoute
        List<Hand> hands = frame.Hands;

		//se c'è una sola mano nel frame, chima la funzione di selezione
        if (frame.Hands.Count == 1)
        {
            fingerPointingWatcher(hands[0]);
        }

		//se ci sono due mani, vengono riconsciute le gesture
        if (frame.Hands.Count == 2)
        {
			//cancella il contorno dall'ultimo oggetto che era stato evidenziato
            if (highlightedModel != null)
            {
                highlightedModel.GetComponent<Outline>().OutlineMode = Outline.Mode.OutlineHidden;
            }


            Hand firstHand = hands[0];
            Hand secondHand = hands[1];

			//se entrambe le mani hanno il pugno chiuso -> gesture di spostamento
            if (firstHand.GrabStrength > 0.7 && secondHand.GrabStrength > 0.7)
            {
				//resetta i parametri riferiti ad altre gesture
                lastDistanceBetweenHands = -1;

				//leggi la posizione del palmo della mano destra
                Vector palmPosition = secondHand.PalmPosition;

				//leggi le coordinate x e y del palmo
                float yDirectionLeftHand = palmPosition.y;
                float xDirectionLeftHand = palmPosition.x;

                if (lastYDirectionLeftHand == -1)
                {
                    lastYDirectionLeftHand = yDirectionLeftHand;
                }

                if (lastXDirectionLeftHand == -1)
                {
                    lastXDirectionLeftHand = xDirectionLeftHand;
                }

				//calcola la differenza con le coordinate lette nel frame precedente
                float differenceY = yDirectionLeftHand - lastYDirectionLeftHand;
                float differenceX = xDirectionLeftHand - lastXDirectionLeftHand;

				//imposta le ultime coordinate lette con quelle attuali
                lastYDirectionLeftHand = yDirectionLeftHand;
                lastXDirectionLeftHand = xDirectionLeftHand;

				//le differenze calcolate sono utilizzate come offset (vengono scalate per fare uno spostamento più preciso) 
				//da aggiungere alle coordinate x e y dell'oggetto, così da spostarlo
                float offsetX = differenceX / 200;
                float offsetY = differenceY / 200;

				//leggi le posizioni attuali del modello
                float CamX = selectedModel.transform.position.x;
                float CamY = selectedModel.transform.position.y;
                float CamZ = selectedModel.transform.position.z;

				//imposta la nuova posizione aggiungendo l'offset
                selectedModel.transform.position = new Vector3(CamX + offsetX, CamY + offsetY, CamZ); //Move the object
            }
			
			//se entrambe le mani non hanno il pugno chiuso e hanno il dito indice a contatto con il pollice (pinchStrength > 0.5) -> gesture di zoom 
            else if (firstHand.PinchStrength > 0.5 && firstHand.GrabStrength < 0.7 && secondHand.PinchStrength > 0.5 && secondHand.GrabStrength < 0.7) 
            {

				//resetta i parametri riferiti ad altre 
                lastXDirectionLeftHand = -1;
                lastYDirectionLeftHand = -1;

				//calcola la distanza tra la posizione dei palmi delle due mani
                float distanceBetweenHands = firstHand.PalmPosition.DistanceTo(secondHand.PalmPosition);

                if (lastDistanceBetweenHands == -1)
                {
                    lastDistanceBetweenHands = distanceBetweenHands;
                }

				//calcola la differenza della distanza tra il frame corrente e quello precedente
                float difference = distanceBetweenHands - lastDistanceBetweenHands;
				//imposta la distanza "precedente" con quella attuale
                lastDistanceBetweenHands = distanceBetweenHands;

				//la differenza viene usata come offset da aggiungere all'asse Z (così l'oggetto si avvicina/allontana)
                if (difference != 0)
                {
                    float R = -difference / 100;

                    //leggi le coordianate correnti e muovi l'oggetto (cambiando solo z)
                    float CamX = selectedModel.transform.position.x;                     
                    float CamY = selectedModel.transform.position.y;                      
                    float CamZ = selectedModel.transform.position.z;                      
                    selectedModel.transform.position = new Vector3(CamX, CamY, CamZ + R);
                }
            }
			//se la mano sinistra ha il pugno chiuso e la mano destra è aperta -> gesture di rotazione
            else if (secondHand.PinchStrength < 0.5 && secondHand.GrabStrength <= 0.7 && firstHand.GrabStrength > 0.7)
            {
                lastDistanceBetweenHands = -1;
				
                Vector palmPosition = secondHand.PalmPosition;

                float yDirectionLeftHand = palmPosition.y;
                float xDirectionLeftHand = palmPosition.x;

                if (lastYDirectionLeftHand == -1)
                {
                    lastYDirectionLeftHand = yDirectionLeftHand;
                }

                if (lastXDirectionLeftHand == -1)
                {
                    lastXDirectionLeftHand = xDirectionLeftHand;
                }

                float differenceY = yDirectionLeftHand - lastYDirectionLeftHand;
                float differenceX = xDirectionLeftHand - lastXDirectionLeftHand;

                lastYDirectionLeftHand = yDirectionLeftHand;
                lastXDirectionLeftHand = xDirectionLeftHand;
				
				//come la gesture di spostamento, solo che le differenze calcolate vengono usate come angolo di rotazione

                selectedModel.transform.Rotate(differenceY / 2, differenceX / 2, 0);
            }
            else
            {
				//se non è stata riconosciuta nesssuna gesture, resetta tutto
                lastDistanceBetweenHands = -1;
                lastXDirectionLeftHand = -1;
                lastYDirectionLeftHand = -1;
            }
        }
    }

	//funzione per selezionare una parte del modello
    private void fingerPointingWatcher(Hand hand)
    {
        Vector3 targetDirection;

		//se il modello relativo alla mano destra è riconosciuto
        if (HandModel != null && HandModel.IsTracked)
        {
			
            targetDirection = Camera.main.transform.TransformDirection(Vector3.forward);

			//leggi il dito indice
            Finger finger = HandModel.GetLeapHand().Fingers[1];

			//se il dito indice è esteso e tutti gli altri no -> gesture di selezione
            if (finger.IsExtended)
            {
                bool cont = true;
                foreach (Finger f in HandModel.GetLeapHand().Fingers)
                {
                    if (f != finger && f.IsExtended)
                    {
                        cont = false;
                    }
                }
				//entra solo se l'unico dito esteso è l'indice (quindi cont non è cambiato nel for each)
                if (cont)
                {
                    if (lastPalmPosition.Equals(new Vector3(-1, -1, -1)))
                    {
                        lastPalmPosition = hand.PalmPosition.ToVector3();
                    }

                    //calcola la variazione della posizione del palmo
                    Vector3 difference = hand.PalmPosition.ToVector3() - lastPalmPosition;
					//se la posizione è variata di pochissimo possiamo assumere che la mano sia rimasta ferma
                    if (difference.x.Abs() < 1 && difference.y.Abs() < 1 && difference.z.Abs() < 1)
                    {
						//effettua un raycast sparando un raggio che parte dal punta del dito e va verso l'asse z
                        RaycastHit hit;
                        if (Physics.Raycast(finger.TipPosition.ToVector3(), targetDirection, out hit))
                        {
							//se è stata riconosciuta una collisione con il modello stesso o con una sua sotto parte
                            if (hit.collider.gameObject == selectedModel || hit.collider.gameObject.transform.parent.gameObject == selectedModel)
                            {
								//imposta la sotto parte da evidenziare
                                highlightedModel = hit.collider.gameObject;
								//imposta il nome della sotto parte selezioanta (nella label in basso a sinistra)
                                label.text = hit.collider.gameObject.name;
								//chiama la funzione per evidenziare l'oggetto
                                highlightObject();

								//incrementa time con il tempo trascorso
                                time += Time.deltaTime;
								
								//se sono trascorsi 2s (in cui la mano è rimasta ferma)
                                if (time >= 2f)
                                {
									//seleziona l'oggetti evidenziato
                                    selectObject(hit.collider.gameObject, 0.57f);
                                    time = 0f;
                                }
                            }
                        }
                        else
                        {
							//se non c'è stata collisione, reimposta la label con il modello selezionato e cancella l'eventuale oggetto evidenziato
                            if (highlightedModel != null)
                            {
                                highlightedModel.GetComponent<Outline>().OutlineMode = Outline.Mode.OutlineHidden;
                            }
                            if (selectedChild != null)
                            {
                                label.text = selectedChild.transform.name;
                            }
                        }
                    }
                    else
                    {
						//reimposta il timer se la mano si è mossa nel mentre
                        time = 0f;
                        lastPalmPosition = new Vector3(-1, -1, -1);
                    }

					//aggiorna l'ultima posizione del palmo con quella corrente
                    lastPalmPosition = hand.PalmPosition.ToVector3();
                }
                else
                {
                    if (selectedChild != null)
                    {
                        label.text = selectedChild.transform.name;
                    }
                }
            }
        }
    }

	//funzione per evidenziare un oggetto
    private void highlightObject()
    {
		//seleziona quello corrente
        if (highlightedModel != null)
        {
            highlightedModel.GetComponent<Outline>().OutlineMode = Outline.Mode.OutlineAll;
        }

		//deseleziona tutti gli altri
        Component[] objectsOutline = selectedModel.GetComponentsInChildren<Outline>();
        foreach (Outline item in objectsOutline)
        {
            if (item.gameObject != highlightedModel)
            {
                item.OutlineMode = Outline.Mode.OutlineHidden;
            }
        }
    }

	//quando viene premuto il pulsante per cambiare l'opacità, inverti il flag 
    public void onOpaquePressed()
    {
        isOpaque = !isOpaque;
        setOpaque(isOpaque, null);

    }

	//per ottenere la posizione di una sottoparte relativamente al padre
    public static Vector3 getRelativePosition(Transform origin, Vector3 position)
    {
        Vector3 distance = position - origin.position;
        Vector3 relativePosition = Vector3.zero;
        relativePosition.x = Vector3.Dot(distance, origin.right.normalized);
        relativePosition.y = Vector3.Dot(distance, origin.up.normalized);
        relativePosition.z = Vector3.Dot(distance, origin.forward.normalized);

        return relativePosition;
    }

	//funzione per selezionare una sotto parte
    public void selectObject(GameObject child, float zoom)
    {
		//muovi l'oggetto padre alla posizione di default
        selectedModel.GetComponent<AnimationController>().SetDestination(defaultPosition);

        label.text = child.gameObject.name;
		//ottieni il centro della sotto parte
        Vector3 center = child.GetComponent<Renderer>().bounds.center;

		//calcola la distanza del centro della sottoparte relativamente al padre
        Vector3 offset = getRelativePosition(selectedModel.transform, center);
		//riposiziona il modello 3d rispetto al centro della sotto parte (così che la sottoparte è al centro della camera)
        Vector3 newPos = new Vector3(defaultPosition.x + offset.x, defaultPosition.y + offset.y, zoom);

        selectedModel.GetComponent<AnimationController>().SetDestination(newPos);

		//imposta il modello tutto trasparente tranne la sottoparte selezionata
        isOpaque = false;
        setOpaque(isOpaque, child);

        selectedChild = child;
    }

	//funzione per reimpostare la posizione del modello 3d ai valori di default
    public void resetMainObject()
    {
        selectedModel.transform.rotation = defaultRotation;
        selectedModel.GetComponent<AnimationController>().SetDestination(defaultPosition);

        label.text = selectedModel.transform.name;

		//reimposta il modello tutto opaco
        isOpaque = true;
        setOpaque(isOpaque, null);

        selectedChild = selectedModel;
    }

	//funzione per impostare l'opacità (per ogni sottoparte, carica il relativo materiale trasparente/opaco)
    private void setOpaque(bool opaque, GameObject exclude)
    {
		//leggi tutte le sotto parti del modello
        for (int i = 0; i < selectedModel.transform.childCount - 1; i++)
        {
            GameObject child = selectedModel.transform.GetChild(i).gameObject;

            if (!opaque)
            {
                if (child != exclude)
                {
					//imposta il materiale trasparente alla sottoparte 
                    child.GetComponent<Renderer>().material = materials[i + selectedModel.transform.childCount - 1];
                }
                else
                {
					//se l'oggetto corrente è quello da escludere, imposta il suo materiale a opaco
                    child.GetComponent<Renderer>().material = materials[i];
                }
            }
            else
            {
				//imposta il materiale opaco alla sottoparte 
                child.GetComponent<Renderer>().material = materials[i];
            }
        }
    }

	//funzione per mettere il primo piano la lesione (che è l'ultimo figlio del modello 3d)
    public void GoToDesease()
    {
        GameObject child = selectedModel.transform.GetChild(selectedModel.transform.childCount - 1).gameObject;
        selectObject(child, 0.19f);
    }
}