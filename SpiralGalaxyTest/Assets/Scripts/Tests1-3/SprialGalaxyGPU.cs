using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprialGalaxyGPU : MonoBehaviour
{

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

    [Header("Line Properties")]
    [SerializeField] bool displayOrbits;
    [SerializeField] Material material;
    [SerializeField] float lineThickness;

    BodyDistributor BodyDistributor;
    [Header("Compute Shader Properties")]
    /*
     We need to know a few properties of the computeShader. We can access these properties 
    through their unity-assigned ids. These ids are claimed on demand and remain the same 
    while our app is running, so we can directly store these in static fields. Further, since
    the values of these fields never change, we can make them readonly.
     */
    [SerializeField] ComputeShader computeShader;
    ComputeBuffer positionsBuffer;

    static readonly int
        positionsID = Shader.PropertyToID("_Positions"),
        resolutionID = Shader.PropertyToID("_Resolution"),
        stepID = Shader.PropertyToID("_Step"),
        timeID = Shader.PropertyToID("_Time");

    /*The points are already on the GPU. We don't need to create gameobjects anymore, we just
     need to tell the gpu to redraw the same mesh with the same material many times at these points.*/
    [SerializeField] Material mat;

    [SerializeField] Mesh mesh;


    /*So that our compute shader can actually use the correct values of these properties,
     we have a dedicated function to set them - called each frame in the event of change.*/
    void UpdateFunctionGpu()
    {
        computeShader.SetFloat(timeID, Time.time);

        //Setting the positions buffer is slightly different. We're not copying any data,
        //but rather linking the buffer to the kernel. The extra argument required is the 
        //kernal index, which is necessary since a compute shader CAN have multiple kernels,
        //and buffers can be linked to specific ones.
        int kernelIndex = computeShader.FindKernel("CSMain");
        computeShader.SetBuffer(kernelIndex, positionsID, positionsBuffer);

        mat.SetBuffer(positionsID, positionsBuffer);
        /*After setting the buffer, we can run our kernel calling the Dispatch method on our compute shader. It takes 4 arguments, the kernel index followed by the number of 
         thread groups to run in each dimension. For example, if we use 1 for each dimension, only the first group of 8 x 8 positions would be calculated.
        Because of our fixed group size, the minimum number of groups we need in the x and y
        dimensions equals resolution/8 rounded up*/
        int threadGroupSize = Mathf.CeilToInt(numOrbits / 128f);
        Debug.Log(threadGroupSize);
        computeShader.Dispatch(kernelIndex, threadGroupSize, 1, 1);

        /*Procedural drawing is done by invoking Graphics.DrawMeshInstancesProcedural with a mesh, sub-mesh index and material as arguments. The sub-mesh index we can leave as zero since our mesh does not consist of multiple parts.
         Since this way of drawing doesn't use gameobjects, Unity doesn't know where to draw the mesh. Consequenctly, this method takes an additional Bound argument - which defines the bounding volume within which the mesh may be drawn. This is an axis aligned bounding box that indicates the spatial bounds of whatever we're drawing.
        The renderer needs to know this, as if the mesh is entirely out of view of the camera then lots of resources can be saved by not drawing the mesh at all. This is known 
        as frustrum culling. So, instead of evaluating the bounds on a point-by-point basis,
        we do it once for the whole graph. This is fine since we're always looking at our whole graph anyway.
             
         Our graph sits at the origin with a domain of [-1,1]. It therefore sits in a box of size 2. But since the points have size, we must account for their potential stickage outage of the boxage.2 Finally, we pass in how many instances to draw - the count of our positions buffer*/
        Bounds bounds = new Bounds(transform.position, Vector3.one * galaxyHaloSize);
        Debug.Log(positionsBuffer.count);
        Graphics.DrawMeshInstancedProcedural(mesh, 0, mat, bounds, numOrbits);

    }


    /*We create our ComputeBuffer in the OnEnable method as they do not survive hot reloads.
     This means that if we change code while in play mode it will disappear. This happens whenever a hot reload occurs, as well as when the gameobject is enabled.*/


    private void OnEnable()
    {
        //To create a compute buffer, we need the number of elements it's going to store, ie
        //the number of points on our graph, and also the Stride - the number of bytes each element in the buffer will take up. For us, this is Vector3s, or float3s.
        positionsBuffer = new ComputeBuffer(numOrbits, sizeof(float) * 3);
    }
    /*We also need a companion OnDisable method. This is called whenever the graph is destroyed or right before a hot reload. Call the release() method on the buffer, which
     indicates that the GPU memory claimed by the buffer can be freed immediately and 
    hence used by other processes. This avoids clogging memory.
    
     It's also best to dereference our compute shader, making it possible for the object 
    to be reclaimed by Unity's Garbage Collector the next time this method runs.*/

    private void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
    }

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
            return Mathf.Lerp(minEccentricity, maxEccentricity, quadraticEaseIn((semiMajorAxis - galacticCoreRadius) / galaxyRadius));
        }
        return Mathf.Lerp(maxEccentricity, 0, quadraticEaseIn((semiMajorAxis - galaxyRadius) / (galaxyHaloSize)));
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
        float orbitalPeriod = Mathf.Lerp(minMaxOrbitalPeriod.x, minMaxOrbitalPeriod.y, Mathf.Pow(semiMajorAxis, 1.5f) / Mathf.Pow(galaxyRadius, 1.5f));
        return new DimensionsEllipse(transform.position, semiMajorAxis, eccentricity, angleOfInclination, longitudeOfAscendingNode, angularOffset, orbitalPeriod);
    }

    private void Update()
    {
        InitialiseOrbits();
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
}
