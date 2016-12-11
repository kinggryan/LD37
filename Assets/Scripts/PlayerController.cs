using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {

    private CharacterController characterController;

    public int playerNum;
    public float maxSpeed;
    public float timeToReachMaxSpeed;
    public Animator animator;

    private Vector2 previousInputVector;
    private GridObject objectToShove;
    private Vector3 objectToShoveDirection;


    // Use this for initialization
    void Start () {
        characterController = GetComponent<CharacterController>();
        previousInputVector = Vector2.zero;
	}
	
	// Update is called once per frame
	void Update () {
        MovePlayer();
        ShoveObjects();
        if(!Mathf.Approximately(0.5f,transform.position.y))
        {
            characterController.SimpleMove(new Vector3(0, 0.5f - transform.position.y, 0));
        }
        UpdateAnimator();
	}

    void UpdateAnimator()
    {
        if(previousInputVector.magnitude > 0.01f)
        {
            animator.SetFloat("Speed", previousInputVector.magnitude);
            transform.rotation = Quaternion.LookRotation(new Vector3(previousInputVector.x, 0, previousInputVector.y), Vector3.up);
        }
        else
        {
            animator.SetFloat("Speed", -1);
        }
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
        Vector2 inputVector = new Vector2(Input.GetAxis("Horizontal" + playerNum), Input.GetAxis("Vertical" + playerNum));
        return inputVector / Mathf.Max(1f,inputVector.magnitude);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        GridObject gridObject = hit.collider.gameObject.GetComponent<GridObject>();
        if(!gridObject)
        {
            gridObject = hit.collider.gameObject.GetComponentInParent<GridObject>();
        }

        if (gridObject)
        {
            if(gridObject.CanBeShoved())
            {
                objectToShove = gridObject;
                objectToShoveDirection = -hit.normal;
            }
            else if(gridObject.ShouldShovePlayer())
            {
                characterController.Move(gridObject.ShovePlayerVector()*Time.deltaTime);
                transform.position += new Vector3(0, 0.5f - transform.position.y, 0);
            }
        }
    }

    void ShoveObjects()
    {
        if(Input.GetButtonDown("Shove" + playerNum) && objectToShove)
        {
            // TODO: Fix this for non 1x1 objects
            objectToShove.ShoveFurniture(objectToShoveDirection);
        }

        objectToShove = null;
    }
}
