using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "BTS/VectorLine/Pipeline Asset")]
public class VectorLinePipelineAsset : RenderPipelineAsset {
    protected override RenderPipeline CreatePipeline() {
        return new VectorLinePipeline(this);
    }
}