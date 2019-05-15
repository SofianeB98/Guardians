using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Jump Value", menuName = "CharacterValue/New Jump Value", order = 1)]
public class JumpSectionValue : ScriptableObject
{
    public float speedPerteSpeed = 1f;
    public float jumpHeight = 8;
    public float jumpTimeToReachMax = 0.5f;
    public AnimationCurve jumpBehaviour;
    public float doubleJumpHeight = 8;
    public float doubleJumpTimeToReachMax = 0.5f;
    public AnimationCurve doubleJumpBehaviour;
}
