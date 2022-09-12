using UnityEngine;


/*We want our orbitting body to follow a particular path. They will be held in arrays in our Orbit class.
 The orbitting body must keep track of its position around the orbit. This will be in the form of curTime/orbitalPeriod, which will be passed to the evaluatePointOnEllipse method */
public class OrbittingBody : MonoBehaviour
{

    [Header("Body Properties")]
    [SerializeField] float radius;

    [Header("Orbit Properties")]
    public DimensionsEllipse orbitalProperties;
    [SerializeField] private float currentTime; //Will be set on instantiation. 
    [SerializeField] public float orbitalPeriod; //time taken to complete a full orbit
    [SerializeField] public float currentPercentage;
    /*TO DO: I think the orbitting bodies start not in the orbit on pressing play due to the way they're instantiated. Look into this please thank you*/

    private void Awake()
    {
        if (Application.isPlaying) 
        {
            setInitialPositionInOrbit();  
        }

    }
    private void setInitialPositionInOrbit() 
    {
        transform.position = this.orbitalProperties.EvaluatePositionOnEllipse(currentPercentage);
    }
    public void setInitialPositionInOrbit(float percent, DimensionsEllipse orbit) 
    {
        this.orbitalProperties = orbit;
        this.orbitalPeriod = orbit.orbitalPeriod;
        currentTime = percent * this.orbitalPeriod;
        currentPercentage = percent;
        transform.position = this.orbitalProperties.EvaluatePositionOnEllipse(percent);

    }

    public void setCurrentPositionInOrbit(DimensionsEllipse orbit) 
    {
        this.orbitalProperties = orbit;
        this.orbitalPeriod = orbit.orbitalPeriod;
        transform.position = this.orbitalProperties.EvaluatePositionOnEllipse(currentPercentage);
    }

    void Update()
    {
        if (currentTime > orbitalPeriod) currentTime = 0f;
        currentTime += Time.deltaTime;
        float percent = currentTime / orbitalPeriod;
        transform.position = orbitalProperties.EvaluatePositionOnEllipse(percent);
    }
}
