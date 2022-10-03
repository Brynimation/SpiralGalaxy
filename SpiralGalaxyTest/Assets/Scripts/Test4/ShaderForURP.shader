/*ShaderLab is a Unity specific language that bridges the gap between HLSL and Unity. Everything
defined outside of the Passes is written in ShaderLab. Everything within the passes
is written in HLSL.
https://www.youtube.com/watch?v=gY1Mx4kkZPU&t=603s
https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics
*/

Shader "Unlit/ShaderForURP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _baseColour ("Base Colour", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        /*Between HLSLINCLUDE and ENDHLSL, we're going to set up everything we need to use 
        in our HLSL pass. Everything within this block will be available to all the passes we define*/
        HLSLINCLUDE
        #include "Packages/com.unity.render-rpipelines.universal/ShaderLibrary/Core.hlsl"
        //we need to include the properties our shader is going to use between the cbuffer start and 
        //cbuffer end Tags
        CBUFFER_START(UnityPerMaterial) //The UnityPerMaterial parameter ensures these properties are consistent among passes.
        //By defining all of our properties within a buffer, we make our material compatible with SRP Batcher, which makes rendering
        //happen faster.
            float4 _baseColour;
        CBUFFER_END

        //Textures don't need to go within the cbuffer
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        /*Shaders usually consist of a vertex shader and a fragment shader.
         Shader graph handles the receipt of certain input from the mesh for the user, such
         as vertex positions and UV coordinates. The properties of our shader are also passed as input.

         The vertex shader operates on every vertex of the mesh that the shader is attached to. It
         will output a 2D screenspace position for each of these vertices. We must first create a
         struct that is passed as input to our VertexShader*/

        struct VertexInput
        {
            float4 position : POSITION;
            float2 uv : TEXCOORD0; 
        }

        /*A semantic is a string attached to a shader input or output that conveys information 
        about the intended use of a parameter. Semantics are required on all variables passed 
        between shader stages.*/

        /*Output by our vertex shader, passed to our fragment shader.*/
        struct v2f 
        {
            float4 position : SV_POSITION; //SV_POSITION = semantic = System Value position - pixel position
            float2 uv : TEXCOORD0;
        }

        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            //pragma directives
            #pragma vertex vert
            #pragman fragment frag
            //Vertex shader
            v2f vert(VertexInput i)
            {
                v2f o; //What our function will output.
                //We need to transform the vertices from object space, where each vertex is positioned
                //relative to the object's centre, to clip space, where each vertex is positioned relative
                //to the 2D screen coordinates.'
                o.position = TransformObjectToHClip(i.position.xyz);
                o.uv = i.uv;
            }
            //Fragment Shader
            /*In a process known as rasterisation, post vertex shader, HLSL takes all triangle pixels currently
            on screen and turns them to fragments. Our fragment shader will operate on every one of these and 
            return a colour : the final colour of those fragments.
            */
            float4 frag(v2f i) : SV_Target 
            {
                //Sample the main texture at the correct uv coordinate using the SAMPLE_TEXTURE_2D macro, and 
                //then passing in the main texture, its sampler and the specified uv coordinate
                float4 baseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return baseTex * _baseColour;
            }
            ENDHLSL
        }
    }
}
