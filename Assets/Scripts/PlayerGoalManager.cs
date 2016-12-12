using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PlayerGoalManager : MonoBehaviour {

    private class FurnitureSet
    {
        public List<GridObject> gridObjects;
        public bool[] connectedObjects;
        public bool complete;
    }
    
    static List<GridObject> gridObjectsOutOfSets = new List<GridObject>();
    static List<GridObject> gridObjectsInSets = new List<GridObject>();

    public int numFurnitureSets = 2;
    public int sizeOfSets = 3;
    public Canvas winTheGameCanvas;
    public Color playerColor;
    public List<FurnitureIcon> unmatchedFurnitureIcons = new List<FurnitureIcon>();  // TODO: Make this not have to be in order

    private Renderer rRenderer;
    private FurnitureSet[] furnitureSets;

	// Use this for initialization
	void Start () {
        SetupGridObjectsOutOfSets();
        CreateSets();

        rRenderer = GetComponent<Renderer>();
        rRenderer.material.color = playerColor;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void SetupGridObjectsOutOfSets()
    {
        if (gridObjectsOutOfSets.Count == 0)
            gridObjectsOutOfSets = new List<GridObject>((GridObject[])GridObject.FindObjectsOfType<GridObject>());
    }

    void CreateSets()
    {
        furnitureSets = new FurnitureSet[numFurnitureSets];
        for(int i = 0; i < numFurnitureSets; i++)
        {
            furnitureSets[i] = CreateSetOfSize(sizeOfSets);
        }
    }

    FurnitureSet CreateSetOfSize(int numObjects)
    {
        FurnitureSet newSet = new FurnitureSet();
        newSet.gridObjects = new List<GridObject>();
        newSet.connectedObjects = new bool[numObjects];
        newSet.complete = false;

        for (int i = 0; i < numObjects; i++)
        {
            int objToAddIndex = Random.Range(0, gridObjectsOutOfSets.Count - 1);
            GridObject objToAdd = gridObjectsOutOfSets[objToAddIndex];
            gridObjectsOutOfSets.RemoveAt(objToAddIndex);

            newSet.gridObjects.Add(objToAdd);
            gridObjectsInSets.Add(objToAdd);

            objToAdd.icon = unmatchedFurnitureIcons[0];
            unmatchedFurnitureIcons[0].iconImage.sprite = objToAdd.iconTex;
            unmatchedFurnitureIcons.RemoveAt(0);

            Renderer objRenderer = objToAdd.GetComponentInChildren<Renderer>();

            if(objRenderer)
            {
            //    objRenderer.material.color = playerColor;
            }
        }

        return newSet;
    }

    bool IsObjectPartOfSet(GridObject obj, FurnitureSet set)
    {
        foreach (GridObject gridObj in set.gridObjects)
        {
            if (obj == gridObj)
                return true;
        }

        return false;
    }

    public void AddObjectsToSetsIfNeeded(GridObject obj1, GridObject obj2)
    {
        foreach(FurnitureSet set in furnitureSets)
        {
            int index1 = set.gridObjects.IndexOf(obj1);
            int index2 = set.gridObjects.IndexOf(obj2);

            if(index1 >= 0 && index2 >= 0)
            {
                set.connectedObjects[index1] = true;
                set.connectedObjects[index2] = true;

                List<GridObject> objectsToConnect1 = new List<GridObject>(set.gridObjects);
                for(int i = 0; i < set.connectedObjects.Length; i++)
                {
                    if (!set.connectedObjects[i])
                        objectsToConnect1.RemoveAt(i);
                }

                objectsToConnect1.Remove(obj1);
                obj1.LockIntoPlaceWithObjects(objectsToConnect1.ToArray());
                foreach(GridObject otherObj in objectsToConnect1)
                {
                    otherObj.LockIntoPlaceWithObjects(new GridObject[] { obj1 });
                }

                List<GridObject> objectsToConnect2 = new List<GridObject>(set.gridObjects);
                for (int i = 0; i < set.connectedObjects.Length; i++)
                {
                    if (!set.connectedObjects[i])
                        objectsToConnect2.RemoveAt(i);
                }

                objectsToConnect2.Remove(obj2);
                obj2.LockIntoPlaceWithObjects(objectsToConnect2.ToArray());
                foreach (GridObject otherObj in objectsToConnect2)
                {
                    otherObj.LockIntoPlaceWithObjects(new GridObject[] { obj2 });
                }

                // CHeck if set is complete
                bool complete = true;
                foreach(bool locked in set.connectedObjects)
                {
                    if (!locked)
                    {
                        complete = false;
                        break;
                    }
                }

                if (complete)
                    set.complete = true;

                if (AllSetsComplete())
                    WinTheGame();

                break;
            }
        }
    }

    void WinTheGame()
    {
        winTheGameCanvas.enabled = true;
    }

    bool AllSetsComplete()
    {
        bool allSetsComplete = true;
        foreach(FurnitureSet set in furnitureSets)
        {
            if (!set.complete)
            {
                allSetsComplete = false;
                break;
            }
        }

        return allSetsComplete;
    }

    public static void ObjectSlidIntoPlace(GridObject stoppedObj,GridObject[] touchingObjects)
    {
        // For each touching object, see if the stopped object and touching object belong to a victory set
        foreach(GridObject touchingObject in touchingObjects)
        {
            foreach(PlayerGoalManager goalManager in Object.FindObjectsOfType<PlayerGoalManager>())
            {
                goalManager.AddObjectsToSetsIfNeeded(stoppedObj, touchingObject);
            }
        }
    }

    
    
}
