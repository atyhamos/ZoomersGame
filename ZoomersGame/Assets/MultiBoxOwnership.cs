using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MultiBoxOwnership : MonoBehaviour
{
    private PhotonView view;
    private Photon.Realtime.Player player;
    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();
        Debug.Log("Owner of box is: " + view.Owner.ToString());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            view = GetComponent<PhotonView>();
            Debug.Log("Collision owner: " + collision.gameObject.GetComponent<PhotonView>().Owner.ToString());
            if (view.Owner == collision.gameObject.GetComponent<PhotonView>().Owner)
                return;
            Debug.Log("Transferring owners...");
            player = collision.gameObject.GetComponent<PhotonView>().Owner;
            view.RPC("TransferOwner", player);            
        }
    }

    [PunRPC]
    private void TransferOwner()
    {
        view.TransferOwnership(player);
        StartCoroutine(StartDespawnBox(this.gameObject));
    }

    private IEnumerator StartDespawnBox(GameObject box)
    {
        yield return new WaitForSeconds(8f);
        PhotonNetwork.Destroy(box);
    }
}
