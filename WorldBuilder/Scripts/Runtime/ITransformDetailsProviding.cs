using System.Collections.Generic;

namespace SKFX.WorldBuilder {
    public interface ITransformDetailsProviding {
        long DetailsCount { get; }

        long GenerateDetails(TransformDetails[] transformDetailsOut, long startIndex, int snapLayerMask = 0);

        //IEnumerable<TransformDetails> GenerateDetails();
        //IEnumerable<TransformDetails> GenerateSnappedDetails(int snapLayerMask);
    }
}
