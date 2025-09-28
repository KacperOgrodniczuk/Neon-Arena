using System;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerAnimationManager : MonoBehaviour
{
    public Animator animator { get; private set; }

    float animationDampTime = 0.15f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void UpdateMovementParameters(float horizontalInput, float verticalInput)
    {
        animator.SetFloat("HorizontalInput", horizontalInput, animationDampTime, Time.deltaTime);
        animator.SetFloat("VerticalInput", verticalInput, animationDampTime, Time.deltaTime);
    }

    // Play Target Animation, using the name of the animation state as a parameter (Not the animation clip name)
    public void PlayTargetAnimation(string animationStateName)
    {
        animator.CrossFade(animationStateName, animationDampTime);
    }
}
