using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalProperties
{
    Vector3 centre;
    float semiMajorAxis;
    float eccentricity;
    float semiMinorAxis;
    float inclination;
    float longitudeOfAscendingNode;
    float angularOffset;
    float orbitalPeriod;
    public OrbitalProperties(Vector3 centre, float semiMajorAxis, float eccentricity, float inclination, float longitudeOfAscendingNode, float angularOffset, float orbitalPeriod)
    {
        this.centre = centre;
        this.semiMajorAxis = semiMajorAxis;
        this.eccentricity = eccentricity;
        this.inclination = inclination;
        this.longitudeOfAscendingNode = longitudeOfAscendingNode;
        this.angularOffset = angularOffset;
        this.orbitalPeriod = orbitalPeriod;
    }
}
