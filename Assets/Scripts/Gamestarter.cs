using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gamestarter : MonoBehaviour
{
    bool startPlaying = false;
    float gameTime = 0;
    public static Gamestarter instance;
    public GameObject player;
    public GameObject mainCamera;
    public float cylinderHeight;
    public float cylinderRadius;
    public float cameraDistance;
    public float movementSensitivity;
    float playerRadian;
    public GameObject targetFab;
    GameObject cylinder;
    GameObject target;
    float targetRadian = 0;
    int targetsHit;

    int misses;
    int badhits;
    int okhits;
    int goodhits;

    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;
    public GameObject panel;

    public MMF_Player MissFeedback;
    public MMF_Player BadhitFeedback;
    public MMF_Player OKhitFeedback;
    public MMF_Player GoodhitFeedback;



    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        cylinderHeight = MenuScript.instance.cylinderHeight / 2;
        cylinderRadius = MenuScript.instance.cylinderRadius;
        cameraDistance = MenuScript.instance.cameraDistance;
        movementSensitivity = MenuScript.instance.movementSensitivity / 10;

        // Summons cylinder
        cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.localScale = new Vector3(cylinderRadius * 2, cylinderHeight, cylinderRadius * 2);

        // Set player pos to (0 radian, levelWidth distance) polar coordinate on the bottom of the cylinder
        Vector3 TargetLocation = new Vector3(polarToCartesian_X(0f, cylinderRadius), cylinder.transform.position.y - (cylinderHeight), polarToCartesian_Z(0f, cylinderRadius));
        player.transform.SetLocalPositionAndRotation(TargetLocation, Quaternion.identity);


    }

    // Update is called once per frame
    void Update()
    {
     // If we've been told to start playing and the the player hasn't hit all targets yet, perform all the movement actions for player and camera + increment gametime
        if (startPlaying && targetsHit < (2*cylinderHeight))
        {
            gameTime += Time.deltaTime;

            // gets mouse movement
            float mouseX = Input.GetAxis("Mouse X") * movementSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * movementSensitivity;

            // modifies radian based on mouse input, makes sure it doesnt overflow
            playerRadian += mouseX;
            if (Mathf.Abs(playerRadian) > 2*Mathf.PI) { playerRadian = 0; }
            
            // calculate new position based on modified radian value
            float newX = polarToCartesian_X(playerRadian, cylinderRadius);
            float newZ = polarToCartesian_Z(playerRadian, cylinderRadius);

            // update player position
            player.transform.position = new Vector3(newX, player.transform.position.y, newZ);
            player.transform.LookAt(new Vector3(0, player.transform.position.y, 0));

            // do the same for camera but with a wider radius so its further out
            float cameraNewX = polarToCartesian_X(playerRadian, cylinderRadius + cameraDistance);
            float cameraNewZ = polarToCartesian_Z(playerRadian, cylinderRadius + cameraDistance);

            mainCamera.transform.position = new Vector3(cameraNewX, player.transform.position.y + 2f, cameraNewZ);
            // and make it look at the player
            mainCamera.transform.LookAt(player.transform.position);

            // check for player input
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                // calculate absolute difference to target. Done differently depending if playerradian is negative or not (this is definitely wrong lol)
                // !!theres probably a function in unity to get the absolute distance to target, this would make precision more predictable (meaning big cylinders require more precision -> is our way of increasing difficulty)
                float difference;
                if (playerRadian < 0) { difference = Mathf.Abs((2*Mathf.PI) + playerRadian - targetRadian); }
                else { difference = Mathf.Abs(targetRadian - playerRadian); }

                // hit or miss, i guess they never miss huh
                if (difference > 0.5f) 
                {
                    Miss();
                }
                else
                {
                    Hit(difference);
                }
            }
        } 
        else if (targetsHit == 2*cylinderHeight){ endLevel(); }
    }

    float polarToCartesian_X(float angle, float radius) { return Mathf.Cos(angle) * radius; }
    float polarToCartesian_Z(float angle, float radius) { return Mathf.Sin(angle) * radius; }

    public void startLevel()
    {
        startPlaying = true;

        spawnTarget();
    }

    void endLevel() 
    { 
        //Time.timeScale = 0;

        string timetaken = "Time taken: " + gameTime.ToString("0.00") + " seconds";
        string percentage = "Accuracy percentage: " + CalculatePercentage() + "%";   

        text1.text = timetaken;
        text2.text = percentage;

        panel.SetActive(true);
    }

    public void resetLevel()
    {
        SceneManager.LoadScene(1);
    }

    public void goToMenu()
    {
        SceneManager.LoadScene(0);
    }

    float CalculatePercentage()
    {
        // Calculate total points earned from hits
        float totalPoints = (goodhits * 100) + (okhits * 90) + (badhits * 75);

        // Calculate the total possible points, including misses
        float maxPossiblePoints = (2*cylinderHeight + misses) * 100;

        // Calculate the percentage
        float percentage = (totalPoints / maxPossiblePoints) * 100;

        // Ensure percentage is between 0% and 100%
        percentage = Mathf.Clamp(percentage, 0f, 100f);

        return percentage;
    }

    void spawnTarget()
    {
        Destroy(target);
        //spawn Target prefab 1 unit above player at a random radian
        targetRadian = Random.Range(0, 2 * Mathf.PI);
        Vector3 wherePosition = new Vector3(polarToCartesian_X(targetRadian, cylinderRadius), player.transform.position.y+1, polarToCartesian_Z(targetRadian, cylinderRadius));
        target = Instantiate(targetFab, wherePosition, Quaternion.identity);
        target.transform.LookAt(new Vector3(0, target.transform.position.y,0));
    }

    void Hit(float misaim)
    {
        targetsHit++;

        // send player up
        player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y+1f, player.transform.position.z);

        // scoring branches... probably not final score system but wutevs
        // UPDATE: We should definitely make scoring dependent on the size of the cylinder. Bigger cylinder, smaller score window. Makes big cylinders a precision challenge
        // (because if they share scoring thresholds with small cylinders, youll be able to hit way outside of targets on big cylinders since 0.5 radians just translates to further away
        if (misaim > 0.25f)
        {
            BadhitFeedback?.PlayFeedbacks();
            badhits++;
        }
        else if (misaim > 0.1f)
        {
            OKhitFeedback?.PlayFeedbacks();
            okhits++;
        }
        else
        {
            GoodhitFeedback?.PlayFeedbacks();
            goodhits++;
        }

        if (targetsHit < (2 * cylinderHeight)) { spawnTarget(); }
        
    }

    void Miss()
    {
        MissFeedback?.PlayFeedbacks();
        misses++;
    }
}
