using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class ComputeShaderToCameraTest : ScriptableRenderPass { 
    public ComputeShader computeShader;
    public RenderTexture renderTexture; //Dynamic Render Texture - set as the target of our compute shader.

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        throw new System.NotImplementedException();
    }


    //OnRenderImage - Event method that is called after the camera has finished rendering, allowing you to 
    //modify the camera's final image. Attach this to our camera.


}