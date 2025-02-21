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
        public const string EventsNamespaceSufix = "Events";
        public const string DependencyInversionNamespaceSufix = "DependencyInversion";

        private const string DataTransferObjectNameSufix = "Dto";
        private const string IdentityDataTransferObjectNameSufix = "IdentityDto";

        private const string InsertedEventNameSufix = "Inserted";
        private const string UpdatedEventNameSufix = "Updated";
        private const string DeletedEventNameSufix = "Deleted";
        private const string DataAdapterNameSufix = "DataAdapter";
        private const string CrudServiceNameSufix = "CrudService";

        /// <summary>
        /// {field type}
        /// {field name}
        /// </summary>
        private const string fieldTemplate =
@"        private {0} _{1};";

        // {parameter type
        // ,{parameter name}
        private const string constructorParameterTemplate =
@"            {0} {1}";

        /// <summary>
        /// {parametername}
        /// </summary>
        private const string constructorParameterValidationTemplate =
@"            Framework.Validation.Requires.NotNull({0}, nameof({0}));";

        /// <summary>
        /// {field name}
        /// {parametername}
        /// </summary>
        private const string fieldAssignmentTemplate =
@"            _{0} = {0};";

        /// <summary>
        /// {property type}
        /// {property name}
        /// {field name}
        /// {required attribute}
        /// {foreign group attribute}
        /// {custom display attribute}
        /// </summary>
        private const string propertyTemplate = @"{3}{4}{5}
        public {0} {1}
        {{
            get => _{2};
            set => PropertyChangedDispatcher.SetProperty(ref _{2}, value);
        }}";

        /// <summary>
        /// {property type}
        /// {property name}
        /// {field name}
        /// {identity data transfer object type}
        /// {required attribute}
        /// {foreign group attribute}
        /// {dependency attribute}
        /// {custom display attribute}
        /// </summary>
        private const string propertyTemplateWithDependency = @"{4}{5}{6}{7}
        public {0} {1}
        {{
            get 
            {{
                return _{2};
            }}

            set
            {{
                if (_{2} != value)
                {{
                    _{2} = value;
                    if (_{2} != null)
                        Id = new {3}(Identity, {2}.Id);

                    PropertyChangedDispatcher.Notify(nameof({1}));
                    PropertyChangedDispatcher.Notify(nameof(Id));
                }}
            }}
        }}";

        /// <summary>
        /// {property type}
        /// {property name}
        /// {foreign group field name}
        /// {identity data transfer object type}
        /// {foreign group attribute}
        /// {custom display attribute}
        /// </summary>
        private const string dependencyPropertyTemplate = @"
        [Required]
        {4}
        {5}
        public {0} {1}
        {{
            get 
            {{
                return _{2}.{1};
            }}

            set
            {{
                if (_{2}.{1} != value)
                {{
                    _{2}.{1} = value;
                    if (_{2}.{1} != null)
                        Id = new {3}(Identity, _{2}.Id);

                    PropertyChangedDispatcher.Notify(nameof({1}));
                    PropertyChangedDispatcher.Notify(nameof(Id));
                }}
            }}
        }}";

        /// <summary>
        /// {identity class name}
        /// {equality confition}
        /// {hashcode properties list}
        /// {toString properties list}
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
        /// {class name}
        /// {class fields}
        /// {constructor parameters}
        /// {parameters validation}
        /// {field assignments}
        /// {field assignments for non parameterless constructor}
        /// {serialization read properties assignments}
        /// {class properties}
        /// {equatable implementation}
        /// {serialization write properties}
        /// </summary>
        private const string identityClassTemplate = @"
    [Serializable]
    public class {0}
        : IEquatable<{0}>, IByTypeCloneable<{0}>, ISerializable
    {{
        private PropertyChangedDispatcher _propertyChangedDispatcher;
{1}

        public {0}(
{2})
            : this()
        {{
{3}{4}
        }}

        public {0}()
        {{
            _propertyChangedDispatcher = new PropertyChangedDispatcher(this);
{5}
        }}

        protected {0}(SerializationInfo info, StreamingContext context)
        {{
            Requires.NotNull(info, nameof(info));

{6}
        }}

        public PropertyChangedDispatcher PropertyChangedDispatcher => _propertyChangedDispatcher;
{7}

        public static bool operator !=({0} left, {0} right) => !Equals(left, right);

        public static bool operator ==({0} left, {0} right) => Equals(left, right);

{8}

        public {0} Clone()
        {{
            return this;
        }}

        object ICloneable.Clone()
        {{
            return Clone();
        }}

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {{
            GetObjectData(info, context);
        }}

        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {{
            Requires.NotNull(info, nameof(info));

{9}
        }}
    }}";

        /// <summary>
        /// {class name}
        /// {data transfer identity object class name}
        /// {identity field type}
        /// {class fields}
        /// {data transfer identity object parameter name}
        /// {constructor parameters}
        /// {field assignments}
        /// {field assignments for non parameterless constructor}
        /// {serialization read properties assignments}
        /// {class properties}
        /// {serialization write properties}
        /// </summary>
        private const string dataTransferObjectClassTemplate = @"
    [Serializable]
    public class {0} : 
        : DtoBase<{0}, {1}, {2}>, IEquatable<{0}>
    {{
{3}

        public {0}(
{5})
            : base({4})
        {{
{6}
        }}

        public {0}()
            : base()
        {{
{7}
        }}

        protected {0}(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {{
            Requires.NotNull(info, nameof(info));

{8}
        }}

        public override {2} Identity
        {{
            get => Id.Id;
            set => Id.Id = value;
        }}
{9}

        protected override void GetObjectData(SerializationInfo info, StreamingContext context)
        {{
            base.GetObjectData(info, context);

{10}
        }}
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
        private const string dataAdapterExtensionTemplate = @"
    public static class {0}
    {{
        public static PageResponse<{1}> ConvertEntityPageResponseToDataTransferObjectPageResponse(this PageResponse<{2}> entitiesPageResponse)
        {{
            return new PageResponse<{1}>(
                entitiesPageResponse.Items.ConvertEntityListToDataTransferObjectList(),
                entitiesPageResponse.PageNumber,
                entitiesPageResponse.IsLastPage);
        }}

        public static ReadOnlyMemory<{1}> ConvertEntityListToDataTransferObjectList(this ReadOnlyMemory<{2}> entities)
        {{
            return entities.ConvertAll(ConvertEntityToDataTransferObject);
        }}

        public static {1} ConvertEntityToDataTransferObject(this {2} entity)
        {{
            return new {1}(
{5});
        }}

        public static {3} ConvertIdentityToIdDto(this {4} identity)
        {{
            return new {3}(
{6});
        }}

        public static ReadOnlyMemory<{2}> ConvertDataTransferObjectListToEntityList(this ReadOnlyMemory<{1}> dataTransferObjects)
        {{
            return dataTransferObjects.ConvertAll(ConvertDataTransferObjectToEntity);
        }}

        public static {2} ConvertDataTransferObjectToEntity(this {1} dataTransferObject)
        {{
            return new {2}(
{7});
        }}

        public static {4} ConvertIdDtoToIdentity(this {3} dataTransferIdentityObject)
        {{
            return new {4}(
{8});
        }}
    }}";

        /// <summary>
        /// {crud service class name}
        /// {data transfer object type}
        /// {data transfer object identity type}
        /// {inserted event type}
        /// {updated event type}
        /// {deleted event type}
        /// {entity type}
        /// {identity type}
        /// {identity filed type}
        /// {identity generator type}
        /// {repository type}
        /// {repository field name}
        /// {data adapter type}
        /// </summary>
        private const string crudServiceTemplate = @"
    internal class {0}
        : CrudServiceBase<
            {1},
            {2},
            {3},
            {4},
            {5},
            {6},
            {7},
            {8},
            {9}>
    {{
        {10} _{11};

        public EmpresaCrudService(
            IBus bus,
            IEventPublisher eventPublisher,
            ILogStorage logStorage,
            {10} {11})
            : base(bus, eventPublisher, logStorage, {11})
        {{
            _{11} = {11};
        }}

        public override async Task GenerateIdentity({6} entity)
        {{
            entity.Identity = await _{11}.CreateNewIdentity();
        }}

        public override bool HasDefaultIdentity({6} entity)
        {{
            return entity.Identity.Id == 0;
        }}

        protected override {6}[] ConvertDtosToEntities({1}[] dtos)
        {{
            return {12}.ConvertDataTransferObjectListToEntityList(dtos).ToArray();
        }}

        protected override {6} ConvertDtoToEntity({1} dto)
        {{
            return {12}.ConvertDataTransferObjectToEntity(dto);
        }}

        protected override {1}[] ConvertEntitiesToDtos({6}[] entities)
        {{
            return {12}.ConvertEntityListToDataTransferObjectList(entities).ToArray();
        }}

        protected override {1} ConvertEntityToDto({6} entity)
        {{
            return {12}.ConvertEntityToDataTransferObject(entity);
        }}

        protected override {7} ConvertIdentityDtoToIdentity({2} identityDto)
        {{
            return {12}.ConvertIdDtoToIdentity(identityDto);
        }}

        protected override {2} ConvertIdentityToIdentiyDto({7} identity)
        {{
            return {12}.ConvertIdentityToIdDto(identity);
        }}

        protected override Task DataInserted({1} dto)
        {{
            EventPublisher.Publish(new {3}(dto));
            return Task.CompletedTask;
        }}

        protected override Task DataUpdated({1} dto)
        {{
            EventPublisher.Publish(new {4}(dto));
            return Task.CompletedTask;
        }}

        protected override Task DataDeleted({1} dto)
        {{
            EventPublisher.Publish(new {5}(dto));
            return Task.CompletedTask;
        }}

        protected override Task ErrorOnDeleteData({1} dto)
        {{
            // TODO: Adicionar ações quando ocorre erro ao tentar remover um dado
            return Task.CompletedTask;
        }}

        protected override Task ErrorOnInsertData({1} dto)
        {{
            // TODO: Adicionar ações quando ocorre erro ao tentar inserir um dado
            return Task.CompletedTask;
        }}

        protected override Task ErrorOnUpdateData({1} dto)
        {{
            // TODO: Adicionar ações quando ocorre erro ao tentar atualizar um dado
            return Task.CompletedTask;
        }}

        protected override Task ValidateDataBeforeDelete({1} dto)
        {{
            // TODO: Adicionar validações antes de realizar remoção de um dado
            return Task.CompletedTask;
        }}

        protected override Task ValidateDataBeforeInsert({1} dto)
        {{
            // TODO: Adicionar validações antes de realizar inserção de um dado
            return Task.CompletedTask;
        }}

        protected override Task ValidateDataBeforeUpdate({1} dto)
        {{
            // TODO: Adicionar validações antes de realizar atualização de um dado
            return Task.CompletedTask;
        }}
    }}";

        /// <summary>
        /// {crud service class name}
        /// {data transfer object type}
        /// {data transfer object identity type}
        /// {parent data transfer object type}
        /// {inserted event type}
        /// {updated event type}
        /// {deleted event type}
        /// {entity type}
        /// {identity type}
        /// {identity filed type}
        /// {identity generator type}
        /// {repository type}
        /// {repository field name}
        /// {data adapter type}
        /// {create new identity with scope parameter}
        /// {parent filter path}
        /// </summary>
        private const string crudServiceWithParentTemplate = @"
    internal class {0}
        : CrudServiceBaseWithParent<
            {1},
            {2},
            {3},
            {4},
            {5},
            {6},
            {7},
            {8},
            {9},
            {10}>
    {{
        {11} _{12};

        public EmpresaCrudService(
            IBus bus,
            IEventPublisher eventPublisher,
            ILogStorage logStorage,
            {11} {12})
            : base(bus, eventPublisher, logStorage, {12})
        {{
            _{12} = {12};
        }}

        public override async Task GenerateIdentity({7} entity)
        {{
            entity.Identity = await _{11}.CreateNewIdentity({14});
        }}

        public override bool HasDefaultIdentity({7} entity)
        {{
            return entity.Identity.Id == 0;
        }}

        protected override {7}[] ConvertDtosToEntities({1}[] dtos)
        {{
            return {13}.ConvertDataTransferObjectListToEntityList(dtos).ToArray();
        }}

        protected override {7} ConvertDtoToEntity({1} dto)
        {{
            return {13}.ConvertDataTransferObjectToEntity(dto);
        }}

        protected override {1}[] ConvertEntitiesToDtos({7}[] entities)
        {{
            return {13}.ConvertEntityListToDataTransferObjectList(entities).ToArray();
        }}

        protected override {1} ConvertEntityToDto({7} entity)
        {{
            return {13}.ConvertEntityToDataTransferObject(entity);
        }}

        protected override {7} ConvertIdentityDtoToIdentity({2} identityDto)
        {{
            return {13}.ConvertIdDtoToIdentity(identityDto);
        }}

        protected override {2} ConvertIdentityToIdentiyDto({7} identity)
        {{
            return {13}.ConvertIdentityToIdDto(identity);
        }}


        protected override MatchCriteriaCollection CreateParentIdentityFilter({3} parentDto)
        {{
            // TODO: Corrigir criação de MatchCriteriaOperator abaixo
            return new MatchCriteriaCollection(
                new MatchCriteriaOperator(
                    $""{15}"",
                    OperatorType.Equals,
                    parentDto.Id.Id));
        }}

        protected override Task DataInserted({1} dto)
        {{
            EventPublisher.Publish(new {4}(dto));
            return Task.CompletedTask;
        }}

        protected override Task DataUpdated({1} dto)
        {{
            EventPublisher.Publish(new {5}(dto));
            return Task.CompletedTask;
        }}

        protected override Task DataDeleted({1} dto)
        {{
            EventPublisher.Publish(new {6}(dto));
            return Task.CompletedTask;
        }}

        protected override Task ErrorOnDeleteData({1} dto)
        {{
            // TODO: Adicionar ações quando ocorre erro ao tentar remover um dado
            return Task.CompletedTask;
        }}

        protected override Task ErrorOnInsertData({1} dto)
        {{
            // TODO: Adicionar ações quando ocorre erro ao tentar inserir um dado
            return Task.CompletedTask;
        }}

        protected override Task ErrorOnUpdateData({1} dto)
        {{
            // TODO: Adicionar ações quando ocorre erro ao tentar atualizar um dado
            return Task.CompletedTask;
        }}

        protected override Task ValidateDataBeforeDelete({1} dto)
        {{
            // TODO: Adicionar validações antes de realizar remoção de um dado
            return Task.CompletedTask;
        }}

        protected override Task ValidateDataBeforeInsert({1} dto)
        {{
            // TODO: Adicionar validações antes de realizar inserção de um dado
            return Task.CompletedTask;
        }}

        protected override Task ValidateDataBeforeUpdate({1} dto)
        {{
            // TODO: Adicionar validações antes de realizar atualização de um dado
            return Task.CompletedTask;
        }}
    }}";

        /// <summary>
        /// {container builder class name}, {container registrations class name}
        /// </summary>
        private const string createContainerBuilderTemplate = @"
    public class {0} : ImmutableContainerBuilder
    {{
        public {0}()
            : base(GetBuilders())
        {{
        }}

        private static IEnumerable<IContainerBuilder> GetBuilders()
        {{
            yield return new CrudAbstractionsApplicationContainerBuilder();
            yield return new {1}();
        }}
    }}";

        /// <summary>
        /// {data transfer object type}
        /// {identity data transfer object type}
        /// {inserted event type}
        /// {updated event type}
        /// {deleted event type}
        /// {crud service type}
        /// </summary>
        private const string crudServiceRegistrationTemplate = @"
            yield return CreateSingleton<
                ICrudServiceBase<
                    {0},
                    {1},
                    {2},
                    {3},
                    {4}>,
                {5}>()
                .WithAbstractions()";

        /// <summary>
        /// {data transfer object type}
        /// {identity data transfer object type}
        /// {parent data transfer object type}
        /// {inserted event type}
        /// {updated event type}
        /// {deleted event type}
        /// {crud service type}
        /// </summary>
        private const string crudServiceWithParentRegistrationTemplate = @"
            yield return CreateSingleton<
                ICrudServiceBaseWithParent<
                    {0},
                    {1},
                    {2},
                    {3},
                    {4},
                    {5}>,
                {6}>()
                .WithAbstractions()";

        /// <summary>
        /// {container registrations class name}
        /// {crud services reg's}
        /// {dataModelToDatabaseAdapter reg's}
        /// </summary>
        private const string createContainerRegistrationsTemplate = @"
    public class {0} : ImmutableContainerBuilder
    {{
        public {0}()
            : base(GetRegistrations())
        {{
        }}

        private static IEnumerable<ContainerRegistration> GetRegistrations()
        {{
            return RegisterApplicationServices()
                .Concat(RegisterDataModelToDatabaseAdapters());
        }}

        private static IEnumerable<ContainerRegistration> RegisterApplicationServices()
        {{
            {1}

            yield return CreateSingleton<ApplicationLogStorage>().WithAbstractions();
            yield return CreateSingleton<IApplicationMetadata, TesteCrudGeneratorApplicationMetadata>();
        }}

        public static IEnumerable<ContainerRegistration> RegisterDataModelToDatabaseAdapters()
        {{
            {2}
        }}
    }}";

        /// <summary>
        /// {inserted event class name}
        /// {data transfer object type}
        /// {constructor parameter}
        /// </summary>
        private const string insertedEventClassTemplate = @"    [Serializable]
    public sealed class {0} : IDataTranserObjectInserted<{1}>, ISerializable
    {{
        public {0}({1} {2})
            : base({2})
        {{
        }}
    }}";

        /// <summary>
        /// {updated event class name}
        /// {data transfer object type}
        /// {constructor parameter}
        /// </summary>
        private const string updatedEventClassTemplate = @"    [Serializable]
    public sealed class {0} : IDataTranserObjectUpdated<{1}>, ISerializable
    {{
        public {0}({1} {2})
            : base({2})
        {{
        }}
    }}";

        /// <summary>
        /// {deleted event class name}
        /// {data transfer object type}
        /// {constructor parameter}
        /// </summary>
        private const string deletedEventClassTemplate = @"    [Serializable]
    public sealed class {0} : IDataTranserObjectDeleted<{1}>, ISerializable
    {{
        public {0}({1} {2})
            : base({2})
        {{
        }}
    }}";

        /// <summary>
        /// {application metadata class name}
        /// </summary>
        private const string applicationMetadataClassTemplate = @"    internal class {0} : IApplicationMetadata
    {{
        public string Name => ""{0}"";

        public string Description => ""{0}"";

        public Version Version => new Version(1, 0, 0);

        public string FriendlyVersion => Version.ToString();

        public DevelopmentStage LifeState => DevelopmentStage.PreAlpha;
    }}";

        /// <summary>
        /// {log storage class name}
        /// </summary>
        private const string logStorageClassTemplate = @"    public delegate void LogEventHandler(EventEntry log);

    public class {0} : ILogStorage
    {{
        public event LogEventHandler LogAddedEvent;

        public void AddEntry(EventEntry eventEntry)
        {{
            LogAddedEvent?.Invoke(eventEntry);
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

        private Dictionary<string, string> _dataTransferIdentityObjectClassNamesMap;
        private Dictionary<string, string> _generatedDataTransferIdentityObjectClassesClassesDictionary;

        /// <summary>
        /// (Key: IdentityClassName; Value: (Key: PropertyName, Value: PropertyType))
        /// </summary>
        private Dictionary<string, List<(string PropertyName, string PropertyType)>> _identityDataTransferObjectClassPropertiesMap;
        private Dictionary<string, List<string>> _identityDataTransferObjectClassPrimaryKeysPropertyMap;

        private Dictionary<string, string> _dataTransferObjectClassNamesMap;
        private Dictionary<string, string> _generatedDataTransferObjectClassesDictionary;

        private Dictionary<string, string> _dataAdapterClassNamesMap;
        private Dictionary<string, string> _generatedDataAdapterClassesDictionary;

        private Dictionary<string, string> _crudServiceClassNamesMap;
        private Dictionary<string, string> _generatedCrudServiceClassesDictionary;

        private Dictionary<string, string> _generatedDependencyInversionClassesDictionary;

        private List<GeneratedClass> _generatedClassesList;
        private ObservableCollection<GeneratedClass> _generatedClasses;

        public ApplicationGenerator(ISchemaInformation schemaInformation)
        {
            Requires.NotNull(schemaInformation, nameof(schemaInformation));

            _schemaInformation = schemaInformation;

            _namespacesMap = new Dictionary<string, string>();
            _namespaceDependenciesMap = new Dictionary<string, List<string>>();

            _dataTransferIdentityObjectClassNamesMap = new Dictionary<string, string>();
            _generatedDataTransferIdentityObjectClassesClassesDictionary = new Dictionary<string, string>();
            _identityDataTransferObjectClassPropertiesMap = new Dictionary<string, List<(string PropertyName, string PropertyType)>>();
            _identityDataTransferObjectClassPrimaryKeysPropertyMap = new Dictionary<string, List<string>>();

            _dataTransferObjectClassNamesMap = new Dictionary<string, string>();
            _generatedDataTransferObjectClassesDictionary = new Dictionary<string, string>();

            _dataAdapterClassNamesMap = new Dictionary<string, string>();
            _generatedDataAdapterClassesDictionary = new Dictionary<string, string>();

            _crudServiceClassNamesMap = new Dictionary<string, string>();
            _generatedCrudServiceClassesDictionary = new Dictionary<string, string>();

            _generatedDependencyInversionClassesDictionary = new Dictionary<string, string>();

            _generatedClassesList = new List<GeneratedClass>();

            _generatedClasses = new ObservableCollection<GeneratedClass>();
            _generatedClasses.CollectionChanged += GeneratedClassesCollectionChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<GeneratedClass> GeneratedClasses => _generatedClasses;

        public void GenerateClasses(string projectName, string nameSpace)
        {
            Requires.NotNullOrEmpty(projectName, nameof(projectName));
            Requires.NotNullOrEmpty(nameSpace, nameof(nameSpace));

            Clear();

            GenerateDataTransferIdentityObjectClasses(nameSpace);
            GenerateDataTransferObjectClasses(nameSpace);
            GenerateDataTransferObjectEventsClasses(nameSpace);
            GenerateDataAdapterClasses(projectName, nameSpace);
            GenerateCrudServiceClasses(projectName, nameSpace);

            GenerateDependencyInversionClasses(projectName, nameSpace);

            GenerateLogStorageAndAppMetadataClasse(projectName, nameSpace);

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

            _dataTransferIdentityObjectClassNamesMap.Clear();
            _generatedDataTransferIdentityObjectClassesClassesDictionary.Clear();
            _identityDataTransferObjectClassPropertiesMap.Clear();

            _dataTransferObjectClassNamesMap.Clear();
            _generatedDataTransferObjectClassesDictionary.Clear();

            _dataAdapterClassNamesMap.Clear();
            _generatedDataAdapterClassesDictionary.Clear();

            _generatedClassesList.Clear();
            _generatedClasses.Clear();
        }

        private void GenerateDataTransferIdentityObjectClasses(string nameSpace)
        {
            foreach (KeyValuePair<string, SchemaInformationTableMapping> schemaInformationTableMapping in _schemaInformation.SchemaTableMappings.OrderBy(kp => kp.Value.Order))
            {
                string tableName = schemaInformationTableMapping.Key.ToLower();
                string identityDtoClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}{IdentityDataTransferObjectNameSufix}";
                _dataTransferIdentityObjectClassNamesMap.Add(tableName, identityDtoClassName);

                string classNameSpace =
                    !string.IsNullOrEmpty(nameSpace)
                    ? $"{nameSpace}.{ApplicationProjectSufix}.{DataTransferObjectsNamespaceSufix}.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}"
                    : $"{ApplicationProjectSufix}.{DataTransferObjectsNamespaceSufix}.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}";

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
                List<string> nonParameterlessConstructorParameters = new List<string>();

                List<string> serializationInfoGetValueCalls = new List<string>();
                List<string> serializationInfoAddValueCalls = new List<string>();

                while (primaryKeyColumns.Count > 0)
                {
                    ColumnInfo primaryKeyColumn = primaryKeyColumns[0];
                    primaryKeyColumns.RemoveAt(0);

                    string columnType = primaryKeyColumn.ResolveSystemTypeName();
                    string propertyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(primaryKeyColumn.Name);

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

                                    columnType = _dataTransferIdentityObjectClassNamesMap[referencedTableName];
                                    fieldValidations.Add(string.Format(constructorParameterValidationTemplate, primaryKeyColumn.Name));

                                    if (!_namespaceDependenciesMap.ContainsKey(tableName.ToLower()))
                                        _namespaceDependenciesMap.Add(tableName.ToLower(), new List<string>());

                                    if (!_namespaceDependenciesMap[tableName.ToLower()].Contains(referencedTableName.ToLower()))
                                        _namespaceDependenciesMap[tableName.ToLower()].Add(referencedTableName.ToLower());

                                    nonParameterlessConstructorParameters.Add($"            {propertyName} = new {columnType}();");
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

                    if (!_identityDataTransferObjectClassPrimaryKeysPropertyMap.ContainsKey(identityDtoClassName))
                        _identityDataTransferObjectClassPrimaryKeysPropertyMap.Add(identityDtoClassName, new List<string>() { CultureInfo.CurrentCulture.TextInfo.ToTitleCase(primaryKeyColumn.Name) });
                    else
                        _identityDataTransferObjectClassPrimaryKeysPropertyMap[identityDtoClassName].Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(primaryKeyColumn.Name));

                    constructorParameters.Add(
                            string.Format(
                                constructorParameterTemplate,
                                columnType,
                                primaryKeyColumn.Name));

                    primaryKeysProperties.Add(
                        string.Format(
                            propertyTemplate,
                            columnType,
                            propertyName,
                            primaryKeyColumn.Name,
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty));

                    if (!_identityDataTransferObjectClassPropertiesMap.ContainsKey(identityDtoClassName))
                        _identityDataTransferObjectClassPropertiesMap.Add(identityDtoClassName, new List<(string PropertyName, string PropertyType)>());

                    _identityDataTransferObjectClassPropertiesMap[identityDtoClassName].Add(($"{propertyName}", columnType));

                    fieldAssignments.Add(
                        string.Format(
                            fieldAssignmentTemplate,
                            primaryKeyColumn.Name));

                    serializationInfoGetValueCalls.Add($"            {GenerateSerializationInfoGetFunction(primaryKeyColumn.DbType)}(nameof({propertyName}));");
                    serializationInfoAddValueCalls.Add($"            info.AddValue<{columnType}>(nameof({propertyName}), {propertyName});");
                }

                string fieldsValidation = string.Empty;
                if (fieldValidations.Any(fv => fv != null))
                    fieldsValidation = string.Join("\r\n", fieldValidations.Where(fv => fv != null)) + "\r\n";

                string generatedEqualityImplementation =
                    string.Format(
                        equatableImplementationTemplate,
                        identityDtoClassName,
                        string.Join(" && ", primaryKeysFieldNames.Select(pkfn => $"{pkfn} == other.{pkfn}")),
                        string.Join(", ", primaryKeysFieldNames),
                        $"$\"{string.Join(" - ", primaryKeysFieldNames.Select(pkfn => "{" + pkfn + "}"))}\"");


                string generatedIdentityClass =
                    string.Format(
                        identityClassTemplate.TrimStart('\r', '\n'),
                        _dataTransferIdentityObjectClassNamesMap[tableName.ToLower()],
                        string.Join("\r\n", primaryKeysFields),
                        string.Join(",\r\n", constructorParameters),
                        fieldsValidation,
                        string.Join("\r\n", fieldAssignments),
                        string.Join("\r\n", nonParameterlessConstructorParameters),
                        string.Join("\r\n", serializationInfoGetValueCalls),
                        string.Join("\r\n", primaryKeysProperties),
                        $"\r\n{generatedEqualityImplementation}",
                        string.Join("\r\n", serializationInfoAddValueCalls));

                List<string> namespaceDependenciesList = new List<string>(
                    new string[]
                    {
                        "Framework",
                        "Framework.NotifyChanges",
                        "Framework.Validation",
                        "System",
                        "System.Runtime.Serialization",
                    });

                if (_namespaceDependenciesMap.ContainsKey(tableName.ToLower()))
                {
                    foreach (string namespaceDependency in _namespaceDependenciesMap[tableName.ToLower()])
                    {
                        string dependencyIdentityClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(namespaceDependency)}{ModelGenerator.IdentityNameSufix}";
                        string dependencyEntityClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(namespaceDependency)}{ModelGenerator.EntityNameSufix}";

                        if (_namespacesMap.ContainsKey(namespaceDependency) &&
                            (generatedIdentityClass.Contains(dependencyIdentityClassName) || generatedIdentityClass.Contains(dependencyEntityClassName)))
                        {
                            namespaceDependenciesList.Add(_namespacesMap[namespaceDependency]);
                        }
                    }
                }

                _generatedDataTransferIdentityObjectClassesClassesDictionary.Add(
                    tableName,
                    string.Format(
                        nameSpaceTemplate,
                        classNameSpace,
                        generatedIdentityClass,
                        string.Join("\r\n", namespaceDependenciesList.OrderBy(ns => ns).Select(dependencyNamespace => $"using {dependencyNamespace};"))));

                _generatedClassesList.Add(new GeneratedClassFromSchemaInformationTableMapping(
                    schemaInformationTableMapping.Value,
                    classNameSpace,
                    identityDtoClassName,
                    namespaceDependenciesList.OrderBy(ns => ns).ToList(),
                    _generatedDataTransferIdentityObjectClassesClassesDictionary[tableName.ToLower()]));
            }
        }

        private void GenerateDataTransferObjectClasses(string nameSpace)
        {
            foreach (KeyValuePair<string, SchemaInformationTableMapping> schemaInformationTableMapping in _schemaInformation.SchemaTableMappings.OrderBy(kp => kp.Value.Order))
            {
                string tableName = schemaInformationTableMapping.Key.ToLower();
                string dataTransferIdentityObjectClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}{IdentityDataTransferObjectNameSufix}";
                string dataTransferObjectClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}{DataTransferObjectNameSufix}";
                _dataTransferObjectClassNamesMap.Add(tableName, dataTransferObjectClassName);

                string classNameSpace =
                    !string.IsNullOrEmpty(nameSpace)
                    ? $"{nameSpace}.{ApplicationProjectSufix}.{DataTransferObjectsNamespaceSufix}.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}"
                    : $"{ApplicationProjectSufix}.{DataTransferObjectsNamespaceSufix}.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}";

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

                List<string> dependencyDataTransferObjectFields = new List<string>();
                List<string> dependencyConstructorParameters = new List<string>();
                List<string> dependencyDataTransferObjectProperties = new List<string>();
                List<string> dependencyFieldValidations = new List<string>();
                List<string> dependencyFieldAssignments = new List<string>();

                List<string> dataTransferObjectFields = new List<string>();
                List<string> constructorParameters = new List<string>();
                List<string> dataTransferObjectProperties = new List<string>();
                List<string> fieldValidations = new List<string>();
                List<string> fieldAssignments = new List<string>();
                List<string> nonParameterlessConstructorParameters = new List<string>();

                List<string> serializationInfoGetValueCalls = new List<string>();
                List<string> serializationInfoAddValueCalls = new List<string>();

                if (containsPrimaryKeyColumns)
                {
                    string columnType = _dataTransferIdentityObjectClassNamesMap[tableName.ToLower()];
                    string propertyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase($"{tableName}Identity");

                    dataTransferObjectFields.Add(
                        string.Format(
                            fieldTemplate,
                            columnType,
                            $"{tableName}Identity"));

                    constructorParameters.Add(
                        string.Format(
                            constructorParameterTemplate,
                            columnType,
                            $"{tableName}Identity"));

                    dataTransferObjectProperties.Add(
                        string.Format(
                            propertyTemplate,
                            columnType,
                            propertyName,
                            $"{tableName}Identity",
                            string.Empty,
                            string.Empty,
                            string.Empty,
                            string.Empty));

                    fieldAssignments.Add(
                        string.Format(
                            fieldAssignmentTemplate,
                            $"{tableName}Identity"));

                    serializationInfoGetValueCalls.Add($"            info.GetNotNullValue<{columnType}>(nameof({propertyName}));");
                    serializationInfoAddValueCalls.Add($"            info.AddValue<{columnType}>(nameof({propertyName}), {propertyName});");
                }

                List<ColumnInfo> primaryKeyColumns = new List<ColumnInfo>(schemaInformationTableMapping.Value.Columns.Where(col => col.IsPrimaryKey));
                int primaryKeyColumnsCount = primaryKeyColumns.Count;

                while (entityColumns.Count > 0)
                {
                    ColumnInfo entityColumn = entityColumns[0];

                    string columnType = entityColumn.ResolveSystemTypeName();
                    string propertyName = string.Empty;

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

                                string fieldName = referencedTableName;

                                SchemaInformationTableMapping dependencySchemaTableMapping = null;
                                string dependencyPropertyName = string.Empty;
                                string foreignFieldName = referencedTableName;

                                // TODO: Criar recursão para realizar busca em profundidade e gerar todas as propriedades dependentes
                                foreach (ForeignColunmnToReferencedColumn foreignColunmnToReferencedColumn in foreignKeyValue.ForeignColunmnsToReferencedColumns)
                                {
                                    if (_schemaInformation.SchemaTableMappings.ContainsKey(referencedTableName))
                                    {
                                        dependencySchemaTableMapping = _schemaInformation.SchemaTableMappings[referencedTableName];
                                        ForeignColunmnToReferencedColumn dependencyForeignColunmnToReferencedColumn = dependencySchemaTableMapping.ForeignKeyValues
                                            .Select(fkvc => fkvc.ForeignKeyValueList
                                                .Where(fkv => fkv.ForeignColunmnsToReferencedColumns
                                                    .Where(fctrc => fctrc.ForeignColumn == foreignColunmnToReferencedColumn.ReferencedColumn) != null)
                                                    .FirstOrDefault())
                                            .FirstOrDefault()?
                                            .ForeignColunmnsToReferencedColumns
                                            .FirstOrDefault(fctrc => fctrc.ForeignColumn == foreignColunmnToReferencedColumn.ReferencedColumn);

                                        if (dependencyForeignColunmnToReferencedColumn != null)
                                        {
                                            string dependencyReferencedTable = dependencyForeignColunmnToReferencedColumn.ReferencedTableName;
                                            dependencyPropertyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dependencyReferencedTable);

                                            string dependencyColumnType = _dataTransferObjectClassNamesMap[dependencyReferencedTable];

                                            string teste = string.Format(
                                                    dependencyPropertyTemplate,
                                                    dependencyColumnType,
                                                    dependencyPropertyName,
                                                    foreignFieldName,
                                                    _dataTransferIdentityObjectClassNamesMap[tableName],
                                                    $"[ForeignGroup(typeof({_dataTransferObjectClassNamesMap[referencedTableName]}))]",
                                                    $"[CustomDisplay(\"{dependencyPropertyName}\", Order = {dataTransferObjectProperties.Count + dependencyDataTransferObjectProperties.Count}, GroupName = \"{tableName}\")])");

                                            dependencyDataTransferObjectProperties.Add(
                                                string.Format(
                                                    dependencyPropertyTemplate,
                                                    dependencyColumnType,
                                                    dependencyPropertyName,
                                                    foreignFieldName,
                                                    _dataTransferIdentityObjectClassNamesMap[tableName],
                                                    $"[ForeignGroup(typeof({_dataTransferObjectClassNamesMap[referencedTableName]}))]",
                                                    $"[CustomDisplay(\"{dependencyPropertyName}\", Order = {dataTransferObjectProperties.Count + dependencyDataTransferObjectProperties.Count}, GroupName = \"{tableName}\")])"));

                                            if (!_namespaceDependenciesMap.ContainsKey(tableName.ToLower()))
                                                _namespaceDependenciesMap.Add(tableName.ToLower(), new List<string>());

                                            if (!_namespaceDependenciesMap[tableName.ToLower()].Contains(dependencyReferencedTable.ToLower()))
                                                _namespaceDependenciesMap[tableName.ToLower()].Add(dependencyReferencedTable.ToLower());
                                        }
                                    }

                                    int columnIndexToRemove = entityColumns.FindIndex(col => col.Name.ToLower() == foreignColunmnToReferencedColumn.ForeignColumn.ToLower());
                                    if (columnIndexToRemove > -1)
                                        entityColumns.RemoveAt(columnIndexToRemove);
                                }

                                propertyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(referencedTableName);

                                columnType = _dataTransferObjectClassNamesMap[referencedTableName];
                                dependencyFieldValidations.Add(string.Format(constructorParameterValidationTemplate, fieldName));

                                dependencyDataTransferObjectFields.Add(
                                    string.Format(
                                        fieldTemplate,
                                        columnType,
                                        fieldName));

                                dependencyConstructorParameters.Add(
                                    string.Format(
                                        constructorParameterTemplate,
                                        columnType,
                                        fieldName));

                                if (string.IsNullOrEmpty(dependencyPropertyName))
                                {
                                    dependencyDataTransferObjectProperties.Add(
                                        string.Format(
                                            propertyTemplate,
                                            columnType,
                                            propertyName,
                                            fieldName,
                                            foreignColumnIsPrimaryKey ? "\r\n        [Required]" : string.Empty,
                                            $"\r\n        [ForeignGroup(typeof({_dataTransferObjectClassNamesMap[referencedTableName]}))]",
                                            $"\r\n        [CustomDisplay(\"{propertyName}\", Order = {dataTransferObjectProperties.Count + dependencyDataTransferObjectProperties.Count}, GroupName = \"{tableName}\")])"));
                                }
                                else
                                {
                                    dependencyDataTransferObjectProperties.Add(
                                        string.Format(
                                            propertyTemplateWithDependency,
                                            columnType,
                                            propertyName,
                                            fieldName,
                                            _dataTransferIdentityObjectClassNamesMap[tableName],
                                            foreignColumnIsPrimaryKey ? "\r\n        [Required]" : string.Empty,
                                            $"\r\n        [ForeignGroup(typeof({_dataTransferObjectClassNamesMap[referencedTableName]}))]",
                                            !string.IsNullOrEmpty(dependencyPropertyName) ? $"\r\n        [Dependency(typeof({_dataTransferObjectClassNamesMap[dependencyPropertyName.ToLower()]}), nameof({dependencyPropertyName}))]" : string.Empty,
                                            $"\r\n        [CustomDisplay(\"{propertyName}\", Order = {dataTransferObjectProperties.Count + dependencyDataTransferObjectProperties.Count}, GroupName = \"{tableName}\")])"));
                                }

                                dependencyFieldAssignments.Add(
                                    string.Format(
                                        fieldAssignmentTemplate,
                                        fieldName));

                                if (!_namespaceDependenciesMap.ContainsKey(tableName.ToLower()))
                                    _namespaceDependenciesMap.Add(tableName.ToLower(), new List<string>());

                                if (!_namespaceDependenciesMap[tableName.ToLower()].Contains(referencedTableName.ToLower()))
                                    _namespaceDependenciesMap[tableName.ToLower()].Add(referencedTableName.ToLower());

                                nonParameterlessConstructorParameters.Add($"            {propertyName} = new {columnType}();");

                                continue;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(propertyName))
                        propertyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(entityColumn.Name);

                    dataTransferObjectFields.Add(
                        string.Format(
                            fieldTemplate,
                            columnType,
                            entityColumn.Name));

                    constructorParameters.Add(
                        string.Format(
                            constructorParameterTemplate,
                            columnType,
                            entityColumn.Name));

                    dataTransferObjectProperties.Add(
                        string.Format(
                            propertyTemplate,
                            columnType,
                            propertyName,
                            entityColumn.Name,
                            entityColumn.EnforceNotNull ? "\r\n        [Required]" : string.Empty,
                            string.Empty,
                            $"\r\n        [CustomDisplay(\"{propertyName}\", Order = {dataTransferObjectProperties.Count + dependencyDataTransferObjectProperties.Count}, GroupName = \"{tableName}\")])"));

                    fieldAssignments.Add(
                        string.Format(
                            fieldAssignmentTemplate,
                            entityColumn.Name));

                    serializationInfoGetValueCalls.Add($"            info.GetNotNullValue<{columnType}>(nameof({propertyName}));");
                    serializationInfoAddValueCalls.Add($"            info.AddValue<{columnType}>(nameof({propertyName}), {propertyName});");

                    entityColumns.RemoveAt(0);
                }

                string fieldsValidation = string.Empty;
                if (fieldValidations.Any(fv => fv != null))
                    fieldsValidation = string.Join("\r\n", fieldValidations.Concat(dependencyFieldValidations).Where(fv => fv != null)) + "\r\n";

                /// 0  - {class name}
                /// 1  - {data transfer identity object class name}
                /// 2  - {identity field type}
                /// 3  - {class fields}
                /// 4  - {data transfer identity object parameter name}
                /// 5  - {constructor parameters}
                /// 6  - {field assignments}
                /// 7  - {field assignments for non parameterless constructor}
                /// 8  - {serialization read properties assignments}
                /// 9  - {class properties}
                /// 10 - {serialization write properties}

                string generatedEntityClass =
                    string.Format(
                        dataTransferObjectClassTemplate.TrimStart('\r', '\n'),
                        _dataTransferObjectClassNamesMap[tableName.ToLower()], // 0
                        _dataTransferIdentityObjectClassNamesMap[tableName.ToLower()],// 1
                        _identityDataTransferObjectClassPropertiesMap[_dataTransferIdentityObjectClassNamesMap[tableName.ToLower()]].First().PropertyType,// 2
                        string.Join("\r\n", dataTransferObjectFields.Skip(1).Concat(dependencyDataTransferObjectFields)),// 3
                        constructorParameters[0].Trim().Split(' ')[1].Trim(),// 4
                        string.Join(",\r\n", constructorParameters.Concat(dependencyConstructorParameters)),// 5
                                                                                                            //fieldsValidation,
                        string.Join("\r\n", fieldAssignments.Skip(1).Concat(dependencyFieldAssignments)),// 6
                        string.Join("\r\n", nonParameterlessConstructorParameters.Skip(1)),// 7
                        string.Join("\r\n", serializationInfoGetValueCalls.Skip(1)),// 8
                        string.Join("\r\n", dependencyDataTransferObjectProperties.Concat(dataTransferObjectProperties.Skip(1))),// 9
                        string.Join("\r\n", serializationInfoAddValueCalls.Skip(1)));// 10

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
                        string dependencyIdentityClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(namespaceDependency)}{ModelGenerator.IdentityNameSufix}";
                        string dependencyEntityClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(namespaceDependency)}{ModelGenerator.EntityNameSufix}";

                        if (_namespacesMap.ContainsKey(namespaceDependency) &&
                            (generatedEntityClass.Contains(dependencyIdentityClassName) || generatedEntityClass.Contains(dependencyEntityClassName)))
                        {
                            namespaceDependenciesList.Add(_namespacesMap[namespaceDependency]);
                        }
                    }
                }

                _generatedDataTransferObjectClassesDictionary.Add(
                    tableName,
                    string.Format(
                        nameSpaceTemplate,
                        classNameSpace,
                        generatedEntityClass,
                        string.Join("\r\n", namespaceDependenciesList.OrderBy(ns => ns).Select(dependencyNamespace => $"using {dependencyNamespace};"))));

                _generatedClassesList.Add(new GeneratedClassFromSchemaInformationTableMapping(
                    schemaInformationTableMapping.Value,
                    classNameSpace,
                    dataTransferObjectClassName,
                    namespaceDependenciesList.OrderBy(ns => ns).ToList(),
                    _generatedDataTransferObjectClassesDictionary[tableName.ToLower()]));
            }
        }

        private void GenerateDataAdapterClasses(string projectName, string nameSpace)
        {
            foreach (KeyValuePair<string, SchemaInformationTableMapping> schemaInformationTableMapping in _schemaInformation.SchemaTableMappings.OrderBy(kp => kp.Value.Order))
            {
                string tableName = schemaInformationTableMapping.Key.ToLower();
                string dataAdapterClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}{DataAdapterNameSufix}";
                _dataAdapterClassNamesMap.Add(tableName, dataAdapterClassName);

                string identityClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}{ModelGenerator.IdentityNameSufix}";
                string entityClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}{ModelGenerator.EntityNameSufix}";
                string identityDataTransferObjectClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}{IdentityDataTransferObjectNameSufix}";
                string dataTransferObjectClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}{DataTransferObjectNameSufix}";

                string classNameSpace =
                    !string.IsNullOrEmpty(nameSpace)
                    ? $"{nameSpace}.{ApplicationProjectSufix}.{DataTransferObjectsNamespaceSufix}.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}"
                    : $"{ApplicationProjectSufix}.{DataTransferObjectsNamespaceSufix}.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}";

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

                List<string> entityToDataTransferObjectParameters = new List<string>();
                List<string> entityToDataTransferObjectForeignsParameters = new List<string>();
                List<string> dataTransferObjectToEntityParameters = new List<string>();
                List<string> dataTransferObjectToEntityForeignsParameters = new List<string>();
                List<string> identityToIdentityDataTransferObjectParameters = new List<string>();
                List<string> identityDataTransferObjectToIdentityParameters = new List<string>();

                List<ColumnInfo> primaryKeyColumns = new List<ColumnInfo>(schemaInformationTableMapping.Value.Columns.Where(col => col.IsPrimaryKey));
                int primaryKeyColumnsCount = primaryKeyColumns.Count;

                if (containsPrimaryKeyColumns)
                {
                    entityToDataTransferObjectParameters.Add($"                {tableName}Entity.Identity.ConvertIdentityToIdDto()");
                    dataTransferObjectToEntityParameters.Add($"                {tableName}Dto.Id.ConvertIdDtoToIdentity()");

                    identityToIdentityDataTransferObjectParameters.Add($"                identity.Id");
                    identityDataTransferObjectToIdentityParameters.Add($"                dataTransferIdentityObject.Id");

                    primaryKeyColumns.RemoveAt(0);
                    while (primaryKeyColumns.Count > 0)
                    {
                        ColumnInfo primaryKeyColumn = primaryKeyColumns[0];
                        primaryKeyColumns.RemoveAt(0);
                        string titleCasePrimaryKeyColumnName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(primaryKeyColumn.Name);

                        string columnType = primaryKeyColumn.ResolveSystemTypeName();
                        if (_identityDataTransferObjectClassPropertiesMap.ContainsKey(identityDataTransferObjectClassName))
                        {
                            ForeignKeyValueColletion foreignKeyValueColletion = schemaInformationTableMapping.Value.ForeignKeyValues
                                .FirstOrDefault(fkv => fkv.ForeignKeyValueList
                                    .Where(fkv => fkv.ForeignColunmnsToReferencedColumns
                                        .Any(fctrc => fctrc.ForeignColumn == primaryKeyColumn.Name.ToLower())).Any());

                            // Remove da lista primaryKeyColumns, colunas que compôem chave estrangeira referenciada pela chave primária da tabela atual
                            foreach (ForeignKeyValue foreignKeyValue in foreignKeyValueColletion.ForeignKeyValueList)
                            {
                                foreach (ForeignColunmnToReferencedColumn foreignColunmnToReferencedColumn in foreignKeyValue.ForeignColunmnsToReferencedColumns)
                                {
                                    ColumnInfo foreignKeyColumnInfo = primaryKeyColumns.FirstOrDefault(pkColumn => pkColumn.Name.ToUpper() == foreignColunmnToReferencedColumn.ForeignColumn.ToUpper());
                                    if (!string.IsNullOrEmpty(foreignKeyColumnInfo.Name))
                                        primaryKeyColumns.Remove(foreignKeyColumnInfo);
                                }
                            }

                            if (foreignKeyValueColletion != null)
                            {
                                ForeignKeyValue foreignKeyValue = foreignKeyValueColletion.ForeignKeyValueList
                                    .FirstOrDefault(fkv => fkv.ForeignColunmnsToReferencedColumns
                                        .Any(fctrc => fctrc.ForeignColumn == primaryKeyColumn.Name.ToLower()));

                                if (foreignKeyValue != null)
                                {
                                    string referencedTableName = foreignKeyValue.ForeignColunmnsToReferencedColumns.Select(fctrc => fctrc.ReferencedTableName).FirstOrDefault()?.ToLower();
                                    if (!string.IsNullOrEmpty(referencedTableName))
                                    {
                                        identityToIdentityDataTransferObjectParameters.Add($"                identity.{titleCasePrimaryKeyColumnName}.ConvertIdentityToIdDto()");
                                        identityDataTransferObjectToIdentityParameters.Add($"                dataTransferIdentityObject.{titleCasePrimaryKeyColumnName}.ConvertIdDtoToIdentity()");
                                    }
                                }
                            }
                        }
                    }
                }

                while (entityColumns.Count > 0)
                {
                    ColumnInfo entityColumn = entityColumns[0];
                    entityColumns.RemoveAt(0);

                    string columnType = entityColumn.ResolveSystemTypeName();
                    string titleCaseEntityColumnName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(entityColumn.Name); ;

                    ForeignKeyValueColletion foreignKeyValueColletion = schemaInformationTableMapping.Value.ForeignKeyValues
                        .FirstOrDefault(fkv => fkv.ForeignKeyValueList
                            .Where(fkv => fkv.ForeignColunmnsToReferencedColumns
                                .Any(fctrc => fctrc.ForeignColumn == entityColumn.Name.ToLower()))
                            .Any());

                    if (foreignKeyValueColletion != null)
                    {
                        foreach (ForeignKeyValue foreignKeyValue1 in foreignKeyValueColletion.ForeignKeyValueList)
                        {
                            foreach (ForeignColunmnToReferencedColumn foreignColunmnToReferencedColumn in foreignKeyValue1.ForeignColunmnsToReferencedColumns)
                            {
                                ColumnInfo foreignKeyColumnInfo = entityColumns.FirstOrDefault(pkColumn => pkColumn.Name.ToUpper() == foreignColunmnToReferencedColumn.ForeignColumn.ToUpper());
                                if (!string.IsNullOrEmpty(foreignKeyColumnInfo.Name))
                                    entityColumns.Remove(foreignKeyColumnInfo);
                            }
                        }

                        ForeignKeyValue foreignKeyValue =
                            foreignKeyValueColletion.ForeignKeyValueList.FirstOrDefault(fkv => fkv.ForeignColunmnsToReferencedColumns.Any(fctrc => fctrc.ForeignColumn == entityColumn.Name.ToLower()));

                        if (foreignKeyValue != null)
                        {
                            string referencedTableName = foreignKeyValue.ForeignColunmnsToReferencedColumns.Select(fctrc => fctrc.ReferencedTableName).FirstOrDefault()?.ToLower();
                            if (referencedTableName != null && _schemaInformation.SchemaTableMappings.ContainsKey(referencedTableName.ToLower()))
                            {
                                string titleCaseReferencedTable = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(referencedTableName);
                                entityToDataTransferObjectForeignsParameters.Add($"                {titleCaseReferencedTable}DataAdapter.ConvertEntityToDataTransferObject(entity.{titleCaseReferencedTable})");
                                dataTransferObjectToEntityForeignsParameters.Add($"                {titleCaseReferencedTable}DataAdapter.ConvertDataTransferObjectToEntity(dataTransferObject.{titleCaseReferencedTable})");

                                if (!_namespaceDependenciesMap.ContainsKey(tableName.ToLower()))
                                    _namespaceDependenciesMap.Add(tableName.ToLower(), new List<string>());

                                if (!_namespaceDependenciesMap[tableName.ToLower()].Contains(referencedTableName.ToLower()))
                                    _namespaceDependenciesMap[tableName.ToLower()].Add(referencedTableName.ToLower());
                            }
                        }
                    }
                    else
                    {
                        entityToDataTransferObjectParameters.Add($"                entity.{titleCaseEntityColumnName}");
                        dataTransferObjectToEntityParameters.Add($"                dataTransferObject.{titleCaseEntityColumnName}");
                    }
                }

                /// 0 - {adapter class name}
                /// 1 - {data transfer object type}
                /// 2 - {entity type}
                /// 3 - {data transfer object identity type}
                /// 4 - {identity type}
                /// 5 - {entity to data transfer object parameters}
                /// 6 - {identity to data transfer identity object parameters}
                /// 7 - {data transfer object parameters to entity}
                /// 8 - {data transfer identity object to identity  parameters}
                string generatedDataAdapterClass =
                    string.Format(
                        dataAdapterExtensionTemplate.TrimStart('\r', '\n'),
                        dataAdapterClassName, // 0
                        _dataTransferObjectClassNamesMap[tableName.ToLower()],// 1
                        entityClassName,// 2
                        _dataTransferIdentityObjectClassNamesMap[tableName],// 3
                        identityClassName,// 4
                        string.Join(",\r\n", entityToDataTransferObjectParameters.Concat(entityToDataTransferObjectForeignsParameters)),// 5
                        string.Join(",\r\n", identityToIdentityDataTransferObjectParameters),// 6
                        string.Join(",\r\n", dataTransferObjectToEntityParameters.Concat(dataTransferObjectToEntityForeignsParameters)),// 7
                        string.Join(",\r\n", identityDataTransferObjectToIdentityParameters));// 8

                List<string> namespaceDependenciesList = new List<string>(
                    new string[]
                    {
                        "Framework.Communication",
                        "Framework.Extensions",
                        "System",
                    });

                if (_namespaceDependenciesMap.ContainsKey(tableName.ToLower()))
                {
                    foreach (string namespaceDependency in _namespaceDependenciesMap[tableName.ToLower()])
                    {
                        namespaceDependenciesList.Add($"{projectName}.{ApplicationProjectSufix}.{DataTransferObjectsNamespaceSufix}.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(namespaceDependency)}");
                        namespaceDependenciesList.Add($"{projectName}.Model.Models.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(namespaceDependency)}");
                    }
                }

                _generatedDataAdapterClassesDictionary.Add(
                    tableName,
                    string.Format(
                        nameSpaceTemplate,
                        classNameSpace,
                        generatedDataAdapterClass,
                        string.Join("\r\n", namespaceDependenciesList.OrderBy(ns => ns).Select(dependencyNamespace => $"using {dependencyNamespace};"))));

                _generatedClassesList.Add(new GeneratedClassFromSchemaInformationTableMapping(
                    schemaInformationTableMapping.Value,
                    classNameSpace,
                    dataAdapterClassName,
                    namespaceDependenciesList.OrderBy(ns => ns).ToList(),
                    _generatedDataAdapterClassesDictionary[tableName.ToLower()]));
            }
        }

        private void GenerateCrudServiceClasses(string projectName, string nameSpace)
        {
            foreach (KeyValuePair<string, SchemaInformationTableMapping> schemaInformationTableMapping in _schemaInformation.SchemaTableMappings.OrderBy(kp => kp.Value.Order))
            {
                string tableName = schemaInformationTableMapping.Key.ToLower();
                string identityGeneratorType = string.Empty;
                string identityFieldType = string.Empty;
                string parentDataTransferObjectName = string.Empty;
                string parentIdentityDataTransferObjectName = string.Empty;

                string crudServiceClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}{CrudServiceNameSufix}";
                _crudServiceClassNamesMap.Add(tableName, crudServiceClassName);

                bool containsPrimaryKeyColumns = schemaInformationTableMapping.Value.Columns.Where(col => col.IsPrimaryKey).Any();
                List<ColumnInfo> entityColumns = new List<ColumnInfo>(
                    schemaInformationTableMapping.Value.Columns
                        .Where(col =>
                            !col.IsPrimaryKey ||
                            schemaInformationTableMapping.Value.ForeignKeyValues
                                .Any(fkvc => fkvc.ForeignKeyValueList
                                    .Any(fkv => fkv.ForeignColunmnsToReferencedColumns
                                        .Any(fkctrc => fkctrc.ForeignColumn.ToLower() == col.Name.ToLower())))));

                List<string> parentPrimaryKeys = new List<string>();

                List<ColumnInfo> primaryKeyColumns = new List<ColumnInfo>(schemaInformationTableMapping.Value.Columns.Where(col => col.IsPrimaryKey));
                int primaryKeyColumnsCount = primaryKeyColumns.Count;

                if (containsPrimaryKeyColumns)
                {
                    ColumnInfo primaryKeyColumn = primaryKeyColumns[0];
                    primaryKeyColumns.RemoveAt(0);

                    parentPrimaryKeys.Add(primaryKeyColumn.Name);
                    string columnType = primaryKeyColumn.ResolveSystemTypeName();
                    if (IsColumnNumeric(primaryKeyColumn) && string.IsNullOrEmpty(identityFieldType))
                    {
                        identityFieldType = columnType;
                        identityGeneratorType = $"IDatabase{primaryKeyColumn.DbType}IdentityGenerator";
                    }

                    if (primaryKeyColumns.Count > 0)
                    {
                        primaryKeyColumn = primaryKeyColumns[0];
                        primaryKeyColumns.RemoveAt(0);
                        string titleCasePrimaryKeyColumnName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(primaryKeyColumn.Name);

                        parentPrimaryKeys.Add(primaryKeyColumn.Name);

                        ForeignKeyValueColletion foreignKeyValueColletion = schemaInformationTableMapping.Value.ForeignKeyValues
                            .FirstOrDefault(fkv => fkv.ForeignKeyValueList
                                .Where(fkv => fkv.ForeignColunmnsToReferencedColumns
                                    .Any(fctrc => fctrc.ForeignColumn == primaryKeyColumn.Name.ToLower()))
                                .Any());

                        if (foreignKeyValueColletion != null)
                        {
                            foreach (ForeignKeyValue foreignKeyValue1 in foreignKeyValueColletion.ForeignKeyValueList)
                            {
                                foreach (ForeignColunmnToReferencedColumn foreignColunmnToReferencedColumn in foreignKeyValue1.ForeignColunmnsToReferencedColumns)
                                {
                                    ColumnInfo foreignKeyColumnInfo = entityColumns.FirstOrDefault(pkColumn => pkColumn.Name.ToUpper() == foreignColunmnToReferencedColumn.ForeignColumn.ToUpper());
                                    if (!string.IsNullOrEmpty(foreignKeyColumnInfo.Name))
                                        entityColumns.Remove(foreignKeyColumnInfo);
                                }
                            }

                            ForeignKeyValue foreignKeyValue =
                                foreignKeyValueColletion.ForeignKeyValueList.FirstOrDefault(fkv => fkv.ForeignColunmnsToReferencedColumns.Any(fctrc => fctrc.ForeignColumn == primaryKeyColumn.Name.ToLower()));

                            if (foreignKeyValue != null)
                            {
                                string referencedTableName = foreignKeyValue.ForeignColunmnsToReferencedColumns.Select(fctrc => fctrc.ReferencedTableName).FirstOrDefault()?.ToLower();
                                if (referencedTableName != null && _schemaInformation.SchemaTableMappings.ContainsKey(referencedTableName.ToLower()))
                                {
                                    string titleCaseReferencedTable = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(referencedTableName);
                                    parentDataTransferObjectName = $"{titleCaseReferencedTable}{DataTransferObjectNameSufix}";
                                    parentIdentityDataTransferObjectName = $"{titleCaseReferencedTable}{IdentityDataTransferObjectNameSufix}";

                                    if (!_namespaceDependenciesMap.ContainsKey(tableName.ToLower()))
                                        _namespaceDependenciesMap.Add(tableName.ToLower(), new List<string>());

                                    if (!_namespaceDependenciesMap[tableName.ToLower()].Contains(referencedTableName.ToLower()))
                                        _namespaceDependenciesMap[tableName.ToLower()].Add(referencedTableName.ToLower());
                                }
                            }
                        }
                    }
                }

                List<string> namespaceDependenciesList = new List<string>(
                    new string[]
                    {
                        "Crud.Abstractions.Application.Services",
                        "Database.IdentityGeneration",
                        "Domain.Model.DataQuery",
                        "Domain.Model.Messaging",
                        "Framework.Diagnostics",
                        "System.Threading.Tasks",
                    });

                string tableNameTitleCase = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName);

                string classNameSpace =
                    !string.IsNullOrEmpty(nameSpace)
                    ? $"{nameSpace}.{ApplicationProjectSufix}.{ServicesNamespaceSufix}.{tableNameTitleCase}"
                    : $"{ApplicationProjectSufix}.{ServicesNamespaceSufix}.{tableNameTitleCase}";

                namespaceDependenciesList.Add($"{projectName}.{ApplicationProjectSufix}.{DataTransferObjectsNamespaceSufix}.{tableNameTitleCase}");
                namespaceDependenciesList.Add($"{projectName}.{ApplicationProjectSufix}.{DataTransferObjectsNamespaceSufix}.{tableNameTitleCase}.{EventsNamespaceSufix}");
                namespaceDependenciesList.Add($"{projectName}.Model.Models.{tableNameTitleCase}");

                if (_namespaceDependenciesMap.ContainsKey(tableName.ToLower()))
                {
                    foreach (string namespaceDependency in _namespaceDependenciesMap[tableName.ToLower()])
                    {
                        string namespaceDependencyTitleCase = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(namespaceDependency);

                        namespaceDependenciesList.Add($"{projectName}.{ApplicationProjectSufix}.{DataTransferObjectsNamespaceSufix}.{namespaceDependencyTitleCase}");
                        namespaceDependenciesList.Add($"{projectName}.{ApplicationProjectSufix}.{DataTransferObjectsNamespaceSufix}.{namespaceDependencyTitleCase}.{EventsNamespaceSufix}");
                        namespaceDependenciesList.Add($"{projectName}.Model.Models.{namespaceDependencyTitleCase}");
                    }
                }

                string dataTransferObjectClassName = $"{tableNameTitleCase}{DataTransferObjectNameSufix}";
                string identityDataTransferObjectClassName = $"{tableNameTitleCase}{IdentityDataTransferObjectNameSufix}";
                string insertedEventClassName = $"{tableNameTitleCase}{InsertedEventNameSufix}";
                string updatedEventClassName = $"{tableNameTitleCase}{UpdatedEventNameSufix}";
                string deletedEventClassName = $"{tableNameTitleCase}{DeletedEventNameSufix}";
                string identityClassName = $"{tableNameTitleCase}{ModelGenerator.IdentityNameSufix}";
                string entityClassName = $"{tableNameTitleCase}{ModelGenerator.EntityNameSufix}";
                string entityDatabaseRepositoryClassName = $"{tableNameTitleCase}{ModelGenerator.EntityNameSufix}{ModelGenerator.DatabaseRepositoryNameSufix}";
                string entityDatabaseRepositoryFieldNameName = $"{tableName}{ModelGenerator.EntityNameSufix}{ModelGenerator.DatabaseRepositoryNameSufix}";
                string dataAdapterClassName = $"{tableNameTitleCase}{DataAdapterNameSufix}";

                string generatedCrudServiceClass = string.Empty;

                if (string.IsNullOrEmpty(parentDataTransferObjectName))
                {
                    /// 0  - {crud service class name}
                    /// 1  - {data transfer object type}
                    /// 2  - {data transfer object identity type}
                    /// 3  - {inserted event type}
                    /// 4  - {updated event type}
                    /// 5  - {deleted event type}
                    /// 6  - {entity type}
                    /// 7  - {identity type}
                    /// 8  - {identity filed type}
                    /// 9  - {identity generator type}
                    /// 10 - {repository type}
                    /// 11 - {repository field name}
                    /// 12 - {data adapter type}
                    
                    generatedCrudServiceClass =
                        string.Format(
                            crudServiceTemplate,
                            crudServiceClassName, // 0
                            dataTransferObjectClassName,// 1
                            identityDataTransferObjectClassName,// 2
                            insertedEventClassName,// 3
                            updatedEventClassName,// 4
                            deletedEventClassName,// 5
                            identityClassName,// 6
                            entityClassName,// 7
                            identityFieldType,//8
                            identityGeneratorType,//9
                            entityDatabaseRepositoryClassName,//10
                            entityDatabaseRepositoryFieldNameName,//11
                            dataAdapterClassName);//12
                }
                else
                {
                    /// 0  - {crud service class name}
                    /// 1  - {data transfer object type}
                    /// 2  - {data transfer object identity type}
                    /// 3  - {parent data transfer object type}
                    /// 4  - {inserted event type}
                    /// 5  - {updated event type}
                    /// 6  - {deleted event type}
                    /// 7  - {entity type}
                    /// 8  - {identity type}
                    /// 9  - {identity filed type}
                    /// 10  - {identity generator type}
                    /// 11 - {repository type}
                    /// 12 - {repository field name}
                    /// 13 - {data adapter type}
                    /// 14 - {create new identity with scope parameter}
                    /// 15 - {parent filter path}

                    List<string> parentFilterPaths = new List<string>();
                    if (_identityDataTransferObjectClassPrimaryKeysPropertyMap.ContainsKey(identityDataTransferObjectClassName))
                    {
                        int numberOfPaths = _identityDataTransferObjectClassPrimaryKeysPropertyMap[identityDataTransferObjectClassName].Count();
                        if (numberOfPaths > 0)
                        {
                            List<string> pathList = _identityDataTransferObjectClassPrimaryKeysPropertyMap[identityDataTransferObjectClassName].ToList();
                            for (int pathIndex = 0; pathIndex < numberOfPaths; pathIndex++)
                            {
                                if (pathIndex == 0)
                                    parentFilterPaths.Add($"{dataTransferObjectClassName}.{pathList.First()}");
                                else
                                    parentFilterPaths.Add($"{parentFilterPaths[pathIndex - 1]}.{pathList.First()}");

                                pathList.RemoveAt(0);
                            }

                            parentFilterPaths.Add($"{parentFilterPaths[parentFilterPaths.Count - 1]}.Id");
                        }
                    }

                    generatedCrudServiceClass =
                        string.Format(
                            crudServiceWithParentTemplate,
                            crudServiceClassName, // 0
                            dataTransferObjectClassName,// 1
                            identityDataTransferObjectClassName,// 2
                            parentDataTransferObjectName, // 3
                            insertedEventClassName,// 4
                            updatedEventClassName,// 5
                            deletedEventClassName,// 6
                            identityClassName,// 7
                            entityClassName,// 8
                            identityFieldType,//9
                            identityGeneratorType,//10
                            entityDatabaseRepositoryClassName,//11
                            entityDatabaseRepositoryFieldNameName,//12
                            dataAdapterClassName,//13
                            _identityDataTransferObjectClassPrimaryKeysPropertyMap.ContainsKey(identityDataTransferObjectClassName) &&
                            _identityDataTransferObjectClassPrimaryKeysPropertyMap[identityDataTransferObjectClassName].Count > 1
                                ? $"entity.Identity.{_identityDataTransferObjectClassPrimaryKeysPropertyMap[identityDataTransferObjectClassName][1]}"
                                : string.Empty,//14
                            string.Join(".", parentFilterPaths.Select(parentFilterPath => $"nameof({parentFilterPath})")));//15
                }

                _generatedCrudServiceClassesDictionary.Add(
                    tableName,
                    string.Format(
                        nameSpaceTemplate,
                        classNameSpace,
                        generatedCrudServiceClass,
                        string.Join("\r\n", namespaceDependenciesList.OrderBy(ns => ns).Select(dependencyNamespace => $"using {dependencyNamespace};"))));

                _generatedClassesList.Add(new GeneratedClassFromSchemaInformationTableMapping(
                    schemaInformationTableMapping.Value,
                    classNameSpace,
                    crudServiceClassName,
                    namespaceDependenciesList.OrderBy(ns => ns).ToList(),
                    _generatedCrudServiceClassesDictionary[tableName.ToLower()]));
            }
        }

        private void GenerateDataTransferObjectEventsClasses(string nameSpace)
        {
            foreach (KeyValuePair<string, SchemaInformationTableMapping> schemaInformationTableMapping in _schemaInformation.SchemaTableMappings.OrderBy(kp => kp.Value.Order))
            {
                string tableName = schemaInformationTableMapping.Key.ToLower();

                string classNameSpace =
                    !string.IsNullOrEmpty(nameSpace)
                    ? $"{nameSpace}.{ApplicationProjectSufix}.{DataTransferObjectsNamespaceSufix}.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}.{EventsNamespaceSufix}"
                    : $"{ApplicationProjectSufix}.{DataTransferObjectsNamespaceSufix}.{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}.{EventsNamespaceSufix}";

                List<string> namespaceDependenciesList = new List<string>(
                    new string[]
                    {
                        "Crud.Abstractions.Application.DataTransferObjects.Events",
                        "System",
                        "System.Runtime.Serialization",
                    });

                string insertedEventClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}{InsertedEventNameSufix}";

                string generatedInsertedEventClass = string.Format(
                    insertedEventClassTemplate,
                    insertedEventClassName,
                    _dataTransferObjectClassNamesMap[tableName],
                    tableName);

                _generatedClassesList.Add(new GeneratedClassFromSchemaInformationTableMapping(
                    schemaInformationTableMapping.Value,
                    classNameSpace,
                    insertedEventClassName,
                    namespaceDependenciesList.OrderBy(ns => ns).ToList(),
                    string.Format(
                        nameSpaceTemplate,
                        classNameSpace,
                        generatedInsertedEventClass,
                        string.Join("\r\n", namespaceDependenciesList.OrderBy(ns => ns).Select(dependencyNamespace => $"using {dependencyNamespace};")))));

                string updatedEventClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}{UpdatedEventNameSufix}";

                string generatedUpdatedEventClass = string.Format(
                    updatedEventClassTemplate,
                    updatedEventClassName,
                    _dataTransferObjectClassNamesMap[tableName],
                    tableName);

                _generatedClassesList.Add(new GeneratedClassFromSchemaInformationTableMapping(
                    schemaInformationTableMapping.Value,
                    classNameSpace,
                    updatedEventClassName,
                    namespaceDependenciesList.OrderBy(ns => ns).ToList(),
                    string.Format(
                        nameSpaceTemplate,
                        classNameSpace,
                        generatedUpdatedEventClass,
                        string.Join("\r\n", namespaceDependenciesList.OrderBy(ns => ns).Select(dependencyNamespace => $"using {dependencyNamespace};")))));

                string deletedEventClassName = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName)}{DeletedEventNameSufix}";

                string generatedDeletedEventClass = string.Format(
                    deletedEventClassTemplate,
                    deletedEventClassName,
                    _dataTransferObjectClassNamesMap[tableName],
                    tableName);

                _generatedClassesList.Add(new GeneratedClassFromSchemaInformationTableMapping(
                    schemaInformationTableMapping.Value,
                    classNameSpace,
                    deletedEventClassName,
                    namespaceDependenciesList.OrderBy(ns => ns).ToList(),
                    string.Format(
                        nameSpaceTemplate,
                        classNameSpace,
                        generatedDeletedEventClass,
                        string.Join("\r\n", namespaceDependenciesList.OrderBy(ns => ns).Select(dependencyNamespace => $"using {dependencyNamespace};")))));
            }
        }

        private void GenerateLogStorageAndAppMetadataClasse(string projectName, string nameSpace)
        {
            string classNameSpace =
                !string.IsNullOrEmpty(nameSpace)
                ? $"{nameSpace}.{ApplicationProjectSufix}"
                : $"{ApplicationProjectSufix}";

            string applicationMetadaClassName = $"{projectName}ApplicationMetadata";
            string generatedApplicationMetadataClass = string.Format(applicationMetadataClassTemplate, applicationMetadaClassName);
            _generatedClassesList.Add(new GeneratedClass(
                classNameSpace,
                applicationMetadaClassName,
                new[] { "Framework", "System" }.OrderBy(ns => ns).ToList(),
                string.Format(
                    nameSpaceTemplate,
                    classNameSpace,
                    generatedApplicationMetadataClass,
                    string.Join("\r\n", new[] { "Framework", "System" }.OrderBy(ns => ns).OrderBy(ns => ns).Select(dependencyNamespace => $"using {dependencyNamespace};")))));

            string logStorageClassName = $"{projectName}LogStorage";
            string generatedLogStorageClass = string.Format(logStorageClassTemplate, logStorageClassName);
            _generatedClassesList.Add(new GeneratedClass(
                classNameSpace,
                logStorageClassName,
                new[] { "Framework", "System" }.OrderBy(ns => ns).ToList(),
                string.Format(
                    nameSpaceTemplate,
                    classNameSpace,
                    generatedLogStorageClass,
                    string.Join("\r\n", new[] { "Framework.Diagnostics" }.OrderBy(ns => ns).OrderBy(ns => ns).Select(dependencyNamespace => $"using {dependencyNamespace};")))));
        }

        private void GenerateDependencyInversionClasses(string projectName, string nameSpace)
        {
            string classNameSpace =
                !string.IsNullOrEmpty(nameSpace)
                ? $"{nameSpace}.{ApplicationProjectSufix}.{DependencyInversionNamespaceSufix}"
                : $"{ApplicationProjectSufix}.{DependencyInversionNamespaceSufix}";

            string formattedProjectName = projectName[0].ToString().ToUpper() + string.Join("", projectName.Skip(1));
            string containerBuilderClassName = $"{formattedProjectName}ContainerBuilder";
            string containerRegistrationsClassName = $"{formattedProjectName}ContainerRegistrations";

            List<string> namespaceDependenciesList = new List<string>(
                new string[]
                {
                    "Crud.Abstractions.Application.DependencyInversion",
                    "DependencyInversion",
                    "System.Collections.Generic",
                });

            string generatedContainerBuilderClass =
                string.Format(
                    nameSpaceTemplate,
                    classNameSpace,
                    string.Format(createContainerBuilderTemplate, containerBuilderClassName, containerRegistrationsClassName),
                    string.Join("\r\n", namespaceDependenciesList.OrderBy(ns => ns).Select(dependencyNamespace => $"using {dependencyNamespace};")));

            if (_generatedDependencyInversionClassesDictionary.ContainsKey(containerBuilderClassName))
                _generatedDependencyInversionClassesDictionary[containerBuilderClassName] = generatedContainerBuilderClass;
            else
                _generatedDependencyInversionClassesDictionary.Add(containerBuilderClassName, generatedContainerBuilderClass);

            _generatedClassesList.Add(new GeneratedClass(
                classNameSpace,
                containerBuilderClassName,
                namespaceDependenciesList.OrderBy(ns => ns).ToList(),
                generatedContainerBuilderClass));

            namespaceDependenciesList.Clear();
            namespaceDependenciesList.AddRange(
                new string[]
                {
                    "Crud.Abstractions.Application.Services",
                    "Database.DataTransfer",
                    "DependencyInversion",
                    "Framework",
                    "System.Collections.Generic",
                    "System.Linq",
                });

            List<string> crudServiceRegistrations = new List<string>(_schemaInformation.SchemaTableMappings.Count);
            foreach (KeyValuePair<string, SchemaInformationTableMapping> schemaInformationTableMapping in _schemaInformation.SchemaTableMappings.OrderBy(kp => kp.Value.Order))
            {
                string tableName = schemaInformationTableMapping.Key;
                string titleCaseTableName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tableName);
                string parentTableName = string.Empty;
                List<ColumnInfo> primaryKeyColumns = new List<ColumnInfo>(schemaInformationTableMapping.Value.Columns.Where(col => col.IsPrimaryKey));
                int primaryKeyColumnsCount = primaryKeyColumns.Count;

                while (primaryKeyColumns.Count > 0)
                {
                    ColumnInfo entityColumn = primaryKeyColumns[0];
                    primaryKeyColumns.RemoveAt(0);

                    string columnType = entityColumn.ResolveSystemTypeName();
                    string propertyName = string.Empty;

                    ForeignKeyValueColletion foreignKeyValueColletion = schemaInformationTableMapping.Value.ForeignKeyValues.FirstOrDefault(fkv => fkv.ForeignKeyValueList.Where(fkv => fkv.ForeignColunmnsToReferencedColumns.Any(fctrc => fctrc.ForeignColumn == entityColumn.Name.ToLower())).Any());
                    if (foreignKeyValueColletion != null)
                    {
                        parentTableName = foreignKeyValueColletion.ReferencedTable;
                        primaryKeyColumns.Clear();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(parentTableName))
                {
                    /// 0 - {data transfer object type}
                    /// 1 - {identity data transfer object type}
                    /// 2 - {inserted event type}
                    /// 3 - {updated event type}
                    /// 4 - {deleted event type}
                    /// 5 - {crud service type}

                    crudServiceRegistrations.Add(
                        string.Format(
                            crudServiceRegistrationTemplate,
                            _dataTransferObjectClassNamesMap[tableName], // 0
                            _dataTransferIdentityObjectClassNamesMap[tableName], // 1
                            $"{titleCaseTableName}{InsertedEventNameSufix}", // 2
                            $"{titleCaseTableName}{UpdatedEventNameSufix}", // 3
                            $"{titleCaseTableName}{DeletedEventNameSufix}", // 4
                            $"{titleCaseTableName}{CrudServiceNameSufix}")); // 5
                }
                else
                {
                    /// 0 - {data transfer object type}
                    /// 1 - {identity data transfer object type}
                    /// 2 - {parent data transfer object type}
                    /// 3 - {inserted event type}
                    /// 4 - {updated event type}
                    /// 5 - {deleted event type}
                    /// 6 - {crud service type}

                    crudServiceRegistrations.Add(
                        string.Format(
                            crudServiceWithParentRegistrationTemplate,
                            _dataTransferObjectClassNamesMap[tableName], // 0
                            _dataTransferIdentityObjectClassNamesMap[tableName], // 1
                            _dataTransferObjectClassNamesMap[parentTableName], // 2
                            $"{titleCaseTableName}{InsertedEventNameSufix}", // 3
                            $"{titleCaseTableName}{UpdatedEventNameSufix}", // 4
                            $"{titleCaseTableName}{DeletedEventNameSufix}", // 5
                            $"{titleCaseTableName}{CrudServiceNameSufix}")); // 6
                }

                namespaceDependenciesList.Add($"{projectName}.{ApplicationProjectSufix}.{DataTransferObjectsNamespaceSufix}.{titleCaseTableName}");
                namespaceDependenciesList.Add($"{projectName}.{ApplicationProjectSufix}.{DataTransferObjectsNamespaceSufix}.{titleCaseTableName}.{EventsNamespaceSufix}");
                namespaceDependenciesList.Add($"{projectName}.{ApplicationProjectSufix}.{ServicesNamespaceSufix}.{titleCaseTableName}");
                namespaceDependenciesList.Add($"{projectName}.Model.Models.{titleCaseTableName}");
            }

            string generatedContainerRegistrationClass =
                string.Format(
                    nameSpaceTemplate,
                    classNameSpace,
                    string.Format(
                        createContainerRegistrationsTemplate,
                        containerRegistrationsClassName,
                        string.Join("\r\n", crudServiceRegistrations),
                        string.Empty),
                    string.Join("\r\n", namespaceDependenciesList.OrderBy(ns => ns).Select(dependencyNamespace => $"using {dependencyNamespace};")));

            if (_generatedDependencyInversionClassesDictionary.ContainsKey(containerRegistrationsClassName))
                _generatedDependencyInversionClassesDictionary[containerRegistrationsClassName] = generatedContainerRegistrationClass;
            else
                _generatedDependencyInversionClassesDictionary.Add(containerRegistrationsClassName, generatedContainerRegistrationClass);

            _generatedClassesList.Add(new GeneratedClass(
                classNameSpace,
                containerRegistrationsClassName,
                namespaceDependenciesList.OrderBy(ns => ns).ToList(),
                generatedContainerRegistrationClass));
        }

        private string GenerateSerializationInfoGetFunction(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                case DbType.Xml:
                    return "info.GetString";

                case DbType.Date:
                case DbType.DateTime2:
                    return "info.GetDateTime";

                case DbType.Time:
                    return "info.GetTimeSpan";

                case DbType.Binary:
                case DbType.Object:
                    return "info.GetBytes";

                case DbType.Currency:
                    return "info.GetDecimal";
                case DbType.Single:
                    return "info.GetFloat";

                case DbType.VarNumeric:
                    return "info.GetDouble";

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
                    return $"info.Get{dbType}";
                default:
                    throw new InvalidCastException();
            }
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
            if (_identityDataTransferObjectClassPropertiesMap.ContainsKey(columnType))
            {
                (string PropertyName, string PropertyType)? dependencyPropertyTuple = _identityDataTransferObjectClassPropertiesMap[columnType]?.FirstOrDefault(p => p.PropertyName.ToLower() == referencedColumnName.ToLower());
                if (dependencyPropertyTuple == (null, null))
                    dependencyPropertyTuple = _identityDataTransferObjectClassPropertiesMap[columnType]?.FirstOrDefault();

                if (dependencyPropertyTuple != (null, null))
                {
                    parameterPath = $"{parameterPath}.{dependencyPropertyTuple.Value.PropertyName}";
                    string dependencyPropertyType = dependencyPropertyTuple.Value.PropertyType;
                    if (_identityDataTransferObjectClassPropertiesMap.ContainsKey(dependencyPropertyType))
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
