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
    public Text detailText;
    public Text label;

    Controller controller;
    Vector VectorHandPosRight;
    Vector VectorHandPosLeft;
    GameObject selectedModel;
    GameObject selectedChild;
    GameObject highlightedModel;

    bool isOpaque = true;

    List<Material> materials;

    Vector3 defaultPosition;
    Quaternion defaultRotation;

    float lastDistanceBetweenHands = -1;
    //float lastZDirectionLeftHand = -1;
    float lastYDirectionLeftHand = -1;
    float lastXDirectionLeftHand = -1;

    Vector3 lastPalmPosition = new Vector3(-1, -1, -1);
    float time = 0f;

    public HandModelBase HandModel = null;

    void Start()
    {
        //Read data through a local variable
        String patientName = GameControl.control.patientName;
        String pathologyName = GameControl.control.pathologyName;
        String modelName = GameControl.control.modelName;
        String details = GameControl.control.details;

        patientText.text = patientName;
        pathologyText.text = pathologyName;
        detailText.text = details;

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
        }

        selectedModel = mainModel;
        defaultPosition = selectedModel.transform.position;
        defaultRotation = selectedModel.transform.rotation;

        selectedChild = selectedModel;

        label.text = selectedModel.transform.name;
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

                selectedModel.transform.Rotate(differenceY / 2, differenceX / 2, 0);

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
                bool cont = true;
                foreach (Finger f in HandModel.GetLeapHand().Fingers)
                {
                    if (f != finger && f.IsExtended)
                    {
                        cont = false;
                    }
                }
                if (cont)
                {
                    if (lastPalmPosition.Equals(new Vector3(-1, -1, -1)))
                    {
                        lastPalmPosition = hand.PalmPosition.ToVector3();
                    }

                    //check if palm position variation
                    Vector3 difference = hand.PalmPosition.ToVector3() - lastPalmPosition;
                    if (difference.x.Abs() < 1 && difference.y.Abs() < 1 && difference.z.Abs() < 1)
                    {
                        RaycastHit hit;


                        if (Physics.Raycast(finger.TipPosition.ToVector3(), targetDirection, out hit))
                        {
                            if (hit.collider.gameObject == selectedModel || hit.collider.gameObject.transform.parent.gameObject == selectedModel)
                            {
                                highlightedModel = hit.collider.gameObject;
                                label.text = hit.collider.gameObject.name;
                                highlightObject();

                                time += Time.deltaTime;

                                if (time >= 2f)
                                {
                                    selectObject(hit.collider.gameObject, 0.57f);
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
                            if (selectedChild != null)
                            {
                                label.text = selectedChild.transform.name;
                            }
                        }
                    }
                    else
                    {
                        time = 0f;
                        lastPalmPosition = new Vector3(-1, -1, -1);
                    }

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

    private void highlightObject()
    {
        if (highlightedModel != null)
        {
            highlightedModel.GetComponent<Outline>().OutlineMode = Outline.Mode.OutlineAll;
        }

        Component[] objectsOutline = selectedModel.GetComponentsInChildren<Outline>();
        foreach (Outline item in objectsOutline)
        {
            if (item.gameObject != highlightedModel)
            {
                item.OutlineMode = Outline.Mode.OutlineHidden;
            }
        }
    }

    public void onOpaquePressed()
    {
        isOpaque = !isOpaque;
        setOpaque(isOpaque, null);

    }

    public static Vector3 getRelativePosition(Transform origin, Vector3 position)
    {
        Vector3 distance = position - origin.position;
        Vector3 relativePosition = Vector3.zero;
        relativePosition.x = Vector3.Dot(distance, origin.right.normalized);
        relativePosition.y = Vector3.Dot(distance, origin.up.normalized);
        relativePosition.z = Vector3.Dot(distance, origin.forward.normalized);

        return relativePosition;
    }

    public void selectObject(GameObject child, float zoom)
    {
        selectedModel.GetComponent<AnimationController>().SetDestination(defaultPosition);

        label.text = child.gameObject.name;
        Vector3 center = child.GetComponent<Renderer>().bounds.center;

        Vector3 offset = getRelativePosition(selectedModel.transform, center);
        Vector3 newPos = new Vector3(defaultPosition.x + offset.x, defaultPosition.y + offset.y, zoom);

        selectedModel.GetComponent<AnimationController>().SetDestination(newPos);

        isOpaque = false;
        setOpaque(isOpaque, child);

        selectedChild = child;
    }

    public void resetMainObject()
    {
        selectedModel.transform.rotation = defaultRotation;
        selectedModel.GetComponent<AnimationController>().SetDestination(defaultPosition);

        label.text = selectedModel.transform.name;

        isOpaque = true;
        setOpaque(isOpaque, null);

        selectedChild = selectedModel;
    }

    private void setOpaque(bool opaque, GameObject exclude)
    {
        for (int i = 0; i < selectedModel.transform.childCount - 1; i++)
        {
            GameObject child = selectedModel.transform.GetChild(i).gameObject;

            if (!opaque)
            {
                if (child != exclude)
                {
                    child.GetComponent<Renderer>().material = materials[i + selectedModel.transform.childCount - 1];
                }
                else
                {
                    child.GetComponent<Renderer>().material = materials[i];
                }
            }
            else
            {
                child.GetComponent<Renderer>().material = materials[i];
            }
        }
    }

    public void GoToDesease()
    {
        GameObject child = selectedModel.transform.GetChild(selectedModel.transform.childCount - 1).gameObject;
        selectObject(child, 0.19f);
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