using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public static MenuScript instance;
    
    public float cylinderHeight = 10;
    public float cylinderRadius = 4;
    public float cameraDistance = 3;
    public float movementSensitivity = 3;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    public void updateHeight(string height)
    {
        cylinderHeight = int.Parse(height);
    }

    public void updateRadius(string radius) 
    {
        cylinderRadius = float.Parse(radius);
    }

    public void updateDistance(string distance) 
    {
        cameraDistance = float.Parse(distance);
    }

    public void updateSensitivity(string sensitivity)
    {
        movementSensitivity = float.Parse(sensitivity);
    }

    public void LoadGame()
    {
        SceneManager.LoadScene(1);
    }
}
