using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiralGalaxyManager : MonoBehaviour
{
    #region properties
    [Header("Galaxy Properties")]
    [Range(1, 10000)]
    [SerializeField] int numOrbits;
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

    const float MINIMUM_ORBIT_SIZE = 0.01f;
    List<Orbit> orbits;
    #endregion
    [Header("Compute Shader Properties")]
    #region ComputeShader
    [SerializeField] Material mat;
    [SerializeField] Mesh mesh;
    [SerializeField] int threadGroupSize;
    OrbittingBodyData[] bodyData;
    ComputeBuffer orbitBodyBuffer; //Holds the necessary structs to calculate new pos on gpu
    ComputeBuffer positionsBuffer; //Just holds the positions needed to be drawn
    ComputeShader computeShader;
    //static readonly int positionsId = Shader.PropertyToID("_Positions");
    static readonly int bodiesId = Shader.PropertyToID("_OrbittingBodies");
    static readonly int deltaTimeId = Shader.PropertyToID("_DeltaTime");

    #endregion
    private float quadraticEaseIn(float percent)
    {
        if (percent <= 0.5f) return 2 * percent * percent;
        percent -= 0.5f;
        return 2.0f * percent * (1.0f - percent) + 0.5f;
    }
    private float getEccentricity(float semiMajorAxis)
    {
        //ratio of semiMajorAxis/galaxyCore gives the lerpPercent.

        if (semiMajorAxis >= MINIMUM_ORBIT_SIZE && semiMajorAxis < galacticCoreRadius)
        {
            return Mathf.Lerp(0, minEccentricity, quadraticEaseIn(semiMajorAxis / galacticCoreRadius));
        }
        else if (semiMajorAxis >= galacticCoreRadius && semiMajorAxis < galaxyRadius)
        {
            return Mathf.Lerp(minEccentricity, maxEccentricity, quadraticEaseIn((semiMajorAxis - galacticCoreRadius) / galaxyRadius));
        }
        return Mathf.Lerp(maxEccentricity, 0, quadraticEaseIn((semiMajorAxis - galaxyRadius) / (galaxyHaloSize)));
    }

    void GenerateOrbitProperties(int orbitNum)
    {
        float percent = orbitNum / (numOrbits + 0.0f);
        //Calculate size/radius of orbit based on the number of orbits
        float semiMajorAxis = Mathf.Lerp(MINIMUM_ORBIT_SIZE, galaxyHaloSize, percent * percent * percent * percent * percent);
        float angleOfInclination = Mathf.Lerp(0, maxAngleOfInclination, percent);
        float longitudeOfAscendingNode = Mathf.Lerp(0, maxLongitudeOfAscendingNode, percent);
        float angularOffset = Mathf.Lerp(0, maxAngularOffset, percent);
        float eccentricity = getEccentricity(semiMajorAxis);
        float orbitalPeriod = Mathf.Lerp(minMaxOrbitalPeriod.x, minMaxOrbitalPeriod.y, Mathf.Pow(semiMajorAxis, 1.5f) / Mathf.Pow(galaxyRadius, 1.5f));
        bodyData[orbitNum] = new OrbittingBodyData(new OrbitalProperties(transform.position, semiMajorAxis, eccentricity, angleOfInclination, longitudeOfAscendingNode, angularOffset, orbitalPeriod));

    }
    private void InitialiseOrbits() 
    {
        bodyData = new OrbittingBodyData[numOrbits];
        for (int i = 0; i < numOrbits; i++) 
        {
            GenerateOrbitProperties(i);
        }
        orbitBodyBuffer = new ComputeBuffer(numOrbits, OrbittingBodyData.Size);
        orbitBodyBuffer.SetData(bodyData);
        computeShader.SetFloat(deltaTimeId, Time.deltaTime);
        int kernelIndex = computeShader.FindKernel("CSMain");
        computeShader.SetBuffer(kernelIndex, "_OrbitalBodies", orbitBodyBuffer);

        int numThreadGroups = Mathf.CeilToInt(numOrbits / threadGroupSize);
        computeShader.Dispatch(kernelIndex, numThreadGroups, 1, 1);

        mat.SetBuffer(bodiesId, orbitBodyBuffer);
        var bounds = new Bounds(Vector3.zero, Vector3.one * galaxyHaloSize);
        Graphics.DrawMeshInstancedProcedural(mesh, 0, mat, bounds, numOrbits);
        orbitBodyBuffer.Release();

        /*To do: Choose sphere as the mesh in the inspector. 
         * Create the material we need to make this work.
         In the likely event that this code does nothing once that's done, it may be necessary to not only update the body positions but also create the orbits on the GPU side. Might be worth going through https://toqoz.fyi/thousands-of-meshes.html*/

    }

    private void Update()
    {
        
    }

    /*Struct containing all the data the gpu needs to calculate the position of an orbitting body, with no gameobject or nothin. We'll just be instantiating spherical meshes for now.*/
    struct OrbittingBodyData {
        OrbitalProperties orbitalProperties;
        Vector3 position;
        float currentPercentage;
        float orbitStartTime;
        float timePassedSinceOrbitStart;
        public OrbittingBodyData(OrbitalProperties op)
        {
            orbitalProperties = op;
            position = Vector3.zero;
            currentPercentage = 0;
            orbitStartTime = 0;
            timePassedSinceOrbitStart = 0;
        }

        public OrbittingBodyData(OrbitalProperties op, Vector3 pos, float cp, float ost, float time) 
        {
            orbitalProperties = op;
            position = pos;
            currentPercentage = cp;
            orbitStartTime = ost;
            timePassedSinceOrbitStart = time;   
        }
        //Returns the stride of our compute buffer (memory taken up by each instance of
        //an OrbittingBodyData in bytes)
        public static int Size {
            get {
                return sizeof(float) * 16;
            }
        }
    }

    /*    Vector3 centre;
    float semiMajorAxis;
    float eccentricity;
    float semiMinorAxis;
    float inclination;
    float longitudeOfAscendingNode;
    float angularOffset;
    float orbitalPeriod;*/
}
