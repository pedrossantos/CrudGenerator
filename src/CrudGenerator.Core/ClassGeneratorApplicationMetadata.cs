using Framework;
using System;

namespace CrudGenerator.Core
{
    internal class CrudGeneratorApplicationMetadata : IApplicationMetadata
    {
        public string Name => "CrudGenerator";

        public string Description => "CrudGenerator";

        public Version Version => new Version(1, 0, 0);

        public string FriendlyVersion => Version.ToString();

        public DevelopmentStage LifeState => DevelopmentStage.PreAlpha;
    }
}
