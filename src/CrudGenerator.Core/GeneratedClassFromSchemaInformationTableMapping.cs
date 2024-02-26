using Database.DataMapping;
using System.Collections.Generic;

namespace CrudGenerator.Core
{
    public class GeneratedClassFromSchemaInformationTableMapping : GeneratedClass
    {
        private readonly SchemaInformationTableMapping _schemaInformationTableMapping;

        public GeneratedClassFromSchemaInformationTableMapping()
            : base()
        {
        }

        public GeneratedClassFromSchemaInformationTableMapping(
            SchemaInformationTableMapping schemaInformationTableMapping,
            string classNameSpace,
            string className,
            List<string> dependencyNameSpaces,
            string classContent)
            : base(classNameSpace, className, dependencyNameSpaces, classContent)
        {
            _schemaInformationTableMapping = schemaInformationTableMapping;
        }

        public SchemaInformationTableMapping SchemaInformationTableMapping => _schemaInformationTableMapping;
    }
}