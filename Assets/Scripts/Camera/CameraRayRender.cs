using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRayRender : MonoBehaviour
{
    [SerializeField] private GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnPostRender()
    {
        gameController.DrawRays();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
