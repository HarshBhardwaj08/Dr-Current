using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject player;
    public GameObject postiveBar;
    public GameObject UpCam;
    public GameObject Buld;
    public GameObject Resistor;
    void OnEnable()
    {
       
       Movement.OnItemPickedUp += UpdateScence;
    }

    void OnDisable()
    {
      
        Movement.OnItemPickedUp -= UpdateScence;
    }

  
    void UpdateScence()
    {
        Debug.Log("UI updated after collecting an item.");
        player.SetActive(false);
        postiveBar.SetActive(true);
        UpCam.SetActive(true);
        Resistor.GetComponent<BoxCollider>().enabled = false;
        Buld.GetComponent<BoxCollider>().enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
