using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPowerUp : MonoBehaviour
{
    private PowerUp CherryPowerUp;
    private PowerUp BoxPowerUp;

    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(FindPowerUps());

    }
    public PowerUp GetRandomPower()
    {
        int i = Random.Range(1, 3); // 3 exclusive
        switch (i)
        {
            case 1:
                return this.CherryPowerUp;
            case 2:
                return this.BoxPowerUp;
        }
        return this.CherryPowerUp; // default
    }

    private IEnumerator FindPowerUps()
    {
        yield return new WaitForSeconds(5f);
        CherryPowerUp = GameObject.Find("Cherry").GetComponent<CherryPowerUp>();
        BoxPowerUp = GameObject.Find("Box (PowerUp)").GetComponent<BoxPowerUp>();
    }
}
