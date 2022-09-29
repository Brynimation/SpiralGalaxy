using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbittingBodyGPU 
{
    public DimensionsEllipse orbitalProperties;
    Vector3 position; 
    public float currentPercentage;
    private float currentTime; //Will be set on instantiation. 

    private void setInitialPositionInOrbit()
    {
        position = orbitalProperties.EvaluatePositionOnEllipse(currentPercentage);
    }
    public void setInitialPositionInOrbit(float percent, DimensionsEllipse orbit)
    {
        this.orbitalProperties = orbit;
  
        currentTime = percent * this.orbitalProperties.orbitalPeriod;
        currentPercentage = percent;
        position = this.orbitalProperties.EvaluatePositionOnEllipse(percent);

    }

    public void setCurrentPositionInOrbit(DimensionsEllipse orbit)
    {
        this.orbitalProperties = orbit;
        position = this.orbitalProperties.EvaluatePositionOnEllipse(currentPercentage);
    }

    void UpdateBody()
    {
        if (currentTime > orbitalProperties.orbitalPeriod) currentTime = 0f;
        currentTime += Time.deltaTime;
        float percent = currentTime / orbitalProperties.orbitalPeriod;
        //We want to go from setting a gameObject's position -> storing a position on the 
        position = orbitalProperties.EvaluatePositionOnEllipse(percent);
    }

}
