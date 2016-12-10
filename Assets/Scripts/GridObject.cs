using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject : MonoBehaviour {

    static GridObject[][] gameGrid = null;

    public int gridX;
    public int gridY;

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
        // TODO: Support with shapes that are not 1x1
        int closestX = Mathf.RoundToInt(transform.position.x - 0.5f);
        int closestY = Mathf.RoundToInt(transform.position.z - 0.5f);

        int horizontalCorner = -GridWidth() / 2;
        int verticalCorner = -GridHeight() / 2;

        gridX = closestX - horizontalCorner;
        gridY = closestY - horizontalCorner;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    static int GridWidth()
    {
        return gameGrid.Length;
    }

    static int GridHeight()
    {
        return gameGrid.Length > 0 ? gameGrid[0].Length : 0;
    }
}
