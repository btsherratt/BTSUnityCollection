using System.Collections.Generic;

namespace SKFX.WorldBuilder {
    public interface ITransformDetailsProviding {
        long DetailsCount { get; }

        IEnumerable<TransformDetails> GenerateDetails();
        IEnumerable<TransformDetails> GenerateSnappedDetails(int snapLayerMask);
    }
}
