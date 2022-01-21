using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "BTS/VectorLine/Pipeline Asset")]
public class VectorLinePipelineAsset : RenderPipelineAsset {
    public Shader m_basicShader;

    protected override RenderPipeline CreatePipeline() {
        return new VectorLinePipeline(this);
    }
}