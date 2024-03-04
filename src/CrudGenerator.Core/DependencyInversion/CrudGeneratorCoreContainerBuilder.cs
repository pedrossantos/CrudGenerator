using Database.DependencyInversion;
using DependencyInversion;
using System.Collections.Generic;

namespace CrudGenerator.Core.DependencyInversion
{
    public class CrudGeneratorCoreContainerBuilder : ImmutableContainerBuilder
    {
        public CrudGeneratorCoreContainerBuilder()
            : base(GetBuilders())
        {
        }

        private static IEnumerable<IContainerBuilder> GetBuilders()
        {
            yield return new DatabaseContainerBuilder();
            yield return new CrudGeneratorCoreContainterRegistrations();
        }
    }
}
