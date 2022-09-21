using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShaderTest : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture renderTexture; //Dynamic Render Texture - set as the target of our compute shader.
    void Start()
    {
        renderTexture = new RenderTexture(256, 256, 24);
        renderTexture.enableRandomWrite = true; //Need to enable RandomWrite property for this texture to be used by the shader
        renderTexture.Create();

        /*SetTexture(KernelIndex (the index of the function we're calling), nameID ("variable we're outputting to in shader), texture*/
        computeShader.SetTexture(0, "Result", renderTexture);
        //We now execute the shader using the dispatch method, causing our shader to draw to our renderTexture.
        //Here, we tell the compute shader the index of the kernel to use (ie, what function defined in the shader
        //that we want to call) and how many thread groups to use. This is calculated by dividing our texture dimensions by the corresponding dimensions of our thread groups.
        //by 
        computeShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);
        
    }

    /*Render textures are textures that can be rendered to.

    They can be used to implement image based rendering effects, dynamic shadows, projectors, reflections or surveillance cameras.

    One typical usage of render textures is setting them as the "target texture" property of a Camera (Camera.targetTexture), this will make a camera render into a texture instead of rendering to the screen.*/
}
