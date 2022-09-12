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

    [Header("Galaxy Radius")]
    [Range(1, 10000)]
    [SerializeField] float galaxyRadius;
    [SerializeField] Orbit orbitPrefab;

    const float MINIMUM_ORBIT_SIZE = 1;
    List<Orbit> orbits;

    [Header("Line Properties")]
    [SerializeField] bool displayOrbits;
    [SerializeField] Material material;
    [SerializeField] float lineThickness;

    private DimensionsEllipse GenerateOrbitProperties(int orbitNum) 
    {
        float percent = orbitNum / (numOrbits + 0.0f);
        float semiMajorAxis = Mathf.Lerp(MINIMUM_ORBIT_SIZE, galaxyRadius, percent);
        float angleOfInclination = Mathf.Lerp(0, maxAngleOfInclination, percent);
        float longitudeOfAscendingNode = Mathf.Lerp(0, maxLongitudeOfAscendingNode, percent);
        float eccentricity = Mathf.Lerp(0, 1, percent);
        float orbitalPeriod = Mathf.Lerp(minMaxOrbitalPeriod.x, minMaxOrbitalPeriod.y, Mathf.Pow(semiMajorAxis, 1.5f)/Mathf.Pow(galaxyRadius, 1.5f));
        return new DimensionsEllipse(transform.position, semiMajorAxis, eccentricity, angleOfInclination, longitudeOfAscendingNode, orbitalPeriod);
    }
    private void ChangeOrbitSizes() 
    {
        //if just the size of the galaxy has been updated
        for (int i = 0; i < numOrbits; i++)
        {
            Debug.Log(i);
            orbits[i].semiMajorAxis = Mathf.Lerp(MINIMUM_ORBIT_SIZE, galaxyRadius, (i / (numOrbits + 0.0f)));
            orbits[i].DrawEllipse();
        }
    }

    public void InitialiseOrbits() 
    {
        //Initialise our orbits, if none exist
        if (orbits == null || orbits.Count == 0)
        {
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
