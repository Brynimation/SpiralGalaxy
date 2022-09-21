using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 In the central bulge, surface brightness can be estimated using de Vacouler's law:
 ln(I(R)) = ln(I0) - KR^0.25
 If we choose some value for k and take the exponent of both sides, we get 
 I(R) = I0 * e^(-kR^0.25)
    - I0 is maximum intensity
    - R is the distance from the galactic centre
    - In the beltoforion article, they use I0 = 1.0 and k = 0.02

Note the idea of half-light radius (Re) - the radius at which half the total light of a galaxy is emitted.
To get a fuckin fat load of stars in the centre, we can define this number to be quite small.
I(R) = Ie * e^(-7.67 * Mathf.Pow((R / Re, 0.25) - 1)), where Ie is the surface brightness at Re.  

 So I THINK we'll need to use an inverse Cumulative Distribiton Function.
//This will be called numOrbits times, take in a random number and return a DISTANCE from the centre of the 
//galaxy, which will be the size of the orbit we spawn.
 In the disk (ie, beyond the bounds of the central bulge into the spiral arms), surface
 brightness can be determined using ln

//Parameters:
    //-Central Intensity - 1.0
    //-Intensity (value input to our function? Will act as a probability to return the size of a current star's orbit - the semi major axis).
    //-Distance from Centre
    //-Maximum orbit size
    //-Total number of orbits
    //
    //GetIntensity(distance){
        return Ie * e^(-7.67 * Mathf.Pow((R / Re, 0.25) - 1))
    }
    //
 
 */

public class SpiralGalaxy : MonoBehaviour
{
    [Header("Galaxy Properties")]
    [Range(1, 10000)]
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
    float halfLightRadius;

    const float MINIMUM_ORBIT_SIZE = 1;
    List<Orbit> orbits;

    [Header("Line Properties")]
    [SerializeField] bool displayOrbits;
    [SerializeField] Material material;
    [SerializeField] float lineThickness;

    BodyDistributor BodyDistributor;

    private float quadraticEaseIn(float percent) 
    {
        if (percent <= 0.5f) return 2 * percent * percent;
        percent -= 0.5f;
        return 2.0f * percent * (1.0f - percent) + 0.5f;
    }

    private float exponentialEaseOut(float percent) 
    {
        return percent == 0 ? 0 : Mathf.Exp(percent);
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
        //Calculate size/radius of orbit based on the number of orbits
        float semiMajorAxis = Mathf.Lerp(MINIMUM_ORBIT_SIZE, galaxyHaloSize, percent * percent * percent * percent * percent);
        float angleOfInclination = Mathf.Lerp(0, maxAngleOfInclination, percent);
        float longitudeOfAscendingNode = Mathf.Lerp(0, maxLongitudeOfAscendingNode, percent);
        float angularOffset = Mathf.Lerp(0, maxAngularOffset, percent);
        float eccentricity = getEccentricity(semiMajorAxis);
        float orbitalPeriod = Mathf.Lerp(minMaxOrbitalPeriod.x, minMaxOrbitalPeriod.y, Mathf.Pow(semiMajorAxis, 1.5f)/Mathf.Pow(galaxyRadius, 1.5f));
        return new DimensionsEllipse(transform.position, semiMajorAxis, eccentricity, angleOfInclination, longitudeOfAscendingNode, angularOffset, orbitalPeriod);
    }

    private DimensionsEllipse GenerateOrbitProperties2(int orbitNum) 
    {
        //To ensure that the majority of our stars are distributed in the centre, we will
        //use the half light radius of our galaxy. We will only instantiate orbits outside of this
        //radius if we've already created half the total orbits.
        float minOrbitSize = (orbitNum > numOrbits / 2) ? MINIMUM_ORBIT_SIZE : halfLightRadius;
        float maxOrbitSize = (orbitNum > numOrbits / 2) ? galaxyHaloSize : halfLightRadius;
        float random = Random.Range(minOrbitSize, maxOrbitSize);
        float semiMajorAxis = BodyDistributor.GenerateOrbitRadius(random);
        float percent = random / (float)semiMajorAxis;
        float angleOfInclination = Mathf.Lerp(0, maxAngleOfInclination, percent);
        float longitudeOfAscendingNode = Mathf.Lerp(0, maxLongitudeOfAscendingNode, percent);
        float angularOffset = Mathf.Lerp(0, maxAngularOffset, percent);
        float eccentricity = getEccentricity(semiMajorAxis);
        float orbitalPeriod = Mathf.Lerp(minMaxOrbitalPeriod.x, minMaxOrbitalPeriod.y, Mathf.Pow(semiMajorAxis, 1.5f) / Mathf.Pow(galaxyRadius, 1.5f));
        return new DimensionsEllipse(transform.position, semiMajorAxis, eccentricity, angleOfInclination, longitudeOfAscendingNode, angularOffset, orbitalPeriod);
    }
    public void InitialiseOrbits() 
    {
        //Initialise our orbits, if none exist
        BodyDistributor = new BodyDistributor(1.0f, 0.02f, MINIMUM_ORBIT_SIZE, galacticCoreRadius, galaxyRadius
            );
        if (orbits == null || orbits.Count == 0)
        {
            galaxyHaloSize = galaxyRadius * 2;
            halfLightRadius = galacticCoreRadius;
            orbits = new List<Orbit>();
            for (int i = 0; i < numOrbits; i++)
            {
                Orbit curOrbit = Instantiate(orbitPrefab, transform.position, Quaternion.identity) as Orbit;
                curOrbit.SetOrbitProperties(numBodiesPerOrbit, GenerateOrbitProperties2(i), displayOrbits, lineThickness);
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
