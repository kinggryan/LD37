using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FurnitureIcon : MonoBehaviour {

    public UnityEngine.UI.Image iconImage;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void LockIntoPlace()
    {
        animator.SetBool("Matched", true);
    }
}
