using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterFloat : MonoBehaviour
{
    //public properties
    public float airDrag = 1;
    public float waterDrag = 10;
    public Transform[] floatPoints;
    public bool attachToSurface;

    //used components
    protected Rigidbody Rigidbody;
    protected WaterGen waves;

    //water line
    protected float waterLine;
    protected Vector3[] waterLinePoints;

    protected Vector3 centerOffset;
    protected Vector3 smoothVectorRotation;
    protected Vector3 targetUp;

    public Vector3 Center { get { return transform.position + centerOffset; } }


    void Awake()
    {
        waves = FindObjectOfType<WaterGen>();
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.useGravity = false;

        //get center
        waterLinePoints = new Vector3[floatPoints.Length];
        for(int i = 0; i < floatPoints.Length; i++)
        {
            waterLinePoints[i] = floatPoints[i].position;
            centerOffset = PhysicsHelper.GetCenter(waterLinePoints) - transform.position; //get center of this custom script just gets all the point and calculates the average
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if(floatPoints == null)
        {
            return;
        }

        for(int i = 0; i < floatPoints.Length; i++)
        {
            if (floatPoints[i] == null)
            {
                continue;
            }

            if(waves != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(waterLinePoints[i], Vector3.one * 0.3f);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(floatPoints[i].position, 0.1f);
        }

        if(Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(new Vector3(Center.x, waterLine, Center.z), Vector3.one * 1f);
        }
    }

    void Update()
    {
        var newWaterLine = 0f;
        var pointUnderWater = false;

        //set waterLinePoints and waterLine
        for(int i = 0; i < floatPoints.Length; i++)
        {
            waterLinePoints[i] = floatPoints[i].position;
            waterLinePoints[i].y = waves.GetHeight(floatPoints[i].position);
            newWaterLine += waterLinePoints[i].y / floatPoints.Length;
            if (waterLinePoints[i].y > floatPoints[i].position.y)
            {
                pointUnderWater = true;
            }
        }

        var waterLineDelta = newWaterLine - waterLine;
        waterLine = newWaterLine;

        //gravity
        var gravity = Physics.gravity;
        Rigidbody.drag = airDrag;
        if(waterLine > Center.y)
        {
            Rigidbody.drag = waterDrag;
            if(attachToSurface)
            {
                //attach to surface
                //Rigidbody.position = new Vector3(Rigidbody.position.x, waterLine - centerOffset.y, Rigidbody.position.y);
            }
            else
            {
                gravity = -Physics.gravity;
                transform.Translate(Vector3.up * waterLineDelta * 0.9f);
            }
        }
        Rigidbody.AddForce(gravity * Mathf.Clamp(Mathf.Abs(waterLine - Center.y), 0, 1));

        //up vector using PhysicsHelper
        //https://www.ilikebigbits.com/2015_03_04_plane_from_points.html basis of GetNormal func(I barely understood any of this)
        targetUp = PhysicsHelper.GetNormal(waterLinePoints);

        //rotation when point is underwater
        if(pointUnderWater)
        {
            //attach to water surface
            targetUp = Vector3.SmoothDamp(transform.up, targetUp, ref smoothVectorRotation, 0.2f);
            Rigidbody.rotation = Quaternion.FromToRotation(transform.up, targetUp) * Rigidbody.rotation;
        }
    }
}
