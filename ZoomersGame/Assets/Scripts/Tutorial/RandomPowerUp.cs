using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPowerUp : MonoBehaviour
{
    [SerializeField] private PowerUp CherryPowerUp;
    [SerializeField] private PowerUp BoxPowerUp;

    // Start is called before the first frame update
    
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
}
