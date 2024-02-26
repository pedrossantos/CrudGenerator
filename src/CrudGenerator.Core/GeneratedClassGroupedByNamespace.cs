using Framework.Validation;
using System.Collections.Generic;
using System.Linq;

namespace CrudGenerator.Core
{
    public class GeneratedClassGroupedByNamespace
    {
        private string _nameSpace;
        private IEnumerable<GeneratedClass> _generatedClasses;

        public GeneratedClassGroupedByNamespace(
            IGrouping<string, GeneratedClass> groupedGeneratedClasses)
        {
            Requires.NotNull(groupedGeneratedClasses, nameof(groupedGeneratedClasses));

            _nameSpace = groupedGeneratedClasses.Key;
            _generatedClasses = groupedGeneratedClasses.ToArray();
        }

        public GeneratedClassGroupedByNamespace(
            string nameSpace,
            IEnumerable<GeneratedClass> generatedClasses)
        {
            Requires.NotNullOrEmpty(nameSpace, nameof(nameSpace));
            Requires.NotNull(generatedClasses, nameof(generatedClasses));

            _nameSpace = nameSpace;
            _generatedClasses = generatedClasses;
        }

        public string NameSpace => _nameSpace;

        public IEnumerable<GeneratedClass> GeneratedClasses => _generatedClasses;
    }
}
