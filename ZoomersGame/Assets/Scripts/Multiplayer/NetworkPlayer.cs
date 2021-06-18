using UnityEngine;
using Photon.Pun;
 
public class NetworkPlayer : MonoBehaviourPun, IPunObservable
{
    Vector2 networkPosition;
    [SerializeField] private Rigidbody2D rigidbody;
    [SerializeField] private float moveSpeed = 20;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rigidbody.position);
            stream.SendNext(rigidbody.rotation);
            stream.SendNext(rigidbody.velocity);
        }
        else
        {
            rigidbody.position = (Vector2)stream.ReceiveNext();
            rigidbody.velocity = (Vector2)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            rigidbody.position += rigidbody.velocity * lag;
        }
    }
}
