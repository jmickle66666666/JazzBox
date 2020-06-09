using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class IKTwoLimb : MonoBehaviour
{
    Transform Upper;//root of upper arm
    Transform Lower;//root of lower arm
    Transform End;//root of hand
    public Transform Target;//target position of hand
    public Transform Pole;//direction to bend towards 
    public float UpperElbowRotation;//Rotation offsetts
    public float LowerElbowRotation;

    private float a;//values for use in cos rule
    private float b;
    private float c;
    private Vector3 en;//Normal of plane we want our arm to be on

    void Start()
    {
        End = transform;
        Lower = transform.parent;
        Upper = Lower.parent;
    }
    
    void Update()
    { 
        a = Lower.localPosition.magnitude;
        b = End.localPosition.magnitude;
        c = Vector3.Distance(Upper.position, Target.position);
        en = Vector3.Cross(Target.position-Upper.position, Pole.position-Upper.position);

        //Set the rotation of the upper arm
        Upper.rotation = Quaternion.LookRotation(Target.position-Upper.position, Quaternion.AngleAxis(UpperElbowRotation, Lower.position - Upper.position) * (en));
        Upper.rotation *= Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, Lower.localPosition));
        Upper.rotation=  Quaternion.AngleAxis(-CosAngle(a, c, b), -en)*Upper.rotation;

        //set the rotation of the lower arm
        Lower.rotation =  Quaternion.LookRotation(Target.position - Lower.position, Quaternion.AngleAxis(LowerElbowRotation, End.position - Lower.position) * (en));
        Lower.rotation *= Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, End.localPosition));

        //Lower.LookAt(Lower, Pole.position - Upper.position);
        //Lower.rotation = Quaternion.AngleAxis(CosAngle(a, b, c), en);
        End.rotation = Target.rotation;
    }

    //function that finds angles using the cosine rule 
    float CosAngle(float a, float b, float c) {
        if ( !float.IsNaN(Mathf.Acos((-(c * c) + (a * a) + (b * b)) / (-2 * a * b)) * Mathf.Rad2Deg))
        {
            return Mathf.Acos((-(c * c) + (a * a) + (b * b)) / (2 * a * b)) * Mathf.Rad2Deg;
        }
        else
        {
            return 1;
        }
    }
}