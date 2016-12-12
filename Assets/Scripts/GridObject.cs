using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GridObject : MonoBehaviour {

    private enum FurnitureState { Stopped, Sliding, Stopping }

    static GridObject[][] gameGrid = null;

    public float slideSpeed = 5f;
    public FurnitureIcon icon;
    public GameObject linkPrefab;
    public Sprite iconTex;

    private int gridX;
    private int gridY;
    private Rigidbody rigidbody;

    // Sliding properties
    private FurnitureState furnitureState;
    private Vector3 slideVector;
    private int endSlideGridX;
    private int endSlideGridY;
    private HashSet<GridObject> interlockedObjects = new HashSet<GridObject>();

    // Use this for initialization
    void Start() {
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
    void Update() {
        // If we're sliding, track the grid position
        if (furnitureState == FurnitureState.Sliding)
        {
            transform.position += Time.deltaTime * slideVector;
            endSlideGridX = GetGridXPositionFromWorldPosition(transform.position);
            endSlideGridY = GetGridYPositionFromWorldPosition(transform.position);
        }
        else if (furnitureState == FurnitureState.Stopping)
        {
            SlideToStop();
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        // If we collided with another grid object
        GridObject collidedGridObj = collider.gameObject.GetComponent<GridObject>();
        Wall wall = collider.gameObject.GetComponent<Wall>();

        if (((collidedGridObj && collidedGridObj != this) || wall) && furnitureState == FurnitureState.Sliding)
        {
            StartSlidingToStop();
            foreach(GridObject obj in interlockedObjects)
            {
                obj.StartSlidingToStop();
            }
        }
    }

    public void StartSlidingToStop()
    {
        if(furnitureState == FurnitureState.Sliding)
            StartSlidingToStop(endSlideGridX, endSlideGridY);
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

        foreach (RaycastHit hitInfo in allHits)
        {
            GridObject gridObject = hitInfo.collider.GetComponent<GridObject>();
            if (gridObject && gridObject != this)
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

    public bool ShouldShovePlayer()
    {
        return furnitureState == FurnitureState.Sliding;
    }

    public Vector3 ShovePlayerVector()
    {
        return slideVector;
    }

    public void ShoveFurniture(Vector3 shoveDirectionWorldSpace)
    {
        if(furnitureState == FurnitureState.Stopped && CanBeShovedInDirection(shoveDirectionWorldSpace) && InterlockedObjectsCanBeShovedInDirection(shoveDirectionWorldSpace))
        {
            SlideInDirection(shoveDirectionWorldSpace);
            foreach(GridObject obj in interlockedObjects)
            {
                obj.SlideInDirection(shoveDirectionWorldSpace);
            }
        }
    }

    public bool InterlockedObjectsCanBeShovedInDirection(Vector3 shoveDirectionWorldSpace)
    {
        foreach(GridObject obj in interlockedObjects)
        {
            if(!obj.CanBeShovedInDirection(shoveDirectionWorldSpace))
            {
                return false;
            }
        }

        return true;
    }

    public bool CanBeShovedInDirection(Vector3 shoveDirectionWorldSpace)
    {
        // Verify we wouldn't hit a grid object immediately.
        RaycastHit[] hitsInShoveDirection = rigidbody.SweepTestAll(shoveDirectionWorldSpace, 0.3f, QueryTriggerInteraction.Collide);
        bool hitGridObj = false;
        foreach (RaycastHit hitInfo in hitsInShoveDirection)
        {
            GridObject gridObj = hitInfo.collider.GetComponent<GridObject>();
            if ((gridObj && gridObj != this && !interlockedObjects.Contains(gridObj)) || hitInfo.collider.GetComponent<Wall>())
            {
                hitGridObj = true;
            }
        }

        return !hitGridObj;
    }

    public void SlideInDirection(Vector3 shoveDirectionWorldSpace)
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
    
    public void LockIntoPlaceWithObjects(GridObject[] otherObjects)
    {
        if(interlockedObjects.Count == 0)
        {
            // First time interlocking
            icon.LockIntoPlace();
        }

        
        foreach(GridObject obj in otherObjects)
        {
            foreach(Collider col in obj.GetComponents<Collider>())
            {
                foreach(Collider selfcol in GetComponents<Collider>())
                {
                    Physics.IgnoreCollision(selfcol, col);
                }
            }
               
            if(interlockedObjects.Add(obj))
            {
                // Arbitrarily make it so that only one of the objects adds a link
                if(transform.position.x > obj.transform.position.x || (Mathf.Approximately(obj.transform.position.x,transform.position.x) && transform.position.y > obj.transform.position.y))
                {
                    Vector3 meetingPoint = GetMeetingPointWithGridObject(obj);
                    Vector3 spawnPoint = meetingPoint;
                    spawnPoint.y = 0.1f;
                    GameObject.Instantiate(linkPrefab, spawnPoint, Quaternion.LookRotation(meetingPoint - transform.position, Vector3.up));
                }
            }
        }
    }

    Vector3 GetMeetingPointWithGridObject(GridObject otherGridObject)
    {
        if (PointsAreGridAligned(transform.position, otherGridObject.transform.position))
        {
            return (transform.position + otherGridObject.transform.position) / 2;
        }
        else if (PointsAreGridAligned(transform.position, otherGridObject.transform.position + otherGridObject.transform.right))
        {
            return (transform.position + otherGridObject.transform.position + otherGridObject.transform.right) / 2;
        }
        else if (PointsAreGridAligned(transform.position + transform.right, otherGridObject.transform.position))
        {
            return (transform.position + transform.right + otherGridObject.transform.right) / 2;
        }
        else
        {
            Debug.LogError("ERROR: Objects " + this + " and " + otherGridObject + " did not align.");
            return Vector3.zero;
        }
    }

    bool PointsAreGridAligned(Vector3 point1, Vector3 point2)
    {
        return VectorIsGridAligned(point1 - point2);
    }

    bool VectorIsGridAligned(Vector3 vector)
    {
        return Mathf.Approximately(vector.x, 0) || Mathf.Approximately(vector.z, 0);
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
