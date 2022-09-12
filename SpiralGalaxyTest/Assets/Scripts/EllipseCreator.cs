using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EllipseCreator : MonoBehaviour
{
   /* private DimensionsEllipse orbit;


    [Header("Ellipse Properties")]

    [SerializeField] Vector3 centre;
    [SerializeField] float semiMajorAxis;
    [Range(0, 1)]
    [SerializeField] float eccentricity;
    [Range(3, 36)]
    [SerializeField] int resolution;
    [Range(0, 360)]
    [SerializeField] float inclination;
    [Range(0, 360)]
    [SerializeField] float longitudeOfAscendingNode;


    [Header("Line Properties")]
    [SerializeField] Material material;
    [SerializeField] float lineThickness;
    LineRenderer lr;

    Vector3[] pointsOnEllipse;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.material = material;
        lr.startWidth = lineThickness;
        lr.endWidth = lineThickness;
    }

    public void DrawEllipse() 
    {
        lr = GetComponent<LineRenderer>();
        lr.material = material;
        lr.startWidth = lineThickness;
        lr.endWidth = lineThickness;
        orbit = new DimensionsEllipse(centre, semiMajorAxis, eccentricity, inclination, longitudeOfAscendingNode);
        pointsOnEllipse = new Vector3[resolution + 1];

        Quaternion inclinationRotation = Quaternion.Euler(inclination * Mathf.Deg2Rad, 0f, 0f);
        float angleStep = 360 / resolution;
        for (int i = 0; i <= resolution; i++)
        {
            float curAngleDeg = i * angleStep;
            float curAngle = curAngleDeg * Mathf.Deg2Rad; //+ orbit.AngleOffsetRadians;
            Vector3 point = new Vector3(orbit.semiMajorAxis * Mathf.Sin(curAngle), orbit.SemiMinorAxis * Mathf.Cos(curAngle));
            pointsOnEllipse[i] = orbit.getRotatedPoint(point, Quaternion.Euler(orbit.Inclination, orbit.LongitudeOfAscendingNode, 0f));
        }

        //Before calling SetPositions we must access positionCount
        lr.positionCount = resolution + 1;
        lr.SetPositions(pointsOnEllipse);
    }
    void Start()
    {
        DrawEllipse();
       
        
    }*/

}
