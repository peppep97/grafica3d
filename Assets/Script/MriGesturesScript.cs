using Leap;
using Leap.Unity;
using Leap.Unity.Infix;
using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Rect = UnityEngine.Rect;

public class MriGesturesScript : MonoBehaviour
{
	//dimensione del video frame
    public static int VIDEO_SIZE = 512;
    public RawImage outputImage;
    public VideoPlayer videoPlayer;

    public Text patientText;
    public Text pathologyText;
    public Text dateText;
    public Text viewText;
    public Text detailText;
    public Text label;

	//vettori contenti le clip (mri) di ogni modello
    public List<VideoClip> cervelloClips;
    public List<VideoClip> polmoniClips;
    public List<VideoClip> gastroClips;

	//per impostare le clip del modello selezioanto
    List<VideoClip> clips;


	//per impostare i fattori di proporzionalità dei 3 modelli (per effettuare la misurazione)
	//18cm / 344px (larghezza cranio medio in cm / larghezza cranio in px nella rawimage)
    //15cm / 260px
    //15cm / 133px
    public float cervelloFactor;
    public float polmoniFactor;
    public float gastroFactor;

    float factor;

    public GameObject lateralView;
    public GameObject frontalView;

	//valore di contrasto da aggiungere
    double contrast = 0;
	//per la gesture di pinch
    float lastDistanceBetweenHands = -1;

	//flag per effettuare la misurazione/screenshot
    bool measure = false;
    bool screenshot = false;
	
	//per memorizzare i punti selezionati dalle due dita (per effettuare la misurazione)
    Vector2 point1 = new Vector2(-1, -1);
    Vector2 point2 = new Vector2(-1, -1);

    Controller controller;

    Vector3 lastPalmPositionLeft = new Vector3(-1, -1, -1);
    Vector3 lastPalmPositionRight = new Vector3(-1, -1, -1);
    float time = 0f;

    // Start is called before the first frame update
    void Start()
    {
        String patientName = GameControl.control.patientName;
        String pathologyName = GameControl.control.pathologyName;
        String modelName = GameControl.control.modelName;
        String details = GameControl.control.details;

        patientText.text = patientName;
        pathologyText.text = pathologyName;
        detailText.text = details;

		//carica le clip e il fattore di proporzionalità del modello selezionato
        if (modelName.Equals("Polmoni"))
        {
            clips = polmoniClips;
            factor = polmoniFactor;
        }
        else if (modelName.Equals("Cervello"))
        {
            clips = cervelloClips;
            factor = cervelloFactor;
        }
        else
        {
            clips = gastroClips;
            factor = gastroFactor;
        }

        if (modelName.Equals("Sistema Gastrointestinale"))
        {
            viewText.text = "Vista laterale";
            videoPlayer.clip = clips[0];

            lateralView.SetActive(false);
            frontalView.SetActive(false);
        }
        else
        {
            PlayFrontalView();
        }

        videoPlayer.clip = clips[0];

		//quando è pronto a visualizzare un fotogramma che ha letto    
        videoPlayer.sendFrameReadyEvents = true; 
		//quando il fotogramma è pronto esegue la funzione
        videoPlayer.frameReady += OnNewFrame; 

        videoPlayer.Pause();
    }

