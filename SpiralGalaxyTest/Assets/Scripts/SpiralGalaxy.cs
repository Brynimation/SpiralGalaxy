using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiralGalaxy : MonoBehaviour
{
    [Header("Galaxy Properties")]
    [Range(0, 360)]
    [SerializeField] float numOrbits;
    [Range(1, 10)]
    [SerializeField] int numBodiesPerOrbit;
    [SerializeField] Vector2 minMaxOrbitalPeriod;
    [Range(0, 360)]
    [SerializeField] float maxAngleOfInclination;
    [Range(0, 360)]
    [SerializeField] float maxLongitudeOfAscendingNode;
    [Range(0, 720)]
    [SerializeField] float maxAngularOffset;
    [Range(0, 1)]
    [SerializeField] float minEccentricity;
    [Range(0, 1)]
    [SerializeField] float maxEccentricity;

    [Header("Galaxy Radius")]
    [SerializeField] float galaxyHaloSize;
    [Range(1, 10000)]
    [SerializeField] float galaxyRadius;
    [Range(1, 1000)]
    [SerializeField] float galacticCoreRadius;
    [SerializeField] Orbit orbitPrefab;

    const float MINIMUM_ORBIT_SIZE = 1;
    List<Orbit> orbits;

    [Header("Line Properties")]
    [SerializeField] bool displayOrbits;
    [SerializeField] Material material;
    [SerializeField] float lineThickness;

    private float quadraticEaseIn(float percent) 
    {
        if (percent <= 0.5f) return 2 * percent * percent;
        percent -= 0.5f;
        return 2.0f * percent * (1.0f - percent) + 0.5f;
    }


    /*Eccentricity is determined by the radius of the current orbit.*/
    private float getEccentricity(float semiMajorAxis) 
    {
        //ratio of semiMajorAxis/galaxyCore gives the lerpPercent.

        if (semiMajorAxis >= MINIMUM_ORBIT_SIZE && semiMajorAxis < galacticCoreRadius)
        {
            return Mathf.Lerp(0, minEccentricity, quadraticEaseIn(semiMajorAxis / galacticCoreRadius));
        }
        else if (semiMajorAxis >= galacticCoreRadius && semiMajorAxis < galaxyRadius) 
        {
            return Mathf.Lerp(minEccentricity, maxEccentricity, quadraticEaseIn((semiMajorAxis - galacticCoreRadius)/galaxyRadius) );
        }
        return Mathf.Lerp(maxEccentricity, 0, quadraticEaseIn((semiMajorAxis - galaxyRadius)/(galaxyHaloSize)));
    }

    private DimensionsEllipse GenerateOrbitProperties(int orbitNum) 
    {
        float percent = orbitNum / (numOrbits + 0.0f);
        float semiMajorAxis = Mathf.Lerp(MINIMUM_ORBIT_SIZE, galaxyHaloSize, percent);
        float angleOfInclination = Mathf.Lerp(0, maxAngleOfInclination, percent);
        float longitudeOfAscendingNode = Mathf.Lerp(0, maxLongitudeOfAscendingNode, percent);
        float angularOffset = Mathf.Lerp(0, maxAngularOffset, percent);
        float eccentricity = getEccentricity(semiMajorAxis);
        float orbitalPeriod = Mathf.Lerp(minMaxOrbitalPeriod.x, minMaxOrbitalPeriod.y, Mathf.Pow(semiMajorAxis, 1.5f)/Mathf.Pow(galaxyRadius, 1.5f));
        return new DimensionsEllipse(transform.position, semiMajorAxis, eccentricity, angleOfInclination, longitudeOfAscendingNode, angularOffset, orbitalPeriod);
    }
    public void InitialiseOrbits() 
    {
        //Initialise our orbits, if none exist
        if (orbits == null || orbits.Count == 0)
        {
            galaxyHaloSize = galaxyRadius * 2;
            orbits = new List<Orbit>();
            for (int i = 0; i < numOrbits; i++)
            {
                Orbit curOrbit = Instantiate(orbitPrefab, transform.position, Quaternion.identity) as Orbit;
                curOrbit.SetOrbitProperties(numBodiesPerOrbit, GenerateOrbitProperties(i), displayOrbits, lineThickness);
                curOrbit.DrawEllipse();
                orbits.Add(curOrbit);
                curOrbit.transform.SetParent(transform);
            }
            //If we've just decreased the number of orbits
        }
        else if (numOrbits < orbits.Count)
        {
            while (orbits.Count > numOrbits)
            {
                //Remove all the outermost orbits
                Orbit orbitToRemove = orbits[orbits.Count - 1];
                orbits.RemoveAt(orbits.Count - 1);
                GameObject.Destroy(orbitToRemove.gameObject);
            }
            //Now adjust the properties of the remaining orbits
            for (int i = 0; i < numOrbits; i++)
            {
                orbits[i].SetOrbitProperties(numBodiesPerOrbit, GenerateOrbitProperties(i), displayOrbits, lineThickness);
                orbits[i].DrawEllipse();
            }
        }
        //if we just increased the number of orbits
        else if (orbits.Count < numOrbits) 
        {
            int count = orbits.Count;
            //First add additional orbits
            for (int i = count; i < numOrbits; i++) 
            {
                Orbit curOrbit = Instantiate(orbitPrefab, transform.position, Quaternion.identity) as Orbit;
                curOrbit.SetOrbitProperties(numBodiesPerOrbit, GenerateOrbitProperties(i), displayOrbits, lineThickness);
                orbits.Add(curOrbit);
                curOrbit.transform.SetParent(transform);
            }
            for (int i = 0; i < numOrbits; i++)
            {
                orbits[i].SetOrbitProperties(numBodiesPerOrbit, GenerateOrbitProperties(i), displayOrbits, lineThickness);
                orbits[i].DrawEllipse();
            }
        }
        //If we change the size, minMaxOrbitalPeriods or line thickness and the number of orbits does not change.
        else //2if (orbits.Count == numOrbits) 
        {
            for (int i = 0; i < numOrbits; i++)
            {
                orbits[i].SetOrbitProperties(numBodiesPerOrbit, GenerateOrbitProperties(i), displayOrbits, lineThickness);
                orbits[i].DrawEllipse();
            }
        }
        
       

        
    }
    public void InitialiseOrbits1()
    {
        //if our list of orbits is currently empty, first we gotta initialise em
        /*if (orbits == null || orbits.Count == 0 || numOrbits == 0)
        {
            orbits = new List<Orbit>();
            for (int i = 0; i < numOrbits; i++)
            {
                Orbit orbit = Instantiate(orbitPrefab, transform.position, Quaternion.identity) as Orbit;
                (DimensionsEllipse newOrbit, float orbitalPeriod) = GenerateEllipse(i);
                orbit.SetOrbitProperties(orbitalPeriod, numBodiesPerOrbit, newOrbit);
                orbit.SetLineRendererProperties(material, lineThickness);
                orbit.DrawEllipse();
                orbits.Add(orbit);
            }

        }
        else if (numOrbits == orbits.Count)
        {
            //If just galaxyRadius has been updated
            ChangeOrbitSizes();

        }//if the number of bodies has been decreased.
        else if (numOrbits < orbits.Count)
        {
            while (orbits.Count > numOrbits)
            {
                Orbit o = orbits[orbits.Count - 1];
                orbits.RemoveAt(orbits.Count - 1);
                if (!Application.isPlaying)
                {
                    DestroyImmediate(o.gameObject);
                }
                else
                {
                    Destroy(o.gameObject);
                }
            }
            ChangeOrbitSizes();
        }
        //if the number of bodies has increased.
        else if (numOrbits > orbits.Count)
        {
            for (int i = orbits.Count; i < numOrbits; i++)
            {
                Orbit orbit = Instantiate(orbitPrefab, transform.position, Quaternion.identity) as Orbit;
                (DimensionsEllipse newOrbit, float orbitalPeriod) = GenerateEllipse(i);
                orbit.SetOrbitProperties(orbitalPeriod, numBodiesPerOrbit, newOrbit);
                orbit.SetLineRendererProperties(material, lineThickness);
                orbits.Add(orbit);
            }
            ChangeOrbitSizes();
        }
    }*/
    }
}
