using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowmanMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Movement speed of the character

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

        // Move the character in the direction of the movement vector
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// Turns model accordingly to movement direction
    /// </summary>
    void TurnModel()
    {
        //TODO
    }
}
