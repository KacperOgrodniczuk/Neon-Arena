using System;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerAnimationManager : MonoBehaviour
{
    public Animator animator;

    float dampTime = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void UpdateMovementParameters(float horizontalInput, float verticalInput)
    {
        animator.SetFloat("HorizontalInput", horizontalInput, dampTime, Time.deltaTime);
        animator.SetFloat("VerticalInput", verticalInput, dampTime, Time.deltaTime);
    }
}
