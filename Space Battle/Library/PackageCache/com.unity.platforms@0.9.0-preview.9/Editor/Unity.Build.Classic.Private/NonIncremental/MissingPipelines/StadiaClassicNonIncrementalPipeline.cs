using Bee.Core;
using UnityEditor;

namespace Unity.Build.Classic.Private.MissingPipelines
{
    /// <summary>
    /// Placeholder classic non incremental pipeline for Stadia
    /// Note: Should be remove when a proper implementation is done.
    /// </summary>
    class StadiaClassicNonIncrementalPipeline : MissingNonIncrementalPipeline
    {
        public override Platform Platform => new StadiaPlatform();

        protected override BuildTarget BuildTarget => BuildTarget.Stadia;
    }
}
