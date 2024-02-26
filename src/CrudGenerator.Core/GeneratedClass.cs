using Database.DataMapping;
using System.Collections.Generic;

namespace CrudGenerator.Core
{
    public class GeneratedClass
    {
        private readonly string _classNameSpace;
        private readonly string _className;
        private readonly List<string> _dependencyNameSpaces;
        private readonly string _classContent;

        public GeneratedClass()
        {
            _classNameSpace = string.Empty;
            _className = string.Empty;
            _dependencyNameSpaces = new List<string>();
            _classContent = string.Empty;
    }


        public GeneratedClass(
            string classNameSpace,
            string className,
            List<string> dependencyNameSpaces,
            string classContent)
        {
            _classNameSpace = classNameSpace;
            _className = className;
            _dependencyNameSpaces = dependencyNameSpaces;
            _classContent = classContent;
        }

        public string ClassNameSpace => _classNameSpace;

        public string ClassName => _className;

        public List<string> DependencyNameSpaces => _dependencyNameSpaces;

        public string ClassContent => _classContent;
    }
}
