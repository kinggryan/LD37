using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGenerator : MonoBehaviour {

    // TODO: Make better

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

            foreach(Renderer r in decoration.GetComponentsInChildren<Renderer>())
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
