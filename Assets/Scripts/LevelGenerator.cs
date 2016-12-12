using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour {

    private class LevelGeneratorGridObject
    {
        // "Origin" of object is always towards the negatives of both. E.g. a height of 2 on an object at (1,1) means the object is (1,1) and (1,2)
        public int x;
        public int y;
        public int width;
        public int height;
    }

    // TODO: Support odd width and height. Some logic relies on integer division in the grid object code, I think
    // May be easily fixable
    public int levelWidth;
    public int levelHeight;
    public int minNumFurniture;
    public int maxNumFurniture;
    public float minProportion2x1s;
    public float maxProportion2x1s;

    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject[] furniturePrefabs1x1;
    public GameObject[] furniturePrefabs2x1;

    private bool[][] generationGrid;
    private List<GameObject> unspawnedFurniturePrefabs1x1;
    private List<GameObject> unspawnedFurniturePrefabs2x1;

    // Use this for initialization
    void Start() {
        GenerateLevel();
    }

    // Update is called once per frame
    void Update() {

    }

    void GenerateLevel()
    {
        CreateGenerationGrid();
        List<LevelGeneratorGridObject> generationGridObjects = PopulateGenerationGrid();
        InstantiateFurnitureForGridObjects(generationGridObjects);
        GenerateRoom();
    }

    void CreateGenerationGrid()
    {
        generationGrid = new bool[levelWidth][];
        for (int i = 0; i < levelHeight; i++)
        {
            generationGrid[i] = new bool[levelHeight];
        }
    }

    void InstantiateFurnitureForGridObjects(List<LevelGeneratorGridObject> gridObjects)
    {
        unspawnedFurniturePrefabs2x1 = new List<GameObject>(furniturePrefabs2x1);
        unspawnedFurniturePrefabs1x1 = new List<GameObject>(furniturePrefabs1x1);

        foreach (LevelGeneratorGridObject gridObject in gridObjects)
        {
            InstantiateFurnitureForGridObject(gridObject);
        }
    }

    void InstantiateFurnitureForGridObject(LevelGeneratorGridObject gridObject)
    {
        GameObject prefabToMake;
        float rotationAngle = 0;
        if (gridObject.width > 1 || gridObject.height > 1)
        {
            int randomIndex = Random.Range(0, unspawnedFurniturePrefabs2x1.Count);
            prefabToMake = unspawnedFurniturePrefabs2x1[randomIndex];
            rotationAngle = gridObject.height > 1 ? -90 : 0; //(Random.Range(0, 2) + (gridObject.height > 1 ? 1 : 0)) * 90;
            unspawnedFurniturePrefabs2x1.RemoveAt(randomIndex);
        }
        else
        {
            int randomIndex = Random.Range(0, unspawnedFurniturePrefabs1x1.Count);
            prefabToMake = unspawnedFurniturePrefabs1x1[randomIndex];
            rotationAngle = Random.value < 0.3333 ? 0 : Random.value < 0.5 ? 90 : 270;
            unspawnedFurniturePrefabs1x1.RemoveAt(randomIndex);
        }

        Vector3 positionToSpawn = new Vector3(gridObject.x - generationGrid.Length / 2 + 0.5f, 0.5f, gridObject.y - generationGrid[0].Length / 2 + 0.5f);

        GameObject.Instantiate(prefabToMake, positionToSpawn, Quaternion.AngleAxis(rotationAngle, Vector3.up));
    }

    List<LevelGeneratorGridObject> PopulateGenerationGrid()
    {
        int numObjectsToSpawn = Random.Range(minNumFurniture, maxNumFurniture);
        int num2x1s = Mathf.FloorToInt(numObjectsToSpawn * Random.Range(minProportion2x1s, maxProportion2x1s));
        int num1x1s = numObjectsToSpawn - num2x1s;
        List<LevelGeneratorGridObject> objects =  CreateGridObjects(num1x1s, num2x1s);
        AddObjectsToGrid(objects);
        return objects;
    }

    void AddObjectsToGrid(List<LevelGeneratorGridObject> gridObjects)
    {
        foreach(LevelGeneratorGridObject gridObject in gridObjects)
        {
            PlaceObjectIntoGrid(gridObject);
        }
    }

    List<LevelGeneratorGridObject> CreateGridObjects(int num1x1s, int num2x1s)
    {
        List<LevelGeneratorGridObject> objs = new List<LevelGeneratorGridObject>();
        for(int i = 0; i < num2x1s; i++)
        {
            LevelGeneratorGridObject gridObj = new LevelGeneratorGridObject();
            gridObj.width = 2;
            gridObj.height = 1;
            objs.Add(gridObj);
        }
        for (int i = 0; i < num1x1s; i++)
        {
            LevelGeneratorGridObject gridObj = new LevelGeneratorGridObject();
            gridObj.width = 1;
            gridObj.height = 1;
            objs.Add(gridObj);
        }
        return objs;
    }

    void PlaceObjectIntoGrid(LevelGeneratorGridObject gridObj)
    {
        while(true)
        {
            RandomizeGridObjectRotation(gridObj);
            int randomX = Random.Range(0, levelWidth - gridObj.width + 1);
            int randomY = Random.Range(0, levelHeight - gridObj.height + 1);
            gridObj.x = randomX;
            gridObj.y = randomY;
            if(ObjectCanFitIntoGrid(gridObj))
            {
                MarkObjectSpacesFullOnGrid(gridObj);
                return;
            }
        }
    }

    bool ObjectCanFitIntoGrid(LevelGeneratorGridObject gridObj)
    {
        for(int x = 0; x < gridObj.width; x++)
        {
            for(int y = 0; y < gridObj.height; y++)
            {
                if(generationGrid[gridObj.x+x][gridObj.y+y])
                {
                    return false;
                }
            }
        }

        return true;
    }

    void MarkObjectSpacesFullOnGrid(LevelGeneratorGridObject gridObj)
    {
        for (int x = 0; x < gridObj.width; x++)
        {
            for (int y = 0; y < gridObj.height; y++)
            {
                generationGrid[gridObj.x + x][gridObj.y + y] = true;
            }
        }
    }

    void RandomizeGridObjectRotation(LevelGeneratorGridObject gridObject)
    {
        if(Random.value < 0.5f)
        {
            int newWidth = gridObject.height;
            int newHeight = gridObject.width;

            gridObject.width = newWidth;
            gridObject.height = newHeight;
        }
    }

    void GenerateRoom()
    {
        GenerateFloor();
        GenerateWalls();
    }

    void GenerateFloor()
    {
        for(int x = 0; x < generationGrid.Length; x++)
        {
            for(int y = 0; y < generationGrid[0].Length; y++)
            {
                Vector3 positionToSpawn = new Vector3(x - generationGrid.Length / 2 + 0.5f, 0, y - generationGrid[0].Length / 2 + 0.5f);
                GameObject.Instantiate(floorPrefab, positionToSpawn, Quaternion.identity);
            }
        }
    }

    void GenerateWalls()
    {
        GenerateLeftWall();
        GenerateTopWall();
        GenerateRightWall();
        GenerateBottomWall();
    }

    void GenerateLeftWall()
    {
        float xPos = -generationGrid.Length / 2;
        for(int y = 0; y < generationGrid[0].Length/2; y++)
        {
            Vector3 positionToSpawn = new Vector3(xPos, 1, 2*y - generationGrid[0].Length / 2 + 1);
            GameObject.Instantiate(wallPrefab, positionToSpawn, Quaternion.LookRotation(Vector3.right));
        }
    }

    void GenerateRightWall()
    {
        float xPos = generationGrid.Length / 2;
        for (int y = 0; y < generationGrid[0].Length / 2; y++)
        {
            Vector3 positionToSpawn = new Vector3(xPos, 1, 2 * y - generationGrid[0].Length / 2 + 1);
            GameObject.Instantiate(wallPrefab, positionToSpawn, Quaternion.LookRotation(Vector3.left));
        }
    }

    void GenerateTopWall()
    {
        float yPos = generationGrid.Length / 2;
        for (int x = 0; x < generationGrid[0].Length / 2; x++)
        {
            Vector3 positionToSpawn = new Vector3(2 * x - generationGrid[0].Length / 2 + 1, 1, yPos);
            GameObject.Instantiate(wallPrefab, positionToSpawn, Quaternion.LookRotation(Vector3.back));
        }
    }

    void GenerateBottomWall()
    {
        float yPos = -generationGrid.Length / 2;
        for (int x = 0; x < generationGrid[0].Length / 2; x++)
        {
            Vector3 positionToSpawn = new Vector3(2 * x - generationGrid[0].Length / 2 + 1, 1, yPos);
            GameObject obj = GameObject.Instantiate(wallPrefab, positionToSpawn, Quaternion.LookRotation(Vector3.forward));
            foreach(Renderer r in obj.GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
            }
            obj.GetComponentInChildren<WallGenerator>().enabled = false;
        }
    }
}
