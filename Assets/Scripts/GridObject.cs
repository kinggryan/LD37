using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GridObject : MonoBehaviour {

    private enum FurnitureState { Stopped, Sliding, Stopping }

    static GridObject[][] gameGrid = null;

    public float slideSpeed = 5f;

    private int gridX;
    private int gridY;
    private Rigidbody rigidbody;
    private Animator animator;

    // Sliding properties
    private FurnitureState furnitureState;
    private Vector3 slideVector;
    private int endSlideGridX;
    private int endSlideGridY;
    private List<GridObject> interlockedObjects;

	// Use this for initialization
	void Start () {
        if (gameGrid == null)
            InitializeGameGrid();

        SetupOnGrid();
        rigidbody = GetComponent<Rigidbody>();
	}

    void InitializeGameGrid()
    {
        // TODO: Dynamically set game grid size
        gameGrid = new GridObject[10][];
        for (int i = 0; i < 10; i++)
            gameGrid[i] = new GridObject[10];
    }

    void SetupOnGrid()
    {
        gridX = GetGridXPositionFromWorldPosition(transform.position);
        gridY = GetGridYPositionFromWorldPosition(transform.position);
    }
	
	// Update is called once per frame
	void Update () {
        // If we're sliding, track the grid position
        if(furnitureState == FurnitureState.Sliding)
        {
            transform.position += Time.deltaTime*slideVector;
            endSlideGridX = GetGridXPositionFromWorldPosition(transform.position);
            endSlideGridY = GetGridYPositionFromWorldPosition(transform.position);
        }
        else if(furnitureState == FurnitureState.Stopping)
        {
            SlideToStop();
        }
	}

    void OnTriggerEnter(Collider collider)
    {
        // If we collided with another grid object
        GridObject collidedGridObj = collider.gameObject.GetComponent<GridObject>();
        Wall wall = collider.gameObject.GetComponent<Wall>();

        if(((collidedGridObj && collidedGridObj != this) || wall) && furnitureState == FurnitureState.Sliding)
        {
            // If we are the object that hit the other object, eg the normals are along our movement axis.
            // TODO: Will this work with perfectly diagonal collisions?
            //            float epsilon = 0.1f;
            //            if (Vector3.Angle(collision.contacts[0].normal, -slideVector.normalized) < epsilon)
            //            {
            StartSlidingToStop(endSlideGridX,endSlideGridY);
//            }
        }
    }

    void StartSlidingToStop(int posX, int posY)
    {
        furnitureState = FurnitureState.Stopping;
        gridX = posX;
        gridY = posY;

        // TODO: Animate
        transform.position = GetWorldPositionFromGridPosition(gridX, gridY) + new Vector3(0, transform.position.y, 0);
        furnitureState = FurnitureState.Stopped;

        List<GridObject> objects = GetAdjacentGridObjects();
        PlayerGoalManager.ObjectSlidIntoPlace(this, objects.ToArray());
    }

    void SlideToStop()
    {

    }

    List<GridObject> GetAdjacentGridObjects()
    {
        // Do a self cast 0.3 in each direction to find objects we are touching that are not moving
        RaycastHit[] hitInfosFwd = rigidbody.SweepTestAll(Vector3.forward, 0.3f, QueryTriggerInteraction.Collide);
        RaycastHit[] hitInfosLeft = rigidbody.SweepTestAll(Vector3.left, 0.3f, QueryTriggerInteraction.Collide);
        RaycastHit[] hitInfosBack = rigidbody.SweepTestAll(Vector3.back, 0.3f, QueryTriggerInteraction.Collide);
        RaycastHit[] hitInfosRight = rigidbody.SweepTestAll(Vector3.right, 0.3f, QueryTriggerInteraction.Collide);
        List<RaycastHit> allHits = new List<RaycastHit>();
        allHits.AddRange(hitInfosFwd);
        allHits.AddRange(hitInfosLeft);
        allHits.AddRange(hitInfosBack);
        allHits.AddRange(hitInfosRight);

        List<GridObject> adjObjs = new List<GridObject>();

        foreach(RaycastHit hitInfo in allHits)
        {
            GridObject gridObject = hitInfo.collider.GetComponent<GridObject>();
            if(gridObject && gridObject != this)
            {
                adjObjs.Add(gridObject);
            }
        }

        return adjObjs;
    }

    public bool CanLockIntoPlace()
    {
        return furnitureState == FurnitureState.Stopped;
    }

    public bool CanBeShoved()
    {
        return furnitureState == FurnitureState.Stopped;
    }

    public void ShoveFurniture(Vector3 shoveDirectionWorldSpace)
    {
        // TODO: Verify we wouldn't hit a grid object immediately.
        RaycastHit[] hitsInShoveDirection = rigidbody.SweepTestAll(shoveDirectionWorldSpace, 0.3f, QueryTriggerInteraction.Collide);
        bool hitGridObj = false;
        foreach(RaycastHit hitInfo in hitsInShoveDirection)
        {
            GridObject gridObj = hitInfo.collider.GetComponent<GridObject>();
            if( (gridObj && gridObj != this) || hitInfo.collider.GetComponent<Wall>())
            {
                hitGridObj = true;
            }
        }

        if(furnitureState == FurnitureState.Stopped && !hitGridObj)
        {
            // Determine direction to shove
            if (Mathf.Abs(shoveDirectionWorldSpace.x) > Mathf.Abs(shoveDirectionWorldSpace.z))
            {
                slideVector = new Vector3(slideSpeed * Mathf.Sign(shoveDirectionWorldSpace.x), 0, 0);
            }
            else
            {
                slideVector = new Vector3(0, 0, slideSpeed * Mathf.Sign(shoveDirectionWorldSpace.z));
            }

            furnitureState = FurnitureState.Sliding;
        }
    }

    
    public void LockIntoPlaceWithObjects(GridObject[] otherObjects)
    {

    }

    int GetGridXPositionFromWorldPosition(Vector3 worldPosition)
    {
        // TODO: Support with shapes that are not 1x1
        int closestX = Mathf.RoundToInt(worldPosition.x - 0.5f);
        int horizontalCorner = -GridWidth() / 2;
        int x = closestX - horizontalCorner;

        return x;
    }

    int GetGridYPositionFromWorldPosition(Vector3 worldPosition)
    {
        // TODO: Support with shapes that are not 1x1
        int closestY = Mathf.RoundToInt(worldPosition.z - 0.5f);
        int verticalCorner = -GridHeight() / 2;
        int y = closestY - verticalCorner;

        return y;
    }

    Vector3 GetWorldPositionFromGridPosition(int x, int y)
    {
        float xPos = x + 0.5f - GridWidth() / 2;
        float zPos = y + 0.5f - GridHeight() / 2;
        return new Vector3(xPos, 0, zPos);
    }

    // MARK: Static

    static int GridWidth()
    {
        return gameGrid.Length;
    }

    static int GridHeight()
    {
        return gameGrid.Length > 0 ? gameGrid[0].Length : 0;
    }
}
