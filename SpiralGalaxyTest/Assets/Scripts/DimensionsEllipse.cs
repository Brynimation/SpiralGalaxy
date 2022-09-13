using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*DimensionsEllipse defines an ellipse/orbital path*/

[System.Serializable]
public class DimensionsEllipse
{
    public Vector3 centre;
    public float semiMajorAxis;
    [Range(0, 1)]
    public float eccentricity;
    private float semiMinorAxis;
    public float inclination;
    public float longitudeOfAscendingNode;
    public float angularOffset;
    public float orbitalPeriod;
    public Quaternion rotation;

    public float SemiMinorAxis
    {
        get { return semiMinorAxis; }
    }
    public float Inclination
    {
        get { return inclination; }
    }
    public float LongitudeOfAscendingNode
    {
        get { return longitudeOfAscendingNode; }
    }


    public DimensionsEllipse(Vector3 centre, float semiMajorAxis, float eccentricity, float inclination, float longitudeOfAscendingNode, float angularOffset, float orbitalPeriod)
    {
        this.centre = centre;
        this.SemiMajorAxis = semiMajorAxis;
        this.Eccentricity = eccentricity;
        this.inclination = inclination;
        this.longitudeOfAscendingNode = longitudeOfAscendingNode;
        this.angularOffset = angularOffset;
        this.orbitalPeriod = orbitalPeriod; 
        this.rotation = Quaternion.Euler(new Vector3(inclination, longitudeOfAscendingNode, angularOffset));
    }

    public float SemiMajorAxis {
        get { return semiMajorAxis; }
        set { 
            this.semiMajorAxis = value;
            this.semiMinorAxis = Mathf.Sqrt(semiMajorAxis * semiMajorAxis * (1 - eccentricity * eccentricity));
        }
    }
    public float Eccentricity {
        get { return eccentricity; }
        set {
            eccentricity = value;
            this.semiMinorAxis = Mathf.Sqrt(semiMajorAxis * semiMajorAxis * (1 - eccentricity * eccentricity));
        }
    }
    public Vector3 getRotatedPoint(Vector3 point, Quaternion rot)
    {
        Vector3 dir = point - centre;
        dir = rot * dir;
        return centre + dir;
    }
    public Vector3 EvaluatePositionOnEllipse(float percentageAroundEllipse)
    {
        percentageAroundEllipse = Mathf.Clamp01(percentageAroundEllipse);
        float angle = percentageAroundEllipse * 360 * Mathf.Deg2Rad;
        Vector3 point = new Vector3(semiMajorAxis * Mathf.Sin(angle), semiMinorAxis * Mathf.Cos(angle)) + centre;
        Vector3 orientedPoint = getRotatedPoint(point, Quaternion.Euler(new Vector3(inclination, longitudeOfAscendingNode, angularOffset)));
        return orientedPoint;
    }
}