using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/*+DimensionsEllipse - defines the orbital path followed by all bodies following this orbit
   numBodies - defines the number of bodies following this orbit
 
 */
public class Orbit : MonoBehaviour
{
    public DimensionsEllipse orbitProperties;


    [SerializeField] OrbittingBody bodyPrefab;

    [Range(0, 10)] //max of 10 bodies per orbit
    [SerializeField] int numBodies;
    [SerializeField] float orbitalPeriod; //
    Stack<OrbittingBody> bodies;

    [Header("Ellipse Properties")]

    public float semiMajorAxis;
    [Range(0, 1)]
    public float eccentricity;
    [Range(3, 36)]
    public int resolution;
    [Range(0, 360)]
    public float inclination;
    [Range(0, 360)]
    public float longitudeOfAscendingNode;

    [Header("Line Properties")]
    public Material material;
    public bool displayEllipse;
    public float lineThickness;
    LineRenderer lr;

    Vector3[] pointsOnEllipse;


    public void SetOrbitProperties(int numBodies, DimensionsEllipse orbit, bool displayEllipse, float lineThickness) 
    {
        semiMajorAxis = orbit.semiMajorAxis;
        eccentricity = orbit.eccentricity;
        inclination = orbit.inclination;
        longitudeOfAscendingNode = orbit.longitudeOfAscendingNode;
        this.displayEllipse = displayEllipse;
        this.lineThickness = lineThickness;
        this.orbitalPeriod = orbit.orbitalPeriod;
        this.numBodies = numBodies;
        this.orbitProperties = orbit;
    }
    public void SetLineRendererProperties(Material mat, float thickness) 
    {
        this.material = mat;
        this.lineThickness = thickness;
    }

    public void InitialiseBodies() 
    {
        orbitProperties = new DimensionsEllipse(transform.position, semiMajorAxis, eccentricity, inclination, longitudeOfAscendingNode, orbitalPeriod);

        if (bodies == null || bodies.Count == 0)
        {
            
            bodies = new Stack<OrbittingBody>();
            for (int i = 0; i < numBodies; i++)
            {
                OrbittingBody body = Instantiate(bodyPrefab, transform.position, Quaternion.identity) as OrbittingBody;
                body.setInitialPositionInOrbit(Random.Range(0, 95) / 100f, orbitProperties);
                body.transform.SetParent(this.transform);
                bodies.Push(body);
            }
        }
        //If the properties of the ellipse has become different but the number of bodies is the same
        else if (numBodies == bodies.Count)
        {
            foreach (OrbittingBody body in bodies) body.setCurrentPositionInOrbit(orbitProperties);
        }
        //if the number of bodies has been decreased.
        else if (numBodies < bodies.Count)
        {
            while(bodies.Count > numBodies)
            {
                OrbittingBody b = bodies.Pop();
                if (!Application.isPlaying)
                {
                    DestroyImmediate(b.gameObject);
                }
                else {
                    Destroy(b.gameObject);
                }
            }
            if (bodies.Count == 0) {
                bodies = null;
                return;
            }
            foreach (OrbittingBody body in bodies) body.setCurrentPositionInOrbit(orbitProperties);
        }
        //if the number of bodies has increased.
        else if (numBodies > bodies.Count)
        {
            foreach (OrbittingBody body in bodies) body.setCurrentPositionInOrbit(orbitProperties);
            for (int i = bodies.Count; i < numBodies; i++)
            {
                OrbittingBody body = Instantiate(bodyPrefab, transform.position, Quaternion.identity) as OrbittingBody;
                body.setInitialPositionInOrbit(Random.Range(0, 95) / 100f, orbitProperties);
                body.transform.SetParent(this.transform);
                bodies.Push(body);
            }

        }
    }
    private void Awake()
    {
        foreach (Transform child in transform) 
        {
            Destroy(child.gameObject);
        }
        bodies = null;
        lr = GetComponent<LineRenderer>();
        lr.material = material;
        lr.startWidth = lineThickness;
        lr.endWidth = lineThickness;
        DrawEllipse();
    }

    public void DrawEllipse()
    {
        if (!displayEllipse)
        {
            if (lr != null)
            {
                lr.startWidth = 0f;
                lr.endWidth = 0f;
                InitialiseBodies();
            }
            return;
        }
        if (lr == null)
        {
            this.gameObject.AddComponent<LineRenderer>();
            lr = GetComponent<LineRenderer>();
        }
        InitialiseBodies();
        lr.gameObject.SetActive(true);
        lr.material = material;
        lr.startWidth = lineThickness;
        lr.endWidth = lineThickness;
        
        pointsOnEllipse = new Vector3[resolution + 1];

        

        Quaternion inclinationRotation = Quaternion.Euler(inclination * Mathf.Deg2Rad, 0f, 0f);
        float angleStep = 360 / resolution;
        for (int i = 0; i <= resolution; i++)
        {
            float curAngleDeg = i * angleStep;
            float curAngle = curAngleDeg * Mathf.Deg2Rad; //+ orbit.AngleOffsetRadians;
            Vector3 point = new Vector3(orbitProperties.semiMajorAxis * Mathf.Sin(curAngle), orbitProperties.SemiMinorAxis * Mathf.Cos(curAngle)) + transform.position;
            pointsOnEllipse[i] = orbitProperties.getRotatedPoint(point, Quaternion.Euler(orbitProperties.Inclination, orbitProperties.LongitudeOfAscendingNode, 0f));
        }

        //Before calling SetPositions we must access positionCount
        lr.positionCount = resolution + 1;
        lr.SetPositions(pointsOnEllipse);
        
    }
    private void OnApplicationQuit()
    {
        numBodies = 0;
        foreach (Transform child in transform) 
        {
            GameObject.Destroy(child.gameObject);
        }
    }
    void Start()
    {
        //DrawEllipse();

    }

}
