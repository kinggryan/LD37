﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {

    private CharacterController characterController;

    public float maxSpeed;
    public float timeToReachMaxSpeed;

    private Vector2 previousInputVector;

	// Use this for initialization
	void Start () {
        characterController = GetComponent<CharacterController>();
        previousInputVector = Vector2.zero;
	}
	
	// Update is called once per frame
	void Update () {
        MovePlayer();
	}

    void MovePlayer()
    {
        // Get direction of movement
        Vector2 desiredInputVector = GetInputVector();
        float maxInputVectorMovement = Time.deltaTime / timeToReachMaxSpeed;
        Vector2 actualInputVector = Vector2.MoveTowards(previousInputVector, desiredInputVector, maxInputVectorMovement);
        previousInputVector = actualInputVector;

        // Move player
        Vector3 movementVector = maxSpeed * Time.deltaTime * new Vector3(actualInputVector.x, 0, actualInputVector.y);
        characterController.Move(movementVector);
    }

    Vector2 GetInputVector()
    {
        // TODO: Normalize this vector properly
        Vector2 inputVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        return inputVector / Mathf.Max(1f,inputVector.magnitude);
    }
}