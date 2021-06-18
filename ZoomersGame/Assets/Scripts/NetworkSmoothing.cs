//------------------------------------------------------------------------//
// NetworkSmoothing.cs
// By: Mike Loscocco
// Applies smoothing to a GameObject's movement/rotation over the network
// Attach this component to a GameObject, and drag it into its PhotonView observe field
// Replace observing this object's Transform with observing this script
//------------------------------------------------------------------------//

using UnityEngine;
using Photon.Pun;

public class NetworkSmoothing : MonoBehaviourPun, IPunObservable
{

    private Vector3 realPos = Vector3.zero;
    private Vector3 lastPos;
    private Vector3 velocity;
    private PhotonView view;

    [Range(0.0f, 1.0f)]
    public float predictionCoeff = 1.0f; //How much the game should predict an observed object's velocity: between 0 and 1

    public bool isAuthoritative = false; //Only the master client can send this object's data
    
	void Start ()
	{
	    realPos = this.transform.position;
	    //predictionCoeff = Mathf.Clamp(predictionCoeff, 0.0f, 1.0f);  //Uncomment this to ensure the prediction is clamped

        //Turn this script off if the game is being played locally
        if(PhotonNetwork.OfflineMode|| !PhotonNetwork.InRoom)
        {
            this.enabled = false;
        }
        view = GetComponent<PhotonView>();
	}
	
	public void Reset()
    {
        realPos = this.transform.position;
        lastPos = realPos;
        velocity = Vector3.zero;
    }
	
	void Update () 
    {
        lastPos = realPos;
	    if (!view.IsMine)
	    {
            //Set the position & rotation based on the data that was received
	        transform.position = Vector3.Lerp(transform.position, realPos + (predictionCoeff*velocity*Time.deltaTime), Time.deltaTime);
	    }
	}
	
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && view.IsMine)
        {
            //Send position over network
            stream.SendNext(transform.position);
            //Send velocity over network
            stream.SendNext((realPos - lastPos)/ Time.deltaTime);

        }
        else if(stream.IsReading)
        {
            //Receive positions
            realPos = (Vector3) (stream.ReceiveNext());
            //Receive velocity
            velocity = (Vector3)(stream.ReceiveNext());
        }
    }
}
