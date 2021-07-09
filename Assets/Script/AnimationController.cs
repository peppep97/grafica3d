using System.Security.Cryptography;
using UnityEngine;

//viene aggiunto come componente ad ogni modello 3d
public class AnimationController : MonoBehaviour
{
	//velocità di spostamento
    public float speed;

	//posizione finale
    public Vector3 destination;
	//quando start vale true, l'oggetto viene spostato verso destination
    private bool start = false;

    void Start()
    {
		//imposta come destinazione quella corrente, perché l'oggetto non deve spostarsi
        destination = gameObject.transform.position;    
    }

    void Update()
    {
		//se la destinazione impostata è diversa da quella corrente e start vale true, l'oggetto viene spostato verso la destinazione
		//con la funzione MoveTowards, che sposta l'oggetto di una quantità pari a Time.deltaTime * speed ad ogni update, verso la destinazione finale
        if (destination != gameObject.transform.position)
        {
            if (start)
            {
                transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * speed);
            }
        }
        else
        {
			//appena arriva a destinazione, imposta start a false
            start = false;
        }
    }
	
	//viene chiamata dall'esterno (sul componente AnimationController dell'oggetto corrente) e serve ad impostare la destinazione dell'oggetto
    public void SetDestination(Vector3 value)
    {
        start = true;
        destination = value;
    }
}