using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject : MonoBehaviour {

    private enum FurnitureState { Stopped, Sliding, Stopping }

    static GridObject[][] gameGrid = null;

    public float slideSpeed = 5f;

    private int gridX;
    private int gridY;

    // Sliding properties
    private FurnitureState furnitureState;
    private Vector3 slideVector;
    private int endSlideGridX;
    private int endSlideGridY;

	// Use this for initialization
	void Start () {
        if (gameGrid == null)
            InitializeGameGrid();

        SetupOnGrid();
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
    }

    void SlideToStop()
    {

    }

    public void ShoveFurniture(Vector3 shoveDirectionWorldSpace)
    {
        if(furnitureState == FurnitureState.Stopped)
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

    public bool CanBeShoved()
    {
        return furnitureState == FurnitureState.Stopped;
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
