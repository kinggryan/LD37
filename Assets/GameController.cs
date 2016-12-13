using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Restart"))
        {
            PlayerGoalManager.gridObjectsOutOfSets = new List<GridObject>();
            PlayerGoalManager.gridObjectsInSets = new List<GridObject>();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
