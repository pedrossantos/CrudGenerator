using CrudGenerator.Core.DependencyInversion;
using DependencyInversion;
using View.Abstractions.Wpf.DependencyInversion;

namespace CrudGenerator.App.Wpf.DependencyInversion
{
    internal class CrudGeneratorContainerBuilder : ImmutableContainerBuilder
    {
        public CrudGeneratorContainerBuilder()
            : base(GetBuilders())
        {
        }

        private static IEnumerable<IContainerBuilder> GetBuilders()
        {
            yield return new CrudGeneratorCoreContainerBuilder();
            yield return new CrudGeneratorContainterRegistrations();
            yield return new WpfViewContainerBuilder();
        }
    }
}
