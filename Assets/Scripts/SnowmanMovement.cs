using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowmanMovement : MonoBehaviour
{
    public float maxSpeed = 5f; // Maximum speed the character can reach
    public float acceleration = 2f; // How fast the character accelerates
    private float currentSpeed = 0f; // Current speed of the character
    private Vector3 movement; // Movement direction
    public GameObject snowmanModel;

    void Update()
    {
        // Move the character
        MoveCharacter();
    }

    void MoveCharacter()
    {
        // Get input for horizontal (A/D or Left/Right) and vertical (W/S or Up/Down) axes
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Calculate the movement vector based on input
        movement = new Vector3(moveHorizontal, 0f, moveVertical).normalized;

        // If there is input, gradually increase speed
        if (movement.magnitude > 0)
        {
            // Increase speed over time based on acceleration
            currentSpeed += acceleration * Time.deltaTime;
            // Clamp the speed to the maximum speed
            currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);
        }
        else
        {
            // Optionally, decrease speed gradually when there is no input (optional smooth deceleration)
            currentSpeed = 0f;
        }

        // Move the character in the direction of the movement vector
        transform.Translate(movement * currentSpeed * Time.deltaTime, Space.World);
        TurnModel(moveHorizontal, moveVertical);
    }

    /// <summary>
    /// Turns model accordingly to movement direction
    /// </summary>
    /// <param name="moveHorizontal"></param>
    /// <param name="moveVertical"></param>
    void TurnModel(float moveHorizontal, float moveVertical)
    {
        if (moveVertical > 0 && moveHorizontal > 0)
        {
            snowmanModel.transform.rotation = Quaternion.Euler(new Vector3(0, 135, 0));
        }
        else if (moveVertical > 0 && moveHorizontal < 0)
        {
            snowmanModel.transform.rotation = Quaternion.Euler(new Vector3(0, 45, 0));
        }
        else if (moveVertical < 0 && moveHorizontal > 0)
        {
            snowmanModel.transform.rotation = Quaternion.Euler(new Vector3(0, 225, 0));
        }
        else if (moveVertical < 0 && moveHorizontal < 0)
        {
            snowmanModel.transform.rotation = Quaternion.Euler(new Vector3(0, 315, 0));
        }
        else if (moveVertical > 0)
        {
            snowmanModel.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
        }
        else if (moveVertical < 0)
        {
            snowmanModel.transform.rotation = Quaternion.Euler(new Vector3(0, 270, 0));
        }
        else if (moveHorizontal > 0)
        {
            snowmanModel.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
        }
        else if (moveHorizontal < 0)
        {
            snowmanModel.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
    }
}