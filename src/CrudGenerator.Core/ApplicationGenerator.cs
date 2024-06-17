using Database.DataAccess;
using Database.DataMapping;
using Framework.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;

namespace CrudGenerator.Core
{
    public class ApplicationGenerator : INotifyPropertyChanged, IDisposable
    {
        public const string ApplicationProjectSufix = "Application";
        public const string ServicesNamespaceSufix = "Services";
        public const string DataTransferObjectsNamespaceSufix = "DataTransferObjects";
        public const string DependencyInversionNamespaceSufix = "DependencyInversion";

        /// <summary>
        /// {field type},{field name}
        /// </summary>
        private const string fieldTemplate =
@"        private {0} _{1};";

        // {parameter type},{parameter name}
        private const string constructorParameterTemplate =
@"            {0} {1}";

        /// <summary>
        /// {parametername}
        /// </summary>
        private const string constructorParameterValidationTemplate =
@"            Framework.Validation.Requires.NotNull({0}, nameof({0}));";

        /// <summary>
        /// {field name}, {parametername}
        /// </summary>
        private const string fieldAssignmentTemplate =
@"            _{0} = {0};";

        /// <summary>
        /// {property type},{property name},{field name}
        /// </summary>
        private const string propertyTemplate = @"
        public {0} {1}
        {{
            get => _{2};
            set => _{2} = value;
        }}";

        /// <summary>
        /// {identity class name},  {equality confition}, {hashcode properties list}, { toString properties list}
        /// </summary>
        private const string equatableImplementationTemplate =
@"        public bool Equals({0} other)
        {{
            if (other is null)
            {{
                return false;
            }}

            if (ReferenceEquals(this, other))
            {{
                return true;
            }}

            return {1};
        }}

        public override bool Equals(object obj)
        {{
            return ReferenceEquals(this, obj) || (obj is {0} other && Equals(other));
        }}

        public override int GetHashCode()
        {{
            return HashCode.Combine({2});
        }}

        public override string ToString()
        {{
            return {3};
        }}";

        /// <summary>
        /// {identity class name}, {comparasion condition}
        /// </summary>
        private const string comparableImplementationTemplate =
@"        int IComparable.CompareTo(object obj)
        {{
            if (obj is null)
                return 1;

            return obj is {0} other
                ? CompareTo(other)
                : Throws.CompareTypeMismatch(typeof({0}));
        }}

        public int CompareTo({0} other)
        {{
            if (other is null)
                return 1;

            return {1};
        }}";

        /// <summary>
        /// {class name}, {class fields}, {constructor parameters}, {parameters validation}, {field assignments}, {class properties}, {equatable implementation}, {comparable implementation}
        /// </summary>
        private const string identityClassTemplate = @"
    public class {0}
        : IEquatable<{0}>, IComparable<{0}>, IComparable
    {{
{1}

        public {0}(
{2})
        {{
{3}{4}
        }}
{5}

        public static bool operator !=({0} left, {0} right) => !Equals(left, right);

        public static bool operator ==({0} left, {0} right) => Equals(left, right);

        public static bool operator <({0} left, {0} right) => (left?.CompareTo(right) ?? (right == null ? 0 : -1)) < 0;

        public static bool operator <=({0} left, {0} right) => (left?.CompareTo(right) ?? (right == null ? 0 : -1)) <= 0;

        public static bool operator >({0} left, {0} right) => (left?.CompareTo(right) ?? (right == null ? 0 : 1)) > 0;

        public static bool operator >=({0} left, {0} right) => (left?.CompareTo(right) ?? (right == null ? 0 : 1)) >= 0;
{6}
{7}
    }}";

        /// <summary>
        /// {adapter class name}
        /// {data transfer object type}
        /// {entity type}
        /// {data transfer object identity type}
        /// {identity type}
        /// {entity to data transfer object parameters}
        /// {identity to data transfer identity object parameters}
        /// {data transfer object parameters to entity}
        /// {data transfer identity object to identity  parameters}
        /// </summary>
        private const string dataAdapterExtension = @"
    public static class {0}
    {
        public static PageResponse<{1}> ConvertEntityPageResponseToDataTransferObjectPageResponse(this PageResponse<{2}> entitiesPageResponse)
        {
            return new PageResponse<{1}>(
                entitiesPageResponse.Items.ConvertEntityListToDataTransferObjectList(),
                entitiesPageResponse.PageNumber,
                entitiesPageResponse.IsLastPage);
        }

        public static ReadOnlyMemory<{1}> ConvertEntityListToDataTransferObjectList(this ReadOnlyMemory<{2}> entities)
        {
            return entities.ConvertAll(ConvertEntityToDataTransferObject);
        }

        public static {1} ConvertEntityToDataTransferObject(this {2} entity)
        {
            return new {1}(
{5});
        }

        public static {3} ConvertIdentityToIdDto(this {4} identity)
        {
            return new {3}(
{6});
        }

        public static ReadOnlyMemory<{2}> ConvertDataTransferObjectListToEntityList(this ReadOnlyMemory<{1}> dataTransferObjects)
        {
            return dataTransferObjects.ConvertAll(ConvertDataTransferObjectToEntity);
        }

        public static {2} ConvertDataTransferObjectToEntity(this {1} dataTransferObject)
        {
            return new {2}(
{7});
        }

        public static {4} ConvertIdDtoToIdentity(this {3} dataTransferIdentityObject)
        {
            return new {4}(
{8});
        }
    }";

        /// <summary>
        /// {entity type}, {identity type}, {identity filed type}, {identity generator type}, {create identity function}
        /// </summary>
        private const string noVersionedDatabaseRepositoryWithIdentityTemplate = @"
    public class {0}DatabaseRepository
        : NoVersionedDatabaseRepositoryWithIdentity<
            {0},
            {1},
            {2},
            {3}>
    {{
        public {0}DatabaseRepository(
            IDatabaseInt64IdentityGenerator identityGenerator,
            CreateTableIfNotExistsFactory<{0}> createTableFactory,
            CreateTableIndexesFactory<{0}> createTableIndexesFactory,
            ExistsFactory<{0}, {1}> existsFactory,
            InsertOneFactory<{0}> insertOneFactory,
            FindOneFactory<{0}, {1}> findOneFactory,
            FindOneJoinedFactory<{0}, {1}> findOneJoinedFactory,
            FindAllWithDynamicCriteriaFactory<{0}> findAllWithDynamicCriteriaFactory,
            FindAllJoinedWithDynamicCriteriaFactory<{0}> findAllJoinedWithDynamicCriteriaFactory,
            UpdateOneFactory<{0}> updateOneFactory,
            DeleteOneFactory<{0}, {1}> deleteOneFactory,
            CountAllFactory<{0}> countAllFactory,
            CountAllByDynamicCriteriaFactory<{0}> countAllByDynamicCriteriaFactory,
            DataModelToDatabaseAdapter<{0}> dataModelToDatabaseAdapter,
            ConnectionManager connectionManager)
            : base(
                  identityGenerator,
                  createTableFactory,
                  createTableIndexesFactory,
                  existsFactory,
                  insertOneFactory,
                  findOneFactory,
                  findAllWithDynamicCriteriaFactory,
                  updateOneFactory,
                  deleteOneFactory,
                  countAllFactory,
                  countAllByDynamicCriteriaFactory,
                  dataModelToDatabaseAdapter,
                  connectionManager)
        {{
            findOneQuery = findOneJoinedFactory.Create();
            findAllWithDynamicCriteriaQuery = findAllJoinedWithDynamicCriteriaFactory.Create();
        }}{4}
    }}";

        /// <summary>
        /// {entity type}, {identity type}
        /// </summary>
        private const string noVersionedDatabaseRepositoryTemplate = @"
    public class {0}DatabaseRepository
        : NoVersionedDatabaseRepository<
            {0},
            {1}>
    {{
        public {0}DatabaseRepository(
            CreateTableIfNotExistsFactory<{0}> createTableFactory,
            CreateTableIndexesFactory<{0}> createTableIndexesFactory,
            ExistsFactory<{0}, {1}> existsFactory,
            InsertOneFactory<{0}> insertOneFactory,
            FindOneFactory<{0}, {1}> findOneFactory,
            FindOneJoinedFactory<{0}, {1}> findOneJoinedFactory,
            FindAllWithDynamicCriteriaFactory<{0}> findAllWithDynamicCriteriaFactory,
            FindAllJoinedWithDynamicCriteriaFactory<{0}> findAllJoinedWithDynamicCriteriaFactory,
            UpdateOneFactory<{0}> updateOneFactory,
            DeleteOneFactory<{0}, {1}> deleteOneFactory,
            CountAllFactory<{0}> countAllFactory,
            CountAllByDynamicCriteriaFactory<{0}> countAllByDynamicCriteriaFactory,
            DataModelToDatabaseAdapter<{0}> dataModelToDatabaseAdapter,
            ConnectionManager connectionManager)
            : base(
                  createTableFactory,
                  createTableIndexesFactory,
                  existsFactory,
                  insertOneFactory,
                  findOneFactory,
                  findAllWithDynamicCriteriaFactory,
                  updateOneFactory,
                  deleteOneFactory,
                  countAllFactory,
                  countAllByDynamicCriteriaFactory,
                  dataModelToDatabaseAdapter,
                  connectionManager)
        {{
            findOneQuery = findOneJoinedFactory.Create();
            findAllWithDynamicCriteriaQuery = findAllJoinedWithDynamicCriteriaFactory.Create();
        }}
    }}";

        /// <summary>
        /// {identity type}, {identity field type}
        /// </summary>
        private const string createDatabaseIdentityFunctionTemplate = @"
        public async Task<{0}> CreateNewIdentity(IWorkContext context = null)
        {{
            {1} value = await IdentityGenerator
                .NextId()
                .NoSync();

            return new {0}(value);
        }}";

        /// <summary>
        /// {dependency identity class type}, {dependency entity class type}, {dependency identity parameter}, {identity type}, {identity field type}
        /// </summary>
        private const string createDatabaseIdentityWithScopeFunctionTemplate = @"
        public async Task<{3}> CreateNewIdentity(
            {0},
            IWorkContext context = null)
        {{
            {4} value = await IdentityGenerator
                .NextId(IdentityScope.FromType<{1}>({2}), context)
                .NoSync();

            return new {3}(value, {2});
        }}";

        /// <summary>
        /// {container builder class name}, {container registrations class name}
        /// </summary>
        private const string createContainerBuilderTemplate = @"
    public class {0} : ImmutableContainerBuilder
    {{
        public {0}(DbConnectionStringBuilder dbConnectionStringBuilder = null)
            : base(GetBuilders(dbConnectionStringBuilder))
        {{
        }}

        private static IEnumerable<IContainerBuilder> GetBuilders(DbConnectionStringBuilder dbConnectionStringBuilder = null)
        {{
            yield return new {1}(dbConnectionStringBuilder);
            yield return new DatabaseContainerBuilder();
            yield return new DomainContainerBuilder();
            yield return new DomainDatabaseContainerBuilder();
            
            if (dbConnectionStringBuilder != null)
            {{
                if (dbConnectionStringBuilder is MySqlConnectionStringBuilder)
                    yield return new MySqlContainerBuilder();
                else if (dbConnectionStringBuilder is SQLiteConnectionStringBuilder)
                    yield return new FileSqliteContainerBuilder();
                else if (dbConnectionStringBuilder is SqlConnectionStringBuilder)
                    yield return new SqlServerContainerBuilder();
                else if (dbConnectionStringBuilder is Npgsql.NpgsqlConnectionStringBuilder)
                    yield return new PostgreSqlContainerBuilder();
            }}
        }}
    }}";

        /// <summary>
        /// {container registrations class name}
        /// {table mappins reg's}
        /// {composed model tablemapping providers reg's}
        /// {model tablemapping providers reg's}
        /// {database adapters reg's}
        /// {repositories reg's}
        /// </summary>
        private const string createContainerRegistrationsTemplate = @"
    public class {0} : ImmutableContainerBuilder
    {{
        public {0}(DbConnectionStringBuilder dbConnectionStringBuilder = null)
            : base(GetRegistrations(dbConnectionStringBuilder))
        {{
        }}

        private static IEnumerable<ContainerRegistration> GetRegistrations(DbConnectionStringBuilder dbConnectionStringBuilder = null)
        {{
            return RegisterTableMappings()
                .Concat(RegisterModelTableMappingProviders())
                .Concat(RegisterDatabaseAdapters())
                .Concat(RegisterRepositories())
                .Concat(RegisterDatabaseDataAccessObjects(dbConnectionStringBuilder));
        }}

        private static IEnumerable<ContainerRegistration> RegisterTableMappings()
        {{
{1}
        }}

        private static IEnumerable<ContainerRegistration> RegisterModelTableMappingProviders()
        {{
{2}
{3}
        }}

        private static IEnumerable<ContainerRegistration> RegisterDatabaseAdapters()
        {{
{4}
        }}

        private static IEnumerable<ContainerRegistration> RegisterRepositories()
        {{
{5}
        }}

        private static IEnumerable<ContainerRegistration> RegisterDatabaseDataAccessObjects(DbConnectionStringBuilder dbConnectionStringBuilder = null)
        {{
            if (dbConnectionStringBuilder == null)
            {{
                #region MySql
                yield return CreateSingleton(new MySqlBuilderTemplate());
                yield return CreateSingleton(c => new MySqlNativeCommandBuilder(c.Resolve<MySqlBuilderTemplate>()));
                yield return CreateTransient<MySqlConnectionManagerBuilder>();
                yield return CreateSingleton<MySqlConnectionManager>();
                yield return CreateSingleton<MySqlSchemaInformation>();
                #endregion MySql

                #region PostgreSql
                yield return CreateSingleton(new PostgreSqlBuilderTemplate());
                yield return CreateSingleton(c => new PostgreSqlNativeCommandBuilder(c.Resolve<PostgreSqlBuilderTemplate>()));
                yield return CreateTransient<PostgreSqlConnectionManagerBuilder>();
                yield return CreateSingleton<PostgreSqlConnectionManager>();
                yield return CreateSingleton<PostgreSqlSchemaInformation>();
                #endregion PostgreSql

                #region Sqlite
                yield return CreateSingleton(new SqliteBuilderTemplate());
                yield return CreateSingleton(c => new SqliteNativeCommandBuilder(c.Resolve<SqliteBuilderTemplate>()));
                yield return CreateTransient<SqliteConnectionManagerBuilder>();
                yield return CreateSingleton<FileSqliteConnectionManager>();
                yield return CreateSingleton<SqliteSchemaInformation>();
                #endregion Sqlite

                #region SqlServer
                yield return CreateSingleton(new SqlServerBuilderTemplate());
                yield return CreateSingleton(c => new SqlServerNativeCommandBuilder(c.Resolve<SqlServerBuilderTemplate>()));
                yield return CreateTransient<SqlServerConnectionManagerBuilder>();
                yield return CreateSingleton<SqlServerConnectionManager>();
                yield return CreateSingleton<SqlServerSchemaInformation>();
                #endregion SqlServer
            }}
            else
            {{
                if (dbConnectionStringBuilder is MySqlConnectionStringBuilder mySqlConnectionStringBuilder)
                {{
                    yield return CreateSingleton(container => {{ return mySqlConnectionStringBuilder; }});
                    yield return CreateSingleton<MySqlSchemaInformation>();
                }}
                else if (dbConnectionStringBuilder is NpgsqlConnectionStringBuilder postgreSqlConnectionStringBuilder)
                {{
                    yield return CreateSingleton(container => {{ return postgreSqlConnectionStringBuilder; }});
                    yield return CreateSingleton<PostgreSqlSchemaInformation>();
                }}
                else if (dbConnectionStringBuilder is SQLiteConnectionStringBuilder sqliteConnectionStringBuilder)
                {{
                    yield return CreateSingleton(container => {{ return sqliteConnectionStringBuilder; }});
                    yield return CreateSingleton<SqliteSchemaInformation>();
                }}
                else if (dbConnectionStringBuilder is SqlConnectionStringBuilder sqlServerConnectionStringBuilder)
                {{
                    yield return CreateSingleton(container => {{ return sqlServerConnectionStringBuilder; }});
                    yield return CreateSingleton<SqlServerSchemaInformation>();
                }}
            }}
        }}
    }}";

        /// <summary>
        /// {namespace}, {class implementation}, {namespace dependencies}
        /// </summary>
        private const string nameSpaceTemplate =
@"{2}

namespace {0}
{{
{1}
}}
";

        private readonly ISchemaInformation _schemaInformation;

        private Dictionary<string, string> _namespacesMap;
        private Dictionary<string, List<string>> _namespaceDependenciesMap;

        private Dictionary<string, string> _identityClassNamesMap;
        private Dictionary<string, string> _generatedIdentityClassesClassesDictionary;

        /// <summary>
        /// (Key: IdentityClassName; Value: (Key: PropertyName, Value: PropertyType))
        /// </summary>
        private Dictionary<string, List<(string PropertyName, string PropertyType)>> _identityClassPropertiesMap;

        private Dictionary<string, string> _entityClassNamesMap;
        private Dictionary<string, string> _generatedEntityClassesDictionary;

        private Dictionary<string, string> _tableMappingClassNamesMap;
        private Dictionary<string, string> _generatedTableMappingClassesClassesDictionary;

        private Dictionary<string, string> _dataAdapterClassNamesMap;
        private Dictionary<string, string> _generatedDataAdapterClassesDictionary;

        private Dictionary<string, string> _databaseRepositoryClassNamesMap;
        private Dictionary<string, string> _generatedDatabaseRepositoryClassesDictionary;

        private Dictionary<string, string> _generatedDependencyInversionClassesDictionary;

        private List<GeneratedClass> _generatedClassesList;
        private ObservableCollection<GeneratedClass> _generatedClasses;

        public ApplicationGenerator(
            ModelGenerator modelGenerator,
            ISchemaInformation schemaInformation)
        {
            Requires.NotNull(schemaInformation, nameof(schemaInformation));

            _schemaInformation = schemaInformation;

            _namespacesMap = new Dictionary<string, string>();
            _namespaceDependenciesMap = new Dictionary<string, List<string>>();

            _identityClassNamesMap = new Dictionary<string, string>();
            _generatedIdentityClassesClassesDictionary = new Dictionary<string, string>();
            _identityClassPropertiesMap = new Dictionary<string, List<(string PropertyName, string PropertyType)>>();

            _entityClassNamesMap = new Dictionary<string, string>();
            _generatedEntityClassesDictionary = new Dictionary<string, string>();

            _tableMappingClassNamesMap = new Dictionary<string, string>();
            _generatedTableMappingClassesClassesDictionary = new Dictionary<string, string>();

            _dataAdapterClassNamesMap = new Dictionary<string, string>();
            _generatedDataAdapterClassesDictionary = new Dictionary<string, string>();

            _databaseRepositoryClassNamesMap = new Dictionary<string, string>();
            _generatedDatabaseRepositoryClassesDictionary = new Dictionary<string, string>();

            _generatedDependencyInversionClassesDictionary = new Dictionary<string, string>();

            _generatedClassesList = new List<GeneratedClass>();

            _generatedClasses = new ObservableCollection<GeneratedClass>();
            _generatedClasses.CollectionChanged += GeneratedClassesCollectionChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<GeneratedClass> GeneratedClasses => _generatedClasses;

        /// <summary>
        /// (Key: IdentityClassName; Value: (Key: PropertyName, Value: PropertyType))
        /// </summary>
        public IReadOnlyDictionary<string, List<(string PropertyName, string PropertyType)>> IdentityClassPropertiesMap { get; }

        public IReadOnlyDictionary<string, string> NamespacesMap { get; }

        public IReadOnlyDictionary<string, List<string>> NamespaceDependenciesMap { get; }

        public IReadOnlyDictionary<string, string> IdentityClassNamesMap { get; }

        public IReadOnlyDictionary<string, string> EntityClassNamesMap { get; }

        public IReadOnlyDictionary<string, string> TableMappingClassNamesMap { get; }

        public IReadOnlyDictionary<string, string> DatabaseAdapterClassNamesMap { get; }

        public IReadOnlyDictionary<string, string> DatabaseRepositoryClassNamesMap { get; }

        public void GenerateClasses(string projectName, string nameSpace)
        {
            Requires.NotNullOrEmpty(projectName, nameof(projectName));
            Requires.NotNullOrEmpty(nameSpace, nameof(nameSpace));

            Clear();

            GenerateIdentityClasses(nameSpace);
            GenerateEntityClasses(nameSpace);
            GenerateTableMappingClasses(nameSpace);
            GenerateDatabaseAdapterClasses(nameSpace);
            GenerateDatabaseRepositoryClasses(nameSpace);

            GenerateDependencyInversionClasses(projectName, nameSpace);

            foreach (GeneratedClass generatedClassFromSchemaInformationTableMapping in _generatedClassesList)
                _generatedClasses.Add(generatedClassFromSchemaInformationTableMapping);
        }

        public void Dispose()
        {
            Clear();
        }

        public void Clear()
        {
            _namespacesMap.Clear();
            _namespaceDependenciesMap.Clear();

            _identityClassNamesMap.Clear();
            _generatedIdentityClassesClassesDictionary.Clear();
            _identityClassPropertiesMap.Clear();

            _entityClassNamesMap.Clear();
            _generatedEntityClassesDictionary.Clear();

            _tableMappingClassNamesMap.Clear();
            _generatedTableMappingClassesClassesDictionary.Clear();

            _dataAdapterClassNamesMap.Clear();
            _generatedDataAdapterClassesDictionary.Clear();

            _databaseRepositoryClassNamesMap.Clear();
            _generatedDatabaseRepositoryClassesDictionary.Clear();

            _generatedClassesList.Clear();
            _generatedClasses.Clear();
        }

        private void GenerateIdentityClasses(string nameSpace)
        {
            foreach (KeyValuePair<string, SchemaInformationTableMapping> schemaInformationTableMapping in _schemaInformation.SchemaTableMappings.OrderBy(kp => kp.Value.Order))
            {
                string tableName = schemaInformationTableMapping.Key.ToLower();
                string identityClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}Identity";
                _identityClassNamesMap.Add(tableName, identityClassName);

                string classNameSpace =
                    !string.IsNullOrEmpty(nameSpace)
                    ? $"{nameSpace}.{ModelProjectSufix}.{ModelNamespaceSufix}.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}"
                    : $"{ModelProjectSufix}.{ModelNamespaceSufix}.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}";

                if (!_namespacesMap.ContainsKey(tableName.ToLower()))
                    _namespacesMap.Add(tableName.ToLower(), classNameSpace);

                List<ColumnInfo> primaryKeyColumns = new List<ColumnInfo>(schemaInformationTableMapping.Value.Columns.Where(col => col.IsPrimaryKey));
                int primaryKeyColumnsCount = primaryKeyColumns.Count;

                List<string> primaryKeysFields = new List<string>();
                List<string> primaryKeysFieldNames = new List<string>();
                List<string> constructorParameters = new List<string>();
                List<string> primaryKeysProperties = new List<string>();
                List<string> fieldValidations = new List<string>();
                List<string> fieldAssignments = new List<string>();

                while (primaryKeyColumns.Count > 0)
                {
                    ColumnInfo primaryKeyColumn = primaryKeyColumns[0];
                    primaryKeyColumns.RemoveAt(0);

                    string columnType = primaryKeyColumn.ResolveSystemTypeName();
                    ForeignKeyValueColletion foreignKeyValueColletion = schemaInformationTableMapping.Value.ForeignKeyValues.FirstOrDefault(fkv => fkv.ForeignKeyValueList.Where(fkm => fkm.ForeignColunmnsToReferencedColumns.Any(fctrc => fctrc.ForeignColumn == primaryKeyColumn.Name.ToLower())).Any());

                    if (foreignKeyValueColletion != null)
                    {
                        ForeignKeyValue foreignKeyValue =
                            foreignKeyValueColletion.ForeignKeyValueList.FirstOrDefault(fkv => fkv.ForeignColunmnsToReferencedColumns.Any(fctrc => fctrc.ForeignColumn == primaryKeyColumn.Name.ToLower()));

                        if (foreignKeyValue != null)
                        {
                            string referencedTableName = foreignKeyValue.ForeignColunmnsToReferencedColumns.Select(fctrc => fctrc.ReferencedTableName).FirstOrDefault()?.ToLower();
                            if (referencedTableName != null && _schemaInformation.SchemaTableMappings.ContainsKey(referencedTableName.ToLower()))
                            {
                                SchemaInformationTableMapping referencedSchemaInformationTableMapping = _schemaInformation.SchemaTableMappings[referencedTableName.ToLower()];

                                string referencedColumn = foreignKeyValue.ForeignColunmnsToReferencedColumns.FirstOrDefault(fctrc => fctrc.ForeignColumn.ToLower() == primaryKeyColumn.Name.ToLower()).ReferencedColumn.ToLower();
                                bool referencedColumnIsPrimaryKey = referencedSchemaInformationTableMapping.Columns.FirstOrDefault(col => col.Name.ToLower() == referencedColumn).IsPrimaryKey;
                                if (referencedColumnIsPrimaryKey)
                                {
                                    foreach (ForeignColunmnToReferencedColumn foreignColunmnToReferencedColumn in foreignKeyValue.ForeignColunmnsToReferencedColumns)
                                    {
                                        int columnIndexToRemove = primaryKeyColumns.FindIndex(col => col.Name.ToLower() == foreignColunmnToReferencedColumn.ForeignColumn.ToLower());
                                        if (columnIndexToRemove > -1)
                                            primaryKeyColumns.RemoveAt(columnIndexToRemove);
                                    }

                                    columnType = _identityClassNamesMap[referencedTableName];
                                    fieldValidations.Add(string.Format(constructorParameterValidationTemplate, primaryKeyColumn.Name));

                                    if (!_namespaceDependenciesMap.ContainsKey(tableName.ToLower()))
                                        _namespaceDependenciesMap.Add(tableName.ToLower(), new List<string>());

                                    if (!_namespaceDependenciesMap[tableName.ToLower()].Contains(referencedTableName.ToLower()))
                                        _namespaceDependenciesMap[tableName.ToLower()].Add(referencedTableName.ToLower());
                                }
                            }
                        }
                    }

                    primaryKeysFields.Add(
                        string.Format(
                            fieldTemplate,
                            columnType,
                            primaryKeyColumn.Name));

                    primaryKeysFieldNames.Add($"_{primaryKeyColumn.Name}");

                    constructorParameters.Add(
                        string.Format(
                            constructorParameterTemplate,
                            columnType,
                            primaryKeyColumn.Name));

                    primaryKeysProperties.Add(
                        string.Format(
                            propertyTemplate,
                            columnType,
                            CultureInfo.CurrentCulture.TextInfo.ToTitleCase(primaryKeyColumn.Name),
                            primaryKeyColumn.Name));

                    if (!_identityClassPropertiesMap.ContainsKey(identityClassName))
                        _identityClassPropertiesMap.Add(identityClassName, new List<(string PropertyName, string PropertyType)>());

                    _identityClassPropertiesMap[identityClassName].Add(($"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(primaryKeyColumn.Name)}", columnType));

                    fieldAssignments.Add(
                        string.Format(
                            fieldAssignmentTemplate,
                            primaryKeyColumn.Name));
                }

                string fieldsValidation = string.Empty;
                if (fieldValidations.Any(fv => fv != null))
                    fieldsValidation = string.Join("\r\n", fieldValidations.Where(fv => fv != null)) + "\r\n";

                string generatedEqualityImplementation =
                    string.Format(
                        equatableImplementationTemplate,
                        identityClassName,
                        string.Join(" && ", primaryKeysFieldNames.Select(pkfn => $"{pkfn} == other.{pkfn}")),
                        string.Join(", ", primaryKeysFieldNames),
                        $"$\"{string.Join(" - ", primaryKeysFieldNames.Select(pkfn => "{" + pkfn + "}"))}\"");


                string generatedComparableImplementation =
                    string.Format(
                        comparableImplementationTemplate,
                        identityClassName,
                        string.Join(" + ", primaryKeysFieldNames.Select(pkfn => $"{pkfn}.CompareTo(other.{pkfn})")));

                string generatedIdentityClass =
                    string.Format(
                        identityClassTemplate.TrimStart('\r', '\n'),
                        _identityClassNamesMap[tableName.ToLower()],
                        string.Join("\r\n", primaryKeysFields),
                        string.Join(",\r\n", constructorParameters),
                        fieldsValidation,
                        string.Join("\r\n", fieldAssignments),
                        string.Join("\r\n", primaryKeysProperties),
                        $"\r\n{generatedEqualityImplementation}",
                        $"\r\n{generatedComparableImplementation}");

                List<string> namespaceDependenciesList = new List<string>(
                    new string[]
                    {
                        "Framework",
                        "System",
                    });

                if (_namespaceDependenciesMap.ContainsKey(tableName.ToLower()))
                {
                    foreach (string namespaceDependency in _namespaceDependenciesMap[tableName.ToLower()])
                    {
                        string dependencyIdentityClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(namespaceDependency)}Identity";
                        string dependencyEntityClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(namespaceDependency)}Entity";

                        if (_namespacesMap.ContainsKey(namespaceDependency) &&
                            (generatedIdentityClass.Contains(dependencyIdentityClassName) || generatedIdentityClass.Contains(dependencyEntityClassName)))
                        {
                            namespaceDependenciesList.Add(_namespacesMap[namespaceDependency]);
                        }
                    }
                }

                _generatedIdentityClassesClassesDictionary.Add(
                    tableName,
                    string.Format(
                        nameSpaceTemplate,
                        classNameSpace,
                        generatedIdentityClass,
                        string.Join("\r\n", namespaceDependenciesList.OrderBy(ns => ns).Select(dependencyNamespace => $"using {dependencyNamespace};"))));

                _generatedClassesList.Add(new GeneratedClassFromSchemaInformationTableMapping(
                    schemaInformationTableMapping.Value,
                    classNameSpace,
                    identityClassName,
                    namespaceDependenciesList.OrderBy(ns => ns).ToList(),
                    _generatedIdentityClassesClassesDictionary[tableName.ToLower()]));
            }
        }

        private void GenerateEntityClasses(string nameSpace)
        {
            foreach (KeyValuePair<string, SchemaInformationTableMapping> schemaInformationTableMapping in _schemaInformation.SchemaTableMappings.OrderBy(kp => kp.Value.Order))
            {
                string tableName = schemaInformationTableMapping.Key.ToLower();
                string identityClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}Identity";
                string entityClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}Entity";
                _entityClassNamesMap.Add(tableName, entityClassName);

                string classNameSpace =
                    !string.IsNullOrEmpty(nameSpace)
                    ? $"{nameSpace}.{ModelProjectSufix}.{ModelNamespaceSufix}.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}"
                    : $"{ModelProjectSufix}.{ModelNamespaceSufix}.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}";

                if (!_namespacesMap.ContainsKey(tableName.ToLower()))
                    _namespacesMap.Add(tableName.ToLower(), classNameSpace);

                bool containsPrimaryKeyColumns = schemaInformationTableMapping.Value.Columns.Where(col => col.IsPrimaryKey).Any();
                List<ColumnInfo> entityColumns = new List<ColumnInfo>(
                    schemaInformationTableMapping.Value.Columns
                        .Where(col =>
                            !col.IsPrimaryKey ||
                            schemaInformationTableMapping.Value.ForeignKeyValues
                                .Any(fkvc => fkvc.ForeignKeyValueList
                                    .Any(fkv => fkv.ForeignColunmnsToReferencedColumns
                                        .Any(fkctrc => fkctrc.ForeignColumn.ToLower() == col.Name.ToLower())))));

                List<string> dependencyEntityFields = new List<string>();
                List<string> dependencyConstructorParameters = new List<string>();
                List<string> dependencyEntityProperties = new List<string>();
                List<string> dependencyFieldValidations = new List<string>();
                List<string> dependencyFieldAssignments = new List<string>();

                List<string> entityFields = new List<string>();
                List<string> constructorParameters = new List<string>();
                List<string> entityProperties = new List<string>();
                List<string> fieldValidations = new List<string>();
                List<string> fieldAssignments = new List<string>();

                if (containsPrimaryKeyColumns)
                {
                    string columnType = _identityClassNamesMap[tableName.ToLower()];

                    entityFields.Add(
                        string.Format(
                            fieldTemplate,
                            columnType,
                            $"{tableName}Identity"));

                    constructorParameters.Add(
                        string.Format(
                            constructorParameterTemplate,
                            columnType,
                            $"{tableName}Identity"));

                    entityProperties.Add(
                        string.Format(
                            propertyTemplate,
                            columnType,
                            CultureInfo.CurrentCulture.TextInfo.ToTitleCase($"{tableName}Identity"),
                            $"{tableName}Identity"));

                    fieldAssignments.Add(
                        string.Format(
                            fieldAssignmentTemplate,
                            $"{tableName}Identity"));
                }

                while (entityColumns.Count > 0)
                {
                    ColumnInfo entityColumn = entityColumns[0];

                    string columnType = entityColumn.ResolveSystemTypeName();
                    ForeignKeyValueColletion foreignKeyValueColletion = schemaInformationTableMapping.Value.ForeignKeyValues.FirstOrDefault(fkv => fkv.ForeignKeyValueList.Where(fkv => fkv.ForeignColunmnsToReferencedColumns.Any(fctrc => fctrc.ForeignColumn == entityColumn.Name.ToLower())).Any());

                    if (foreignKeyValueColletion != null)
                    {
                        ForeignKeyValue foreignKeyValue =
                            foreignKeyValueColletion.ForeignKeyValueList.FirstOrDefault(fkv => fkv.ForeignColunmnsToReferencedColumns.Any(fctrc => fctrc.ForeignColumn == entityColumn.Name.ToLower()));

                        if (foreignKeyValue != null)
                        {
                            string referencedTableName = foreignKeyValue.ForeignColunmnsToReferencedColumns.Select(fctrc => fctrc.ReferencedTableName).FirstOrDefault()?.ToLower();
                            if (referencedTableName != null && _schemaInformation.SchemaTableMappings.ContainsKey(referencedTableName.ToLower()))
                            {
                                SchemaInformationTableMapping referencedSchemaInformationTableMapping = _schemaInformation.SchemaTableMappings[referencedTableName.ToLower()];

                                bool foreignColumnIsPrimaryKey = foreignKeyValue.ForeignColunmnsToReferencedColumns.FirstOrDefault(fctrc => fctrc.IsPrimaryKey && fctrc.ForeignColumn.ToLower() == entityColumn.Name.ToLower()) != null;
                                string referencedColumn = foreignKeyValue.ForeignColunmnsToReferencedColumns.FirstOrDefault(fctrc => fctrc.ForeignColumn.ToLower() == entityColumn.Name.ToLower()).ReferencedColumn.ToLower();
                                foreach (ForeignColunmnToReferencedColumn foreignColunmnToReferencedColumn in foreignKeyValue.ForeignColunmnsToReferencedColumns)
                                {
                                    int columnIndexToRemove = entityColumns.FindIndex(col => col.Name.ToLower() == foreignColunmnToReferencedColumn.ForeignColumn.ToLower());
                                    if (columnIndexToRemove > -1)
                                        entityColumns.RemoveAt(columnIndexToRemove);
                                }

                                string fieldName = referencedTableName;
                                string propertyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(referencedTableName);

                                columnType = _entityClassNamesMap[referencedTableName];
                                dependencyFieldValidations.Add(string.Format(constructorParameterValidationTemplate, fieldName));

                                dependencyEntityFields.Add(
                                    string.Format(
                                        fieldTemplate,
                                        columnType,
                                        fieldName));

                                dependencyConstructorParameters.Add(
                                    string.Format(
                                        constructorParameterTemplate,
                                        columnType,
                                        fieldName));

                                dependencyEntityProperties.Add(
                                    string.Format(
                                        propertyTemplate,
                                        columnType,
                                        propertyName,
                                        fieldName));

                                dependencyFieldAssignments.Add(
                                    string.Format(
                                        fieldAssignmentTemplate,
                                        fieldName));

                                if (!_namespaceDependenciesMap.ContainsKey(tableName.ToLower()))
                                    _namespaceDependenciesMap.Add(tableName.ToLower(), new List<string>());

                                if (!_namespaceDependenciesMap[tableName.ToLower()].Contains(referencedTableName.ToLower()))
                                    _namespaceDependenciesMap[tableName.ToLower()].Add(referencedTableName.ToLower());

                                continue;
                            }
                        }
                    }

                    entityFields.Add(
                        string.Format(
                            fieldTemplate,
                            columnType,
                            entityColumn.Name));

                    constructorParameters.Add(
                        string.Format(
                            constructorParameterTemplate,
                            columnType,
                            entityColumn.Name));

                    entityProperties.Add(
                        string.Format(
                            propertyTemplate,
                            columnType,
                            CultureInfo.CurrentCulture.TextInfo.ToTitleCase(entityColumn.Name),
                            entityColumn.Name));

                    fieldAssignments.Add(
                        string.Format(
                            fieldAssignmentTemplate,
                            entityColumn.Name));

                    entityColumns.RemoveAt(0);
                }

                string fieldsValidation = string.Empty;
                if (fieldValidations.Any(fv => fv != null))
                    fieldsValidation = string.Join("\r\n", fieldValidations.Concat(dependencyFieldValidations).Where(fv => fv != null)) + "\r\n";

                string generatedEntityClass =
                    string.Format(
                        entityClassTemplate.TrimStart('\r', '\n'),
                        _entityClassNamesMap[tableName.ToLower()],
                        _identityClassNamesMap[tableName.ToLower()],
                        string.Join("\r\n", entityFields.Concat(dependencyEntityFields)),
                        constructorParameters[0].Trim(),
                        constructorParameters[0].TrimStart(' ').Split(' ')[1].Trim(' '),
                        string.Join(",\r\n", constructorParameters.Concat(dependencyConstructorParameters)),
                        fieldsValidation,
                        string.Join("\r\n", fieldAssignments.Concat(dependencyFieldAssignments)),
                        string.Join("\r\n", entityProperties.Concat(dependencyEntityProperties)),
                        string.Empty);

                List<string> namespaceDependenciesList = new List<string>(
                    new string[]
                    {
                        "Domain.Model",
                    });

                if (schemaInformationTableMapping.Value.Columns.Any(col => new DbType[] { DbType.Date, DbType.DateTime, DbType.DateTime2, DbType.DateTimeOffset, DbType.Time, DbType.Time }.Contains(col.DbType)))
                    namespaceDependenciesList.Add("System");

                if (_namespaceDependenciesMap.ContainsKey(tableName.ToLower()))
                {
                    foreach (string namespaceDependency in _namespaceDependenciesMap[tableName.ToLower()])
                    {
                        string dependencyIdentityClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(namespaceDependency)}Identity";
                        string dependencyEntityClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(namespaceDependency)}Entity";

                        if (_namespacesMap.ContainsKey(namespaceDependency) &&
                            (generatedEntityClass.Contains(dependencyIdentityClassName) || generatedEntityClass.Contains(dependencyEntityClassName)))
                        {
                            namespaceDependenciesList.Add(_namespacesMap[namespaceDependency]);
                        }
                    }
                }

                _generatedEntityClassesDictionary.Add(
                    tableName,
                    string.Format(
                        nameSpaceTemplate,
                        classNameSpace,
                        generatedEntityClass,
                        string.Join("\r\n", namespaceDependenciesList.OrderBy(ns => ns).Select(dependencyNamespace => $"using {dependencyNamespace};"))));

                _generatedClassesList.Add(new GeneratedClassFromSchemaInformationTableMapping(
                    schemaInformationTableMapping.Value,
                    classNameSpace,
                    entityClassName,
                    namespaceDependenciesList.OrderBy(ns => ns).ToList(),
                    _generatedEntityClassesDictionary[tableName.ToLower()]));
            }
        }

        private void GenerateDataAdapterClasses(string nameSpace)
        {
            foreach (KeyValuePair<string, SchemaInformationTableMapping> schemaInformationTableMappingPair in _schemaInformation.SchemaTableMappings.OrderBy(kp => kp.Value.Order))
            {
                SchemaInformationTableMapping schemaInformationTableMapping = schemaInformationTableMappingPair.Value;
                string tableName = schemaInformationTableMappingPair.Key.ToLower();
                string dataAdapterClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}DataAdapter";
                _dataAdapterClassNamesMap.Add(tableName, dataAdapterClassName);

                string classNameSpace =
                    !string.IsNullOrEmpty(nameSpace)
                    ? $"{nameSpace}.{ApplicationProjectSufix}.DataTransferObjects.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}"
                    : $"{ApplicationProjectSufix}.DataTransferObjects.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}";

                if (!_namespacesMap.ContainsKey(tableName.ToLower()))
                    _namespacesMap.Add(tableName.ToLower(), classNameSpace);

                List<string> getIdentityParameters = new List<string>();
                List<string> getIdentityParametersNames = new List<string>();
                List<string> getIdentityMappedDbDataReaderCalls = new List<string>();
                List<string> identityToDbParametersCalls = new List<string>();

                List<ColumnInfo> primaryKeyColumns = new List<ColumnInfo>(schemaInformationTableMapping.Columns.Where(col => col.IsPrimaryKey));
                int primaryKeyColumnsCount = primaryKeyColumns.Count;

                while (primaryKeyColumns.Count > 0)
                {
                    ColumnInfo primaryKeyColumn = primaryKeyColumns[0];
                    primaryKeyColumns.RemoveAt(0);

                    string columnType = primaryKeyColumn.ResolveSystemTypeName();
                    ForeignKeyValueColletion foreignKeyValueColletion = schemaInformationTableMapping.ForeignKeyValues.FirstOrDefault(fkv => fkv.ForeignKeyValueList.Where(fkv => fkv.ForeignColunmnsToReferencedColumns.Any(fctrc => fctrc.ForeignColumn == primaryKeyColumn.Name.ToLower())).Any());

                    if (foreignKeyValueColletion != null)
                    {
                        ForeignKeyValue foreignKeyValue =
                            foreignKeyValueColletion.ForeignKeyValueList.FirstOrDefault(kp => kp.ForeignColunmnsToReferencedColumns.Any(fctrc => fctrc.IsPrimaryKey && fctrc.ForeignColumn == primaryKeyColumn.Name.ToLower()));

                        if (foreignKeyValue != null)
                        {
                            string referencedTableName = foreignKeyValue.ForeignColunmnsToReferencedColumns.Select(fctrc => fctrc.ReferencedTableName).FirstOrDefault()?.ToLower();
                            if (referencedTableName != null && _schemaInformation.SchemaTableMappings.ContainsKey(referencedTableName.ToLower()))
                            {
                                SchemaInformationTableMapping referencedSchemaInformationTableMapping = _schemaInformation.SchemaTableMappings[referencedTableName.ToLower()];

                                string referencedColumn = foreignKeyValue.ForeignColunmnsToReferencedColumns.FirstOrDefault(fctrc => fctrc.ForeignColumn.ToLower() == primaryKeyColumn.Name.ToLower()).ReferencedColumn.ToLower();
                                bool referencedColumnIsPrimaryKey = referencedSchemaInformationTableMapping.Columns.FirstOrDefault(col => col.Name.ToLower() == referencedColumn).IsPrimaryKey;
                                if (referencedColumnIsPrimaryKey)
                                {
                                    columnType = _identityClassNamesMap[referencedTableName];
                                    if (!_namespaceDependenciesMap.ContainsKey(tableName.ToLower()))
                                        _namespaceDependenciesMap.Add(tableName.ToLower(), new List<string>());

                                    if (!_namespaceDependenciesMap[tableName.ToLower()].Contains(referencedTableName.ToLower()))
                                        _namespaceDependenciesMap[tableName.ToLower()].Add(referencedTableName.ToLower());

                                    foreach (ForeignColunmnToReferencedColumn foreignColunmnToReferencedColumn in foreignKeyValue.ForeignColunmnsToReferencedColumns)
                                    {
                                        getIdentityParameters.Add($"            string {foreignColunmnToReferencedColumn.ForeignColumn}");
                                        getIdentityParametersNames.Add($"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(foreignColunmnToReferencedColumn.ForeignColumn)}");

                                        int columnIndexToRemove = primaryKeyColumns.FindIndex(col => col.Name.ToLower() == foreignColunmnToReferencedColumn.ForeignColumn.ToLower());
                                        if (columnIndexToRemove > -1)
                                            primaryKeyColumns.RemoveAt(columnIndexToRemove);

                                        string parameterPath = $"value.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(primaryKeyColumn.Name)}";
                                        string foreignPropertyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(foreignColunmnToReferencedColumn.ForeignColumn);
                                        GetDependencyPropertyPath(ref parameterPath, columnType, foreignColunmnToReferencedColumn.ReferencedColumn);

                                        identityToDbParametersCalls.Add(
                                            $"            yield return new DbParameterDefinition("
                                            + $"{_tableMappingClassNamesMap[tableName.ToLower()]}._TableName, "
                                            + $"{_tableMappingClassNamesMap[tableName.ToLower()]}.{foreignPropertyName}, "
                                            + $"{parameterPath});");
                                    }

                                    IEnumerable<string> foreignKeyGetIdentityParameters = foreignKeyValue.ForeignColunmnsToReferencedColumns.Select(fctrc => fctrc.ForeignColumn);
                                    getIdentityMappedDbDataReaderCalls.Add($"                {_dataAdapterClassNamesMap[referencedTableName.ToLower()]}.GetIdentity(reader, {string.Join(", ", foreignKeyGetIdentityParameters)}, columnAlias)");

                                    continue;
                                }
                            }
                        }
                    }

                    getIdentityParameters.Add($"            string {primaryKeyColumn.Name}");
                    getIdentityParametersNames.Add($"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(primaryKeyColumn.Name)}");
                    getIdentityMappedDbDataReaderCalls.Add($"                {GenerateReaderGetFunction(primaryKeyColumn.DbType)}(ColumnAliasing.GetColumnAlias(columnAlias, {primaryKeyColumn.Name}))");

                    string propertyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(primaryKeyColumn.Name);
                    identityToDbParametersCalls.Add(
                        $"            yield return new DbParameterDefinition("
                        + $"{_tableMappingClassNamesMap[tableName.ToLower()]}._TableName, "
                        + $"{_tableMappingClassNamesMap[tableName.ToLower()]}.{propertyName}, "
                        + $"value.{propertyName});");
                }

                List<string> dependencyEntityMappedDbDataReaderCalls = new List<string>();
                List<string> entityMappedDbDataReaderCalls = new List<string>();
                List<string> entityToDbParametersCalls = new List<string>();

                List<ColumnInfo> otherColumns = new List<ColumnInfo>(
                    schemaInformationTableMapping.Columns
                        .Where(col =>
                            !col.IsPrimaryKey ||
                            schemaInformationTableMapping.ForeignKeyValues
                                .Any(fkvc => fkvc.ForeignKeyValueList
                                    .Any(fkv => fkv.ForeignColunmnsToReferencedColumns
                                        .Any(fkctrc => fkctrc.ForeignColumn.ToLower() == col.Name.ToLower())))));

                int otherColumnsCount = otherColumns.Count;

                while (otherColumns.Count > 0)
                {
                    ColumnInfo otherColumn = otherColumns[0];
                    otherColumns.RemoveAt(0);

                    // TODO: Corrigir para chaves estrangeiras que não compõem chave primária
                    string columnType = otherColumn.ResolveSystemTypeName();
                    ForeignKeyValueColletion foreignKeyValueColletion = schemaInformationTableMapping.ForeignKeyValues.FirstOrDefault(fkv => fkv.ForeignKeyValueList.Where(fkv => fkv.ForeignColunmnsToReferencedColumns.Any(fctrc => fctrc.ForeignColumn == otherColumn.Name.ToLower())).Any());

                    if (foreignKeyValueColletion != null)
                    {
                        ForeignKeyValue foreignKeyValue =
                            foreignKeyValueColletion.ForeignKeyValueList.FirstOrDefault(fkv => fkv.ForeignColunmnsToReferencedColumns.Any(fctrc => fctrc.ForeignColumn == otherColumn.Name.ToLower()));

                        if (foreignKeyValue != null)
                        {
                            string referencedTableName = foreignKeyValue.ForeignColunmnsToReferencedColumns.Select(fctrc => fctrc.ReferencedTableName).FirstOrDefault()?.ToLower();
                            if (referencedTableName != null && _schemaInformation.SchemaTableMappings.ContainsKey(referencedTableName.ToLower()))
                            {
                                SchemaInformationTableMapping referencedSchemaInformationTableMapping = _schemaInformation.SchemaTableMappings[referencedTableName.ToLower()];

                                string referencedColumn = foreignKeyValue.ForeignColunmnsToReferencedColumns.FirstOrDefault(fctrc => fctrc.ForeignColumn.ToLower() == otherColumn.Name.ToLower()).ReferencedColumn.ToLower();
                                //bool referencedColumnIsPrimaryKey = referencedSchemaInformationTableMapping.Columns.FirstOrDefault(col => col.Name.ToLower() == referencedColumn).IsPrimaryKey;
                                //if (referencedColumnIsPrimaryKey)
                                {
                                    columnType = _identityClassNamesMap[referencedTableName];
                                    if (!_namespaceDependenciesMap.ContainsKey(tableName.ToLower()))
                                        _namespaceDependenciesMap.Add(tableName.ToLower(), new List<string>());

                                    if (!_namespaceDependenciesMap[tableName.ToLower()].Contains(referencedTableName.ToLower()))
                                        _namespaceDependenciesMap[tableName.ToLower()].Add(referencedTableName.ToLower());

                                    foreach (ForeignColunmnToReferencedColumn foreignColunmnToReferencedColumn in foreignKeyValue.ForeignColunmnsToReferencedColumns)
                                    {
                                        int columnIndexToRemove = otherColumns.FindIndex(col => col.Name.ToLower() == foreignColunmnToReferencedColumn.ForeignColumn.ToLower());
                                        if (columnIndexToRemove > -1)
                                            otherColumns.RemoveAt(columnIndexToRemove);

                                        //string parameterPath = $"value.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(otherColumn.Name)}";
                                        //string foreignPropertyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(foreignColunmnToReferencedColumn.ForeignColumn);
                                        //GetDependencyPropertyPath(ref parameterPath, columnType, foreignColunmnToReferencedColumn.ReferencedColumn);

                                        //entityToDbParametersCalls.Add(
                                        //    $"            yield return new DbParameterDefinition("
                                        //    + $"{_tableMappingClassNamesMap[tableName.ToLower()]}._TableName, "
                                        //    + $"{_tableMappingClassNamesMap[tableName.ToLower()]}.{foreignPropertyName}, "
                                        //    + $"{parameterPath});");
                                    }

                                    IEnumerable<string> foreignKeyGetIdentityParameters = foreignKeyValue.ForeignColunmnsToReferencedColumns.Select(fctrc => fctrc.ForeignColumn);

                                    dependencyEntityMappedDbDataReaderCalls.Add($"                _{_dataAdapterClassNamesMap[foreignKeyValue.ForeignColunmnsToReferencedColumns.First().ReferencedTableName.ToLower()]}.FromDataReader(reader, {_tableMappingClassNamesMap[tableName]}._TableName)");

                                    continue;
                                }
                            }
                        }
                    }

                    entityMappedDbDataReaderCalls.Add($"                {GenerateReaderGetFunction(otherColumn.DbType)}(ColumnAliasing.GetColumnAlias(columnAlias, {_tableMappingClassNamesMap[tableName.ToLower()]}.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(otherColumn.Name)}))");

                    string propertyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(otherColumn.Name);
                    entityToDbParametersCalls.Add(
                        $"            yield return new DbParameterDefinition("
                        + $"{_tableMappingClassNamesMap[tableName.ToLower()]}._TableName, "
                        + $"{_tableMappingClassNamesMap[tableName.ToLower()]}.{propertyName}, "
                        + $"value.{propertyName});");
                }

                string generatedDatabaseAdapterClass =
                    string.Format(
                        dataAdapterExtension.TrimStart('\r', '\n'),
                        _dataAdapterClassNamesMap[tableName.ToLower()],
                        _entityClassNamesMap[tableName.ToLower()],
                        _identityClassNamesMap[tableName.ToLower()],
                        getIdentityParameters.Count == 0
                            ? string.Empty
                            : $"{string.Join(",\r\n", getIdentityParameters)},",
                        string.Join(",\r\n", getIdentityMappedDbDataReaderCalls),
                        getIdentityParametersNames.Count == 0
                            ? string.Empty
                            : $",\r\n{string.Join(",\r\n", getIdentityParametersNames.Select(pn => $"                    {_tableMappingClassNamesMap[tableName.ToLower()]}.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pn)}"))},\r\n                    columnAlias",
                        string.Join(",\r\n", entityMappedDbDataReaderCalls.Concat(dependencyEntityMappedDbDataReaderCalls)),
                        string.Join("\r\n", identityToDbParametersCalls),
                        string.Join("\r\n", entityToDbParametersCalls),
                        _tableMappingClassNamesMap[tableName],
                        !schemaInformationTableMapping.ForeignKeyValues.Any()
                            ? string.Empty
                            : $"        {string.Join("        \r\n", schemaInformationTableMapping.ForeignKeyValues.Select(fkv => $"private readonly {_dataAdapterClassNamesMap[fkv.ReferencedTable.ToLower()]} _{_dataAdapterClassNamesMap[fkv.ReferencedTable.ToLower()]};"))}\r\n\r\n",
                        !schemaInformationTableMapping.ForeignKeyValues.Any()
                            ? string.Empty
                            : $"\r\n            {string.Join("            \r\n", schemaInformationTableMapping.ForeignKeyValues.Select(fkv => $"_{_dataAdapterClassNamesMap[fkv.ReferencedTable.ToLower()]} = new {_dataAdapterClassNamesMap[fkv.ReferencedTable.ToLower()]}();"))}");

                List<string> namespaceDependenciesList = new List<string>(
                    new string[]
                    {
                        "Database.DataTransfer",
                        "Database.Sql",
                        "System.Collections.Generic",
                    });

                if (_namespaceDependenciesMap.ContainsKey(tableName.ToLower()))
                {
                    foreach (string namespaceDependency in _namespaceDependenciesMap[tableName.ToLower()])
                    {
                        string dependencyDatabaseAdapterClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(namespaceDependency)}DatabaseAdapter";
                        if (_namespacesMap.ContainsKey(namespaceDependency) && generatedDatabaseAdapterClass.Contains(dependencyDatabaseAdapterClassName))
                            namespaceDependenciesList.Add(_namespacesMap[namespaceDependency]);
                    }
                }

                _generatedDataAdapterClassesDictionary.Add(
                    tableName,
                    string.Format(
                        nameSpaceTemplate,
                        classNameSpace,
                        generatedDatabaseAdapterClass,
                        string.Join("\r\n", namespaceDependenciesList.OrderBy(ns => ns).Select(dependencyNamespace => $"using {dependencyNamespace};"))));

                _generatedClassesList.Add(new GeneratedClassFromSchemaInformationTableMapping(
                    schemaInformationTableMapping,
                    classNameSpace,
                    dataAdapterClassName,
                    namespaceDependenciesList.OrderBy(ns => ns).ToList(),
                    _generatedDataAdapterClassesDictionary[tableName.ToLower()]));
            }
        }

        private void GenerateDatabaseRepositoryClasses(string nameSpace)
        {
            foreach (KeyValuePair<string, SchemaInformationTableMapping> schemaInformationTableMappingPair in _schemaInformation.SchemaTableMappings.OrderBy(kp => kp.Value.Order))
            {
                SchemaInformationTableMapping schemaInformationTableMapping = schemaInformationTableMappingPair.Value;
                string tableName = schemaInformationTableMappingPair.Key.ToLower();
                string databaseRepositoryClassName = $"{_entityClassNamesMap[tableName]}DatabaseRepository";
                _databaseRepositoryClassNamesMap.Add(tableName, databaseRepositoryClassName);

                string classNameSpace =
                    !string.IsNullOrEmpty(nameSpace)
                    ? $"{nameSpace}.{ModelProjectSufix}.{ModelNamespaceSufix}.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}"
                    : $"{ModelProjectSufix}.{ModelNamespaceSufix}.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}";

                if (!_namespacesMap.ContainsKey(tableName.ToLower()))
                    _namespacesMap.Add(tableName.ToLower(), classNameSpace);

                string identityFieldType = string.Empty;
                string identityGeneratorType = string.Empty;
                string createNewIdentityParameterType = string.Empty;
                string createNewIdentityParameterName = string.Empty;
                string createNewIdentityScopeEntityName = string.Empty;

                List<ColumnInfo> primaryKeyColumns = new List<ColumnInfo>(schemaInformationTableMapping.Columns.Where(col => col.IsPrimaryKey));
                int primaryKeyColumnsCount = primaryKeyColumns.Count;

                if (primaryKeyColumns.Count <= 2)
                {
                    while (primaryKeyColumns.Count > 0)
                    {
                        ColumnInfo primaryKeyColumn = primaryKeyColumns[0];
                        primaryKeyColumns.RemoveAt(0);

                        string columnType = primaryKeyColumn.ResolveSystemTypeName();
                        ForeignKeyValueColletion foreignKeyValueColletion = schemaInformationTableMapping.ForeignKeyValues.FirstOrDefault(fkv => fkv.ForeignKeyValueList.Where(fkv => fkv.ForeignColunmnsToReferencedColumns.Any(fctrc => fctrc.ForeignColumn == primaryKeyColumn.Name.ToLower())).Any());

                        if (foreignKeyValueColletion == null)
                        {
                            if (IsColumnNumeric(primaryKeyColumn) && string.IsNullOrEmpty(identityFieldType))
                            {
                                identityFieldType = columnType;
                                identityGeneratorType = $"IDatabase{primaryKeyColumn.DbType}IdentityGenerator";
                            }
                        }
                        else if (foreignKeyValueColletion != null)
                        {
                            ForeignKeyValue foreignKeyValue =
                                foreignKeyValueColletion.ForeignKeyValueList.FirstOrDefault(fkv => fkv.ForeignColunmnsToReferencedColumns.Any(fctrc => fctrc.IsPrimaryKey && fctrc.ForeignColumn == primaryKeyColumn.Name.ToLower()));

                            if (foreignKeyValue != null)
                            {
                                string referencedTableName = foreignKeyValue.ForeignColunmnsToReferencedColumns.Select(fctrc => fctrc.ReferencedTableName).FirstOrDefault()?.ToLower();
                                if (referencedTableName != null && _schemaInformation.SchemaTableMappings.ContainsKey(referencedTableName.ToLower()))
                                {
                                    SchemaInformationTableMapping referencedSchemaInformationTableMapping = _schemaInformation.SchemaTableMappings[referencedTableName.ToLower()];

                                    string referencedColumn = foreignKeyValue.ForeignColunmnsToReferencedColumns.FirstOrDefault(fctrc => fctrc.ForeignColumn.ToLower() == primaryKeyColumn.Name.ToLower()).ReferencedColumn.ToLower();
                                    bool referencedColumnIsPrimaryKey = referencedSchemaInformationTableMapping.Columns.FirstOrDefault(col => col.Name.ToLower() == referencedColumn).IsPrimaryKey;
                                    if (referencedColumnIsPrimaryKey)
                                    {
                                        columnType = _identityClassNamesMap[referencedTableName];
                                        if (!_namespaceDependenciesMap.ContainsKey(tableName.ToLower()))
                                            _namespaceDependenciesMap.Add(tableName.ToLower(), new List<string>());

                                        if (!_namespaceDependenciesMap[tableName.ToLower()].Contains(referencedTableName.ToLower()))
                                            _namespaceDependenciesMap[tableName.ToLower()].Add(referencedTableName.ToLower());

                                        createNewIdentityParameterName = $"{columnType[0].ToString().ToLower()}{string.Join("", columnType.Skip(1))}";
                                        createNewIdentityParameterType = $"{columnType}";
                                        createNewIdentityScopeEntityName = _entityClassNamesMap[referencedTableName];
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // TODO: Corrigir quando chave primária é composta por mais de dois valores
                    //while (primaryKeyColumns.Count > 0)
                    //{
                    //    ColumnInfo primaryKeyColumn = primaryKeyColumns[0];
                    //    primaryKeyColumns.RemoveAt(0);

                    //    string columnType = primaryKeyColumn.ResolveSystemTypeName();
                    //    ForeignKeyValueColletion foreignKeyValueColletion = schemaInformationTableMapping.ForeignKeyValues.FirstOrDefault(fkv => fkv.ForeignKeyValueList.Where(fkv => fkv.ForeignColunmnsToReferencedColumns.Any(fctrc => fctrc.ForeignColumn == primaryKeyColumn.Name.ToLower())).Any());

                    //    if (foreignKeyValueColletion == null)
                    //    {
                    //        if (IsColumnNumeric(primaryKeyColumn) && string.IsNullOrEmpty(identityFieldType))
                    //        {
                    //            identityFieldType = columnType;
                    //            identityGeneratorType = $"IDatabase{primaryKeyColumn.DbType}IdentityGenerator";
                    //            break;
                    //        }
                    //    }
                    //}
                }

                string generatedDatabaseRepositoryClass =
                    string.IsNullOrEmpty(identityFieldType)
                    ? string.Format(
                        noVersionedDatabaseRepositoryTemplate.TrimStart('\r', '\n'),
                        _entityClassNamesMap[tableName.ToLower()],
                        _identityClassNamesMap[tableName.ToLower()])
                    : string.Format(
                        noVersionedDatabaseRepositoryWithIdentityTemplate.TrimStart('\r', '\n'),
                        _entityClassNamesMap[tableName.ToLower()],
                        _identityClassNamesMap[tableName.ToLower()],
                        identityFieldType,
                        identityGeneratorType,
                        string.IsNullOrEmpty(createNewIdentityScopeEntityName)
                            ? string.Format(
                                $"\r\n{createDatabaseIdentityFunctionTemplate}",
                                _identityClassNamesMap[tableName.ToLower()],
                                identityFieldType)
                            : string.Format(
                                $"\r\n{createDatabaseIdentityWithScopeFunctionTemplate}",
                                $"{createNewIdentityParameterType} {createNewIdentityParameterName}",
                                createNewIdentityScopeEntityName,
                                createNewIdentityParameterName,
                                _identityClassNamesMap[tableName.ToLower()],
                                identityFieldType));

                List<string> namespaceDependenciesList = new List<string>(
                    new string[]
                    {
                        "Database.DataAccess",
                        "Database.DataTransfer",
                        "Database.IdentityGeneration",
                        "Database.RepositoryServicing.Factories",
                        "Domain.Database",
                    });

                if (!string.IsNullOrEmpty(identityFieldType))
                {
                    namespaceDependenciesList.AddRange(
                        new string[]
                        {
                            "Domain.Model",
                            "Framework.Extensions",
                            "Framework.IdentityGeneration",
                            "System.Threading.Tasks",
                        });
                }

                if (_namespaceDependenciesMap.ContainsKey(tableName.ToLower()))
                {
                    foreach (string namespaceDependency in _namespaceDependenciesMap[tableName.ToLower()])
                    {
                        string dependencyIdentityClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(namespaceDependency)}Identity";
                        string dependencyEntityClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(namespaceDependency)}Entity";
                        if (_namespacesMap.ContainsKey(namespaceDependency) &&
                            (generatedDatabaseRepositoryClass.Contains(dependencyIdentityClassName) || generatedDatabaseRepositoryClass.Contains(dependencyEntityClassName)))
                            namespaceDependenciesList.Add(_namespacesMap[namespaceDependency]);
                    }
                }

                _generatedDatabaseRepositoryClassesDictionary.Add(
                    tableName,
                    string.Format(
                        nameSpaceTemplate,
                        classNameSpace,
                        generatedDatabaseRepositoryClass,
                        string.Join("\r\n", namespaceDependenciesList.OrderBy(ns => ns).Select(dependencyNamespace => $"using {dependencyNamespace};"))));

                _generatedClassesList.Add(new GeneratedClassFromSchemaInformationTableMapping(
                    schemaInformationTableMapping,
                    classNameSpace,
                    databaseRepositoryClassName,
                    namespaceDependenciesList.OrderBy(ns => ns).ToList(),
                    _generatedDatabaseRepositoryClassesDictionary[tableName.ToLower()]));
            }
        }

        private void GenerateDependencyInversionClasses(string projectName, string nameSpace)
        {
            string classNameSpace =
                !string.IsNullOrEmpty(nameSpace)
                ? $"{nameSpace}.{ModelProjectSufix}.{DependencyInversionNamespaceSufix}"
                : $"{ModelProjectSufix}.{DependencyInversionNamespaceSufix}";

            string formattedProjectName = projectName[0].ToString().ToUpper() + string.Join("", projectName.Skip(1));
            string containerBuilderClassName = $"{formattedProjectName}{ModelNamespaceSufix}ContainerBuilder";
            string containerRegistrationsClassName = $"{formattedProjectName}{ModelNamespaceSufix}ContainerRegistrations";

            List<string> namespaceDependenciesList = new List<string>(
                new string[]
                {
                    "Database.DependencyInversion",
                    "Database.MySql.DependencyInversion",
                    "Database.PostgreSql.DependencyInversion",
                    "Database.Sqlite.DependencyInversion",
                    "Database.SqlServer.DependencyInversion",
                    "DependencyInversion",
                    "Domain.Database.DependencyInversion",
                    "Domain.Infrastructure.DependencyInversion",
                    "MySql.Data.MySqlClient",
                    "System.Collections.Generic",
                    "System.Data.Common",
                    "System.Data.SqlClient",
                    "System.Data.SQLite",
                });

            string generatedContainerBuilderClass =
                string.Format(
                    nameSpaceTemplate,
                    classNameSpace,
                    string.Format(
                        createContainerBuilderTemplate,
                        containerBuilderClassName,
                        containerRegistrationsClassName),
                    string.Join("\r\n", namespaceDependenciesList.OrderBy(ns => ns).Select(dependencyNamespace => $"using {dependencyNamespace};")));

            _generatedClassesList.Add(new GeneratedClass(
                classNameSpace,
                containerBuilderClassName,
                namespaceDependenciesList.OrderBy(ns => ns).ToList(),
                generatedContainerBuilderClass));

            namespaceDependenciesList.Clear();
            namespaceDependenciesList.AddRange(
                new string[]
                {
                    "Database.DataTransfer",
                    "Database.MySql.DataAccess",
                    "Database.MySql.Sql",
                    "Database.PostgreSql.DataAccess",
                    "Database.PostgreSql.Sql",
                    "Database.Sqlite.DataAccess",
                    "Database.Sqlite.Sql",
                    "Database.SqlServer.DataAccess",
                    "Database.SqlServer.Sql",
                    "DependencyInversion",
                    "MySql.Data.MySqlClient",
                    "Npgsql",
                    "System.Collections.Generic",
                    "System.Data.Common",
                    "System.Data.SqlClient",
                    "System.Data.SQLite",
                    "System.Linq",
                });

            namespaceDependenciesList.AddRange(_namespacesMap.Values);

            string generatedContainerRegistrationsClass =
                string.Format(
                    nameSpaceTemplate,
                    classNameSpace,
                    string.Format(
                        createContainerRegistrationsTemplate,
                        containerRegistrationsClassName,
                        string.Join("\r\n", _tableMappingClassNamesMap.Values.Select(tableMapping => $"            yield return CreateSingleton<{tableMapping}>().WithAbstractions();")),
                        string.Join("\r\n", _tableMappingClassNamesMap.Select(tableMappingPair => $"            yield return CreateSingleton(c => new ComposedModelTableMappingProvider<{_entityClassNamesMap[tableMappingPair.Key.ToLower()]}, {tableMappingPair.Value}>(c.Resolve<TableMappingProvider>())).WithAbstractions();")),
                        string.Join("\r\n", _tableMappingClassNamesMap.Select(tableMappingPair => $"            yield return CreateSingleton<ModelTableMappingProvider<{_entityClassNamesMap[tableMappingPair.Key.ToLower()]}, {tableMappingPair.Value}>>().WithAbstractions();")),
                        string.Join("\r\n", _dataAdapterClassNamesMap.Values.Select(dataAdapter => $"            yield return CreateSingleton<{dataAdapter}>().WithAbstractions();")),
                        string.Join("\r\n", _databaseRepositoryClassNamesMap.Values.Select(repository => $"            yield return CreateSingleton<{repository}>().WithAbstractions();"))),
                    string.Join("\r\n", namespaceDependenciesList.OrderBy(ns => ns).Select(dependencyNamespace => $"using {dependencyNamespace};")));

            if (!_generatedDependencyInversionClassesDictionary.ContainsKey(containerRegistrationsClassName))
                _generatedDependencyInversionClassesDictionary.Add(containerRegistrationsClassName, generatedContainerBuilderClass);
            else
                _generatedDependencyInversionClassesDictionary[containerRegistrationsClassName] = generatedContainerBuilderClass;

            _generatedClassesList.Add(new GeneratedClass(
                classNameSpace,
                containerRegistrationsClassName,
                namespaceDependenciesList.OrderBy(ns => ns).ToList(),
                generatedContainerRegistrationsClass));
        }

        private string GenerateReaderGetFunction(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                case DbType.Xml:
                    return "reader.GetString";

                case DbType.Date:
                case DbType.DateTime2:
                    return "reader.GetDateTime";

                case DbType.Time:
                    return "reader.GetTimeSpan";

                case DbType.Binary:
                case DbType.Object:
                    return "reader.GetBytes";

                case DbType.Currency:
                    return "reader.GetDecimal";
                case DbType.Single:
                    return "reader.GetFloat";

                case DbType.VarNumeric:
                    return "reader.GetDouble";

                case DbType.String:
                case DbType.DateTime:
                case DbType.DateTimeOffset:
                case DbType.Boolean:
                case DbType.Byte:
                case DbType.Decimal:
                case DbType.Double:
                case DbType.Guid:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.SByte:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                    return $"reader.Get{dbType}";
                default:
                    throw new InvalidCastException();
            }
        }

        private bool IsColumnNumeric(ColumnInfo columnInfo)
        {
            switch (columnInfo.DbType)
            {

                case DbType.Byte:
                case DbType.Currency:
                case DbType.Decimal:
                case DbType.Double:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.SByte:
                case DbType.Single:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                case DbType.VarNumeric:
                    return true;

                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.Binary:
                case DbType.Boolean:
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                case DbType.Guid:
                case DbType.Object:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.Time:
                case DbType.Xml:
                default:
                    return false;
            }
        }

        private string GetDependencyPropertyPath(ref string parameterPath, string columnType, string referencedColumnName)
        {
            if (_identityClassPropertiesMap.ContainsKey(columnType))
            {
                (string PropertyName, string PropertyType)? dependencyPropertyTuple = _identityClassPropertiesMap[columnType]?.FirstOrDefault(p => p.PropertyName.ToLower() == referencedColumnName.ToLower());
                if (dependencyPropertyTuple == (null, null))
                    dependencyPropertyTuple = _identityClassPropertiesMap[columnType]?.FirstOrDefault();

                if (dependencyPropertyTuple != (null, null))
                {
                    parameterPath = $"{parameterPath}.{dependencyPropertyTuple.Value.PropertyName}";
                    string dependencyPropertyType = dependencyPropertyTuple.Value.PropertyType;
                    if (_identityClassPropertiesMap.ContainsKey(dependencyPropertyType))
                        GetDependencyPropertyPath(ref parameterPath, dependencyPropertyType, dependencyPropertyTuple.Value.PropertyName.ToLower());
                }
            }

            return parameterPath;
        }

        private void GeneratedClassesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GeneratedClasses)));
        }
    }
}
