using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



public class SimpleDesaturateEffect : ScriptableRendererFeature
{
    DesaturateRenderPass renderPass; //hold an instnace of our ScriptableRenderPass.
    class DesaturateRenderPass : ScriptableRenderPass
    {
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public Material material;
        public RenderTargetIdentifier source; //Identifies a RenderTexture for a Rendering.CommandBuffer
        public RenderTargetHandle tempTexture;

        public DesaturateRenderPass(Material material) : base()
        {
            this.material = material;
            tempTexture.Init("_TempDesaturateFeature");
        }
        public void SetSource(RenderTargetIdentifier source)
        {
            this.source = source;
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //First, we get a command buffer, which stores our commands. The string is for debugging - not important. 
            

            //One restriction of render targets is that we cannot read to and write from the same texture at once.
            //We encounter this problem as we are wanting to both read from and write to the camera's render target.
            //This is what we use our tempTexture for. We want this to have the same properties/format as the camera
            //texture but without a depth buffer.
            //The temptexture variable now contains a texture we can write to

            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(tempTexture.id, descriptor, FilterMode.Bilinear);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers - This is basically
        // where we send rendering commands to the GPU.
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("SimpleDesaturateEffect");

            //We now need to enqueue commands into the command buffer. To do this, we'll use the Blit command.
            //First, we run the shader on the source, save it to the temp, then transfer it back to the source.
            Blit(cmd, source, tempTexture.Identifier(), material, 0);
            Blit(cmd, tempTexture.Identifier(), source);

            //These last two lines let Unity know that the command buffer has been filled and can be recycled.
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            //Free up the memory that our temporary texture used.
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }

    /// <inheritdoc/>
    public override void Create()
    {
        renderPass = new DesaturateRenderPass(new Material(Shader.Find("Shader Graphs/Desaturate")));

        // Configures where the render pass should be injected.
        renderPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderPass.SetSource(renderer.cameraColorTarget);
        renderer.EnqueuePass(renderPass);
    }
}


