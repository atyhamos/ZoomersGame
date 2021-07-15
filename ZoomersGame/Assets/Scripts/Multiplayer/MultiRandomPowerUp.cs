using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiRandomPowerUp : MonoBehaviour
{
    [SerializeField] private MultiPowerUp CherryPowerUp;
    [SerializeField] private MultiPowerUp BoxPowerUp;
    [SerializeField] private MultiPowerUp FlyPowerUp;

    // Start is called before the first frame update

    public MultiPowerUp GetRandomPower()
    {
        int i = Random.Range(1, 4); // 4 exclusive
        switch (i)
        {
            case 1:
                return this.CherryPowerUp;
            case 2:
                return this.BoxPowerUp;
            case 3:
                return this.FlyPowerUp;
        }
        return this.CherryPowerUp; // default
    }


}