    void Update()
    {
        DateTime dt = DateTime.Now;
        dateText.text = dt.ToString("dd/MM/yyyy - HH:mm");

        controller = new Controller();
        Frame frame = controller.Frame();

		//se c'è una sola mano nel frame
        if (frame.Hands.Count == 1)
        {
            Hand hand = frame.Hands[0];

			//se la mano è aperta, metti il video in play
            if (hand.GrabStrength == 0)
            {
                videoPlayer.Play();
            }
			
			//se la mano è chiusa, metti il video in pausa
            else if (hand.GrabStrength >= 0.8)
            {
                videoPlayer.Pause();
            }

        }
		//se ci sono due mani nel frame
        else if (frame.Hands.Count == 2)
        {
            Hand firstHand = frame.Hands[0];
            Hand secondHand = frame.Hands[1];

			//se entrambi gli indici sono estesi e la funzionalità di misura è abilitata -> gesture per calcolare la distanza
            if (firstHand.Fingers[1].IsExtended && secondHand.Fingers[1].IsExtended && measure)
            {
                videoPlayer.Pause();

				//calcola il punto selezionato dalla mano sinistra nella raw image
                Vector3 pos1 = firstHand.Fingers[1].TipPosition.ToVector3();
                pos1.x += Screen.width / 2;
                pos1.y += 20;
				//converti le coordinate del punto del dito indice in coordinate nella raw image del video
                RectTransformUtility.ScreenPointToLocalPointInRectangle(outputImage.rectTransform, pos1.ToVector2(), Camera.main, out point1);

				//per spostare l'origine nel vertice in alto a destra della raw image
                point1.x += VIDEO_SIZE / 2;
                point1.y -= VIDEO_SIZE / 2;
                point1.y = -point1.y;

				//calcola il punto selezionato dalla mano destra nella raw image
                Vector3 pos2 = secondHand.Fingers[1].TipPosition.ToVector3();
                pos2.x += Screen.width / 2;
                pos2.y += 20;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(outputImage.rectTransform, pos2.ToVector2(), Camera.main, out point2);

                point2.x += VIDEO_SIZE / 2;
                point2.y -= VIDEO_SIZE / 2;
                point2.y = -point2.y;

                if (lastPalmPositionLeft.Equals(new Vector3(-1, -1, -1)))
                {
                    lastPalmPositionLeft = firstHand.PalmPosition.ToVector3();
                }

                if (lastPalmPositionRight.Equals(new Vector3(-1, -1, -1)))
                {
                    lastPalmPositionRight = secondHand.PalmPosition.ToVector3();
                }

                //calcola la variazione di spostamento di entrambe le mani
                Vector3 differenceLeft = firstHand.PalmPosition.ToVector3() - lastPalmPositionLeft;
                Vector3 differenceRight = secondHand.PalmPosition.ToVector3() - lastPalmPositionRight;

				//se entrambe le mani non si sono spostate per 3s, abilita il flag per fare lo screenshot
                if (differenceLeft.x.Abs() < 1 && differenceLeft.y.Abs() < 1 && differenceLeft.z.Abs() < 1
                && differenceRight.x.Abs() < 1 && differenceRight.y.Abs() < 1 && differenceRight.z.Abs() < 1)
                {

                    time += Time.deltaTime;

                    if (time >= 3f)
                    {
                        screenshot = true;
                        time = 0f;
                    }
                }
                else
                {
					//altrimenti resetta il timer se c'è stato spostamento nel mentre
                    time = 0f;
                    lastPalmPositionLeft = new Vector3(-1, -1, -1);
                    lastPalmPositionRight = new Vector3(-1, -1, -1);
                }

				//aggiorna le posizioni della mano con quelle correnti
                lastPalmPositionLeft = firstHand.PalmPosition.ToVector3();
                lastPalmPositionRight = secondHand.PalmPosition.ToVector3();

				//imposta il frame del video
                SetFrame();
            }
			//se entrambe le mani non hanno il pugno chiuso e hanno il dito indice a contatto con il pollice (pinchStrength > 0.5) -> gesture di pinch 
            else if (firstHand.PinchStrength > 0.5 && firstHand.GrabStrength < 0.7 && secondHand.PinchStrength > 0.5 && secondHand.GrabStrength < 0.7
                 && !firstHand.Fingers[1].IsExtended && !secondHand.Fingers[1].IsExtended && !measure)
            {

                float distanceBetweenHands = firstHand.PalmPosition.DistanceTo(secondHand.PalmPosition);

                if (lastDistanceBetweenHands == -1)
                {
                    lastDistanceBetweenHands = distanceBetweenHands;
                }

                float difference = distanceBetweenHands - lastDistanceBetweenHands;
                lastDistanceBetweenHands = distanceBetweenHands;

				//la differenza di spostamento si usa come offset da aggiungere al contrasto
                contrast += difference;

				//per impostare delle soglie massime/minime
                if (contrast > 70)
                {
                    contrast = 70;
                }
                else if (contrast < 0)
                {
                    contrast = 0;
                }

				//se il video è in pausa, aggiorna manualmente il frame
                if (videoPlayer.isPaused)
                {
                    SetFrame();
                }
            }
        }

		//scrivi il valore del contrasto nella label (in basso a sinistra)
        if (contrast > 0)
        {
            label.text = "Contrasto: +" + (int)contrast;
        }
        else
        {
            label.text = "";
        }
    }

    private void SetFrame()
    {
        RenderTexture videoFrame = videoPlayer.texture as RenderTexture;

        //la texture è un file immagine che utilizziamo per dipingere un oggetto. La renderTexture 
        //è la stessa cosa ma prima, quando risiede da qualche parte in memoria. Possiamo quindi 
        // accedere ai valori dei colori dei singoli pixwl e copiarli.
        // Per recuperarla in memoria bisogna fermarla. quando in memoria è pronta, viene emanato
        // un segnale per mandarla affinchè venga visualizzata. La memoria si svuota e si comincia
        // a disegnare l'immagine successiva. Questo avviene molte volte al secondo. 

        // Bisogna allertare Unity per poterla fermare

        RenderTexture.active = videoFrame;
        //si rende attiva la zona di memoria per poterci accedere e non viene modificata
        //fino a quando ci sto lavorando

        //creiamo una texture tex che andrà riempita da pixel colorati
        Texture2D tex = new Texture2D(videoFrame.width, videoFrame.height, TextureFormat.RGB24, false); //in ingresso larghezza ed
                                                                                                        //altezza e un format (immagine RGB a 24 bit)

        //si vanno a colorare i pixel

        tex.ReadPixels(new Rect(0, 0, videoFrame.width, videoFrame.height), 0, 0, false); //legge i pixel fermi in memoria
        //bisogna specificare il rettangolo entro cui cadono i pixels (Rect) che parte da 0,0 e si estende per lunghezza e altezza

        //possiamo poi liberare la memoria
        RenderTexture.active = null;
        Mat img = OpenCvSharp.Unity.TextureToMat(tex);
        Destroy(tex); //si distrugge per non riempire la memoria con roba che non viene utilizzata

        var modifiedSrc = new Mat();

        //aggiorna la MAT cambiando il contrasto
        UpdateBrightnessContrast(img, modifiedSrc, 0, contrast);

		//converti i punti letti in punti di OpenCv
        OpenCvSharp.Point p1 = new OpenCvSharp.Point(point1.x, point1.y);
        OpenCvSharp.Point p2 = new OpenCvSharp.Point(point2.x, point2.y);

		//disegna i punti
        modifiedSrc.Circle(p1, 3, Scalar.DarkRed, 1, LineTypes.AntiAlias);
        modifiedSrc.Circle(p2, 3, Scalar.DarkRed, 1, LineTypes.AntiAlias);

		//disegna la linea tra i due punti
        modifiedSrc.Line(p1, p2, Scalar.Red, 3, LineTypes.AntiAlias);
		
		//calcola la dimensione in base alla distanza tra i due punti (moltiplicata per il fattore di proporzionalità)
        float distance = Vector2.Distance(point1, point2) * factor;

        if (distance > 0)
        {
			//scrivi la dimensione nell'immagine
            modifiedSrc.PutText("Dimensione: " + Math.Round(distance, 2) + "cm", new OpenCvSharp.Point(5, 15), HersheyFonts.Italic, 0.5, Scalar.Red, 1, LineTypes.AntiAlias);

			//se il flag di screenshot è abilitato, converti la mat in texture, codificala in PNG e salvala nella cartella del progetto
            if (screenshot)
            {
                byte[] bytes = OpenCvSharp.Unity.MatToTexture(modifiedSrc).EncodeToPNG();
                var dirPath = Application.dataPath + "/../SaveImages/";
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                File.WriteAllBytes(dirPath + DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ".png", bytes);

				//dopo aver fatto lo screenshot, reimposta i flag a false e i punti
                screenshot = false;
                measure = false;

                point1 = new Vector2(-1, -1);
                point2 = new Vector2(-1, -1);
            }
        }
		
		//imposta la texture della raw image con quella modificata da OpenCv
        outputImage.texture = OpenCvSharp.Unity.MatToTexture(modifiedSrc);
    }

	//funzione chiamata ogni volta che cambia il frame nel videoplayer 
    private void OnNewFrame(VideoPlayer source, long frameidx)
    {
		//chiama la set frame per aggiornare il frame nella raw image del video
        SetFrame();
    }

	//funzione per riprodurre la vista frontale
    public void PlayFrontalView()
    {
        viewText.text = "Vista frontale";
        videoPlayer.clip = clips[0];
    }

	//funzione per riprodurre la lista laterale
    public void PlayLateralView()
    {
        viewText.text = "Vista laterale";
        videoPlayer.clip = clips[1];
    }

	//funzione per variare il contrasto della matrice dell'immagine
    private static void UpdateBrightnessContrast(Mat src, Mat modifiedSrc, int brightness, double contrast)
    {
		//varia i paramentri alpha e beta
		//alpha influenza il contrasto
		//beta influenza la luminosità
        double alpha, beta;
        if (contrast > 0)
        {
            double delta = 127f * contrast / 100f;
            alpha = 255f / (255f - delta * 2);
            beta = alpha * (brightness - delta);
        }
        else
        {
            double delta = -128f * contrast / 100;
            alpha = (256f - delta * 2) / 255f;
            beta = alpha * brightness + delta;
        }
		//memorizza il risultato in modifiedSrc
        src.ConvertTo(modifiedSrc, MatType.CV_8UC3, alpha, beta);
    }

	//funzione per resettare il contrasto quando viene premuto il bottone
    public void OnChangeContrast()
    {
        contrast = 0;
    }

	//funzione per abilitare/disabilitare la funzionalità di misurazione quando viene premuto il bottone
    public void OnMeasure()
    {
        measure = !measure;
    }
}
