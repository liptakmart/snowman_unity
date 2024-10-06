using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowmanMovement : MonoBehaviour
{
    public float movementSpeed = 2f; // Maximum speed the character can reach
    public GameObject snowmanModel;
    public GameObject playerCamera;
    private SnowmanState snowmanState;
    private Vector3 movement; // Movement direction

    private void Start()
    {
        snowmanState = GetComponent<SnowmanState>();
        if (State._state.PlayerStats.Count > 1)
        {
            //if there are more players we use envirometn camera
            playerCamera.SetActive(false);
        }
    }
    void Update()
    {
        // Move the character
        MoveCharacter();
    }

    void MoveCharacter()
    {
        if (snowmanState.IsAlive)
        {
            float moveHorizontal = float.MinValue;
            float moveVertical = float.MinValue;

            int playerNumber = snowmanState.GetPlayerNumber();
            if (playerNumber == 0)
            {
                moveHorizontal = Input.GetAxis("Horizontal1");
                moveVertical = Input.GetAxis("Vertical1");
            }
            else if (playerNumber == 1)
            {
                moveHorizontal = Input.GetAxis("Horizontal2");
                moveVertical = Input.GetAxis("Vertical2");
            }

            // Calculate the movement vector based on input
            movement = new Vector3(moveHorizontal, 0f, moveVertical).normalized;

            // If there is input, gradually increase speed
            if (movement.magnitude > 0)
            {
                // Move the character in the direction of the movement vector
                transform.Translate(movement * movementSpeed * Time.deltaTime, Space.World);

                TurnModel(new Vector2(moveHorizontal, moveVertical));
            }
        }
    }

    /// <summary>
    /// Turns model accordingly to movement direction.
    /// X horizontal, Y vertical
    /// </summary>
    /// <param name="vec"></param>
    void TurnModel(Vector2 vec)
    {
        if (movement.magnitude == 0)
        {
            return;
        }

        if (vec.y > 0 && vec.x > 0)
        {
            snowmanModel.transform.rotation = Quaternion.Euler(new Vector3(0, 135, 0));
        }
        else if (vec.y > 0 && vec.x < 0)
        {
            snowmanModel.transform.rotation = Quaternion.Euler(new Vector3(0, 45, 0));
        }
        else if (vec.y < 0 && vec.x > 0)
        {
            snowmanModel.transform.rotation = Quaternion.Euler(new Vector3(0, 225, 0));
        }
        else if (vec.y < 0 && vec.x < 0)
        {
            snowmanModel.transform.rotation = Quaternion.Euler(new Vector3(0, 315, 0));
        }
        else if (vec.y > 0)
        {
            snowmanModel.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
        }
        else if (vec.y < 0)
        {
            snowmanModel.transform.rotation = Quaternion.Euler(new Vector3(0, 270, 0));
        }
        else if (vec.x > 0)
        {
            snowmanModel.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
        }
        else if (vec.x < 0)
        {
            snowmanModel.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
    }
}