using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using Leap.Unity.Infix;
using System;
using UnityEngine.UI;

public class GesturesScript : MonoBehaviour
{
    public GameObject polmoniObject;
    public GameObject cervelloObject;
    public GameObject gastroObject;
    public List<Material> polmoniMaterials;
    public List<Material> cervelloMaterials;
    public List<Material> gastroMaterials;

    public GameObject mainModel;

    public Text patientText;
    public Text pathologyText;
    public Text dateText;

    public Text label;

    Controller controller;
    Vector VectorHandPosRight;
    Vector VectorHandPosLeft;
    GameObject selectedModel;
    GameObject highlightedModel;

    bool isOpaque = true;

    List<Material> materials;

    Vector3 centerInParent = new Vector3(0f, -0.05f, 0.6f);
    Vector3 defaultPosition;
    Quaternion defaultRotation = new Quaternion(0f, 0f, 0f, 0f);
    Quaternion defaultChildRotation = new Quaternion(-90f, 0f, 0f, 0f);

    float lastDistanceBetweenHands = -1;
    //float lastZDirectionLeftHand = -1;
    float lastYDirectionLeftHand = -1;
    float lastXDirectionLeftHand = -1;

    Vector3 lastPalmVelocity = new Vector3(-1, -1, -1);
    float time = 0f;

    public HandModelBase HandModel = null;
    Vector3 center;


    void Start()
    {
        //Read data through a local variable
        /*String patientName = GameControl.control.patientName;
        String pathologyName = GameControl.control.pathologyName;
        String modelName = GameControl.control.modelName;

        patientText.text = patientName;
        pathologyText.text = pathologyName;

        polmoniObject.SetActive(false);
        cervelloObject.SetActive(false);
        gastroObject.SetActive(false);

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
        }*/

        mainModel = polmoniObject;
        materials = polmoniMaterials;

        selectedModel = mainModel;
        defaultPosition = selectedModel.transform.position;
    }

    void Update()
    {
        DateTime dt = DateTime.Now;
        dateText.text = dt.ToString("dd/MM/yyyy - HH:mm");

        controller = new Controller();
        Frame frame = controller.Frame();
        List<Hand> hands = frame.Hands;

        if (frame.Hands.Count == 1)
        {
            fingerPointingWatcher(hands[0]);
        }

        if (frame.Hands.Count == 2)
        {
            if (highlightedModel != null)
            {
                highlightedModel.GetComponent<Outline>().OutlineMode = Outline.Mode.OutlineHidden;
            }


            Hand firstHand = hands[0];
            Hand secondHand = hands[1];

            VectorHandPosLeft = firstHand.PalmPosition; //position right hand
            VectorHandPosRight = secondHand.PalmPosition; // position left hand

            if (firstHand.GrabStrength > 0.7 && secondHand.GrabStrength > 0.7)
            {
                //lastXDirectionLeftHand = -1;
                //lastYDirectionLeftHand = -1;
                lastDistanceBetweenHands = -1;

                Debug.Log("pan gesture");

                Vector palmPosition = firstHand.PalmPosition;

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

                float offsetX = differenceX / 200;
                float offsetY = differenceY / 200;


                float CamX = selectedModel.transform.position.x;
                float CamY = selectedModel.transform.position.y;
                float CamZ = selectedModel.transform.position.z;

                selectedModel.transform.position = new Vector3(CamX + offsetX, CamY + offsetY, CamZ); //Move the object
            }
            else if (firstHand.PinchStrength > 0.5 && firstHand.GrabStrength < 0.7 && secondHand.PinchStrength > 0.5 && secondHand.GrabStrength < 0.7)
            {

                lastXDirectionLeftHand = -1;
                lastYDirectionLeftHand = -1;

                float distanceBetweenHands = VectorHandPosLeft.DistanceTo(VectorHandPosRight); //distance position between hands as float

                if (lastDistanceBetweenHands == -1)
                {
                    lastDistanceBetweenHands = distanceBetweenHands;
                }

                float difference = distanceBetweenHands - lastDistanceBetweenHands;
                lastDistanceBetweenHands = distanceBetweenHands;

                Debug.Log("pinching: " + difference);

                if (difference != 0)
                {
                    float R = -difference / 100;

                    float CamX = selectedModel.transform.position.x;                      //Get current camera postition for the offset
                    float CamY = selectedModel.transform.position.y;                      //^
                    float CamZ = selectedModel.transform.position.z;                      //^
                    selectedModel.transform.position = new Vector3(CamX, CamY, CamZ + R);//Move the object
                }
            }
            else if (firstHand.IsLeft && firstHand.PinchStrength < 0.5 && firstHand.GrabStrength <= 0.7 && secondHand.GrabStrength > 0.7)
            {
                lastDistanceBetweenHands = -1;

                Vector palmPosition = firstHand.PalmPosition;

                float zDirectionLeftHand = palmPosition.z;
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

                //find mass center
                /*if (selectedModel == mainModel)
                {
                    Vector3 position1 = selectedModel.GetComponentsInChildren<Renderer>()[0].bounds.center;
                    Vector3 position2 = selectedModel.GetComponentsInChildren<Renderer>()[1].bounds.center;
                    Vector3 position3 = selectedModel.GetComponentsInChildren<Renderer>()[2].bounds.center;

                    center = (position1 + position2 + position3) / 3;
                }
                else
                {
                    center = selectedModel.GetComponent<Renderer>().bounds.center;
                }*/

                //myModel.transform.Rotate(differenceY / 2, differenceX / 2, 0);
                selectedModel.transform.RotateAround(center, Vector3.up, differenceX / 2);
                selectedModel.transform.RotateAround(center, Vector3.right, differenceY / 2);

            }
            else
            {
                //reset last distance if gesture is finished
                lastDistanceBetweenHands = -1;
                lastXDirectionLeftHand = -1;
                lastYDirectionLeftHand = -1;

            }
        }
    }

