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
To get a load of stars in the centre, we can define this number to be quite small.
I(R) = Ie * e^(-7.67 * Mathf.Pow((R / Re, 0.25) - 1)), where Ie is the surface brightness at Re.  

//This will be called numOrbits times, take in a random number between our mand return a DISTANCE from the centre of the 
//galaxy, which will be the size of the orbit we spawn.
 In the disk (ie, beyond the bounds of the central bulge into the spiral arms), surface
 brightness can be determined using ln

 EXCEPT: I think all we need to do really is take in a distance and then shift it. We lerp between two distance values

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
[System.Serializable]
public class BodyDistributor
{
    [SerializeField] int numDataPoints;
    [SerializeField] float centralIntensity; //I guess  this really just acts like a constant
    [SerializeField] float k; //So does this - we can fancy this up with half-light radius and such later (maybe)
    [SerializeField] float minOrbitSize;
    [SerializeField] float galacticCoreRadius;

    [SerializeField] float galaxyDiskRadius;
    /*Scale length is used to describe quantities that decline at an exponential rate. In the context of galaxies, 
     The scale length is the radius at which the galaxy is a factor of e (~2.7) less bright than it is at its center*/
    [SerializeField] float scaleLength;

    [SerializeField] float galaxyHaloSize;
    [SerializeField] float[] orbitValues; 

    

    public BodyDistributor(float centralIntensity, float k, float minOrbitSize, float galacticCoreRadius, float galaxyDiskRadius) 
    {
        this.centralIntensity = centralIntensity;
        this.k = k;
        this.minOrbitSize = minOrbitSize;
        this.galacticCoreRadius = galacticCoreRadius;
        this.galaxyDiskRadius = galaxyDiskRadius;
        this.scaleLength = Mathf.Exp(1);
        this.galaxyHaloSize = galaxyDiskRadius * 2;

    }

    public float GenerateOrbitRadius(float val)
    {
        Debug.Log(val + " " + GetDistanceFromCentre(val));
        return GetDistanceFromCentre(val);
    }

    private float GetDistanceFromCentreInCore(float IO, float val, float k) 
    {
        return centralIntensity * Mathf.Exp(-k  * Mathf.Pow(val, 0.25f));
    }
    private float GetDistanceFromCentreInDisk(float IO, float val, float scaleLength) 
    {
        
        return centralIntensity * Mathf.Exp(-val / scaleLength);
    }
    private float GetDistanceFromCentre(float val) 
    {
        return (val < galacticCoreRadius) ? GetDistanceFromCentreInCore(centralIntensity, val, k) : GetDistanceFromCentreInDisk(val - galacticCoreRadius, GetDistanceFromCentreInCore(galacticCoreRadius, centralIntensity, k), scaleLength); 
    }
}
