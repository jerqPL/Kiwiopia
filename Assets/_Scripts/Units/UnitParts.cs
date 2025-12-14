using System.Collections.Generic;
using UnityEngine;

public class UnitParts : MonoBehaviour
{
    public List<GameObject> armor = new List<GameObject>();
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject boneWhileNotFighting;
    public GameObject weapon;

    public Vector3 positionOffsetNotFighting;
    public Vector3 rotationOffsetNotFighting;

    public Vector3 positionOffsetFighting;
    public Vector3 rotationOffsetFighting;

    public float scaleFactor;
}
