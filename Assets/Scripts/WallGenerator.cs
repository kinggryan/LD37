using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGenerator : MonoBehaviour {

    public GameObject[] wallDecorations;
    public float wallDecorationPercent = 0.3f;

	// Use this for initialization
	void Start () {
		if(Random.value < wallDecorationPercent)
        {
            int randomIndex = Random.Range(0, wallDecorations.Length);
            GameObject decoration = GameObject.Instantiate(wallDecorations[randomIndex]);
            decoration.transform.position = transform.position;
            decoration.transform.rotation = transform.rotation;
            decoration.transform.parent = transform;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
