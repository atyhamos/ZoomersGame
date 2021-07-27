    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MultiBoundsCheck : MonoBehaviour
{
    public static MultiBoundsCheck instance;
    [SerializeField] private float extraAllowance = 2;
    float verticalExtent;
    float horizontalExtent;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        verticalExtent = Camera.main.orthographicSize;
        horizontalExtent = verticalExtent * Screen.width / Screen.height;
    }

    public void UpdateBounds()
    {
        verticalExtent = Camera.main.orthographicSize;
        horizontalExtent = verticalExtent * Screen.width / Screen.height;
    }

    public bool WithinBounds(Transform player)
    {
        float leftBound = Camera.main.transform.position.x - horizontalExtent - extraAllowance;
        float rightBound = Camera.main.transform.position.x + horizontalExtent + extraAllowance;
        float bottomBound = Camera.main.transform.position.y - verticalExtent - extraAllowance;
        float topBound = Camera.main.transform.position.y + verticalExtent + extraAllowance;
        return player.position.x >= leftBound && player.position.x <= rightBound
            && player.position.y >= bottomBound && player.position.y <= topBound;
    }

    public void UpdateSize(CinemachineVirtualCamera vcam, float size)
    {
        float initial = vcam.m_Lens.OrthographicSize;
        vcam.m_Lens.OrthographicSize = Mathf.Lerp(initial, size, 0.05f);
    }

    // Update is called once per frame
    void Update()
    {
       // Debug.Log($"({Camera.main.transform.position.x - horizontalExtent}, {Camera.main.transform.position.x + horizontalExtent})\n" +
       //     $"({Camera.main.transform.position.y - verticalExtent}, {Camera.main.transform.position.y + verticalExtent})");   
    }
}
