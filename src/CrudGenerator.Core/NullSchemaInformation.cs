using Database.DataAccess;
using Database.DataMapping;
using System.Data;

namespace CrudGenerator.Core
{
    public class NullSchemaInformation : SchemaInformationBase
    {
        public NullSchemaInformation()
        {
        }

        public override string ValidTableType => string.Empty;

        public new bool CanReadDatabaseSchema => false;

        public override bool IsColumnAutoIncremented(DataTable dataTable, DataRow dataRow)
        {
            return false;
        }

        public override object ConvertDefaultValueToFunction(DbType columnDbType, object defaultValue)
        {
            return null;
        }

        public override ForeignKeyValueColletion[] GetForeignKeyColumns(string tableName, string[] primaryKeys)
        {
            return new ForeignKeyValueColletion[0];
        }

        public override IndexInfo[] GetIndexedColumns(string tableName, string[] primaryKeys)
        {
            return new IndexInfo[0];
        }

        public override string[] GetPrimeryKeyColumns(string tableName)
        {
            return new string[0];
        }

        public override (DbType ColumnType, int? MaxLength) ResolveDbType(string databaseType, int? characterMaxLength)
        {
            return (DbType.Object, null);
        }
    }
}