    private void fingerPointingWatcher(Hand hand)
    {
        Vector3 targetDirection;

        if (HandModel != null && HandModel.IsTracked)
        {
            targetDirection = Camera.main.transform.TransformDirection(Vector3.forward);

            Finger finger = HandModel.GetLeapHand().Fingers[1];

            if (finger.IsExtended)
            {

                if (lastPalmVelocity.Equals(new Vector3(-1, -1, -1)))
                {
                    lastPalmVelocity = hand.PalmPosition.ToVector3();
                }

                //check if palm position variation
                Vector3 difference = hand.PalmPosition.ToVector3() - lastPalmVelocity;
                if (difference.x.Abs() < 1 && difference.y.Abs() < 1 && difference.z.Abs() < 1)
                {
                    RaycastHit hit;

                    if (Physics.Raycast(finger.TipPosition.ToVector3(), targetDirection, out hit))
                    {
                        Debug.Log("hit");
                        if (hit.collider.gameObject.transform.parent.gameObject == mainModel && selectedModel == mainModel)
                        {
                            highlightedModel = hit.collider.gameObject;
                            highlightObject();

                            time += Time.deltaTime;

                            if (time >= 2f)
                            {
                                selectObject(hit.collider.gameObject);
                                time = 0f;
                            }
                        }
                    }
                    else
                    {
                        if (highlightedModel != null)
                        {
                            highlightedModel.GetComponent<Outline>().OutlineMode = Outline.Mode.OutlineHidden;
                        }
                    }
                }
                else
                {
                    time = 0f;
                    lastPalmVelocity = new Vector3(-1, -1, -1);
                }

                lastPalmVelocity = hand.PalmPosition.ToVector3();
            }
        }
    }

    private void highlightObject()
    {
        highlightedModel.GetComponent<Outline>().OutlineMode = Outline.Mode.OutlineAll;

        Component[] objectsOutline = selectedModel.GetComponentsInChildren<Outline>();
        foreach (Outline item in objectsOutline)
        {
            if (item.gameObject != highlightedModel)
            {
                item.OutlineMode = Outline.Mode.OutlineHidden;
            }
        }
    }


    private void selectObject(GameObject selected)
    {
        Component[] objectsRenderer = selectedModel.GetComponentsInChildren<Renderer>();
        foreach (Renderer item in objectsRenderer)
        {
            if (item.gameObject != selected)
            {

                if (item.gameObject.name.Equals("lungs_4_SN"))
                {
                    item.gameObject.GetComponent<Renderer>().material = materials[1];
                }
                else if (item.gameObject.name.Equals("lungs_4_DX"))
                {
                    item.gameObject.GetComponent<Renderer>().material = materials[1];

                }
                else if (item.gameObject.name.Equals("lungs_4_trachea"))
                {
                    item.gameObject.GetComponent<Renderer>().material = materials[3];
                }
                else
                {
                    item.gameObject.GetComponent<Renderer>().material = materials[5];
                }
            }
        }

        selectedModel.GetComponent<GlideController>().SetDestination(centerInParent);

        selectedModel = selected;

    }

    public void onOpaquePressed()
    {
        isOpaque = !isOpaque;
        setOpaque(isOpaque);
        
    }

    public void resetMainObject()
    {
        Debug.Log("click");

        GameObject child = selectedModel.transform.GetChild(2).gameObject;
        center = child.GetComponent<Renderer>().bounds.center;

        Debug.Log(child.transform.localPosition + " - " + selectedModel.transform.position);

        //selectedModel.transform.position = child.transform.localPosition + defaultPosition;

       /* Debug.Log("click");
        if (selectedModel != mainModel)
        {
            selectedModel.transform.localPosition = new Vector3(0f, 0f, 0f);
            //selectedModel.transform.localRotation = defaultChildRotation;
            selectedModel.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);

            selectedModel = mainModel;
            setOpaque(true);
            selectedModel.GetComponent<GlideController>().SetDestination(defaultPosition);
            //selectedModel.GetComponent<GlideController>().SetRotation(defaultRotation);
            selectedModel.transform.rotation = defaultRotation;
        }*/
    }

    private void setOpaque(bool opaque)
    {

        for (int i = 0; i < selectedModel.transform.childCount - 1; i++)
        {
            GameObject child = selectedModel.transform.GetChild(i).gameObject;

            Debug.Log(child.transform.name);

            if (!opaque)
            {
                if (child.transform.childCount > 0)
                {
                    for (int j = 0; j < child.transform.childCount; j++)
                    {
                        child.transform.GetChild(j).gameObject.GetComponent<Renderer>().material = materials[i + selectedModel.transform.childCount - 1];
                    }
                }
                else
                {
                    child.GetComponent<Renderer>().material = materials[i + selectedModel.transform.childCount - 1];

                }
            }
            else
            {
                if (child.transform.childCount > 0)
                {
                    for (int j = 0; j < child.transform.childCount; j++)
                    {
                        child.transform.GetChild(j).gameObject.GetComponent<Renderer>().material = materials[i];
                    }
                }
                else
                {
                    child.GetComponent<Renderer>().material = materials[i];

                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (HandModel.IsTracked)
        {
            Finger finger = HandModel.GetLeapHand().Fingers[1];
            Gizmos.color = Color.white;
            Gizmos.DrawRay(finger.TipPosition.ToVector3(), Camera.main.transform.TransformDirection(Vector3.forward));
        }
    }

    /*void OnGUI()
    {
        GameObject troncoEncefalico = mainModel.transform.GetChild(2).gameObject;
        Vector3 point = Camera.main.WorldToScreenPoint(troncoEncefalico.transform.position + offset);

        rect.x = point.x;
        rect.y = Screen.height - point.y - rect.height; // bottom left corner set to the 3D point
        GUI.Label(rect, troncoEncefalico.transform.name); // display its name, or other string
    }*/
}