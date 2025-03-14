﻿using Database.DataAccess;
using Database.DataMapping;
using Database.MySql.DataAccess;
using Database.PostgreSql.DataAccess;
using Database.Sqlite.DataAccess;
using Database.SqlServer.DataAccess;
using DependencyInversion;
using Framework.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using View.Abstractions;
using View.Abstractions.UseCase;

namespace CrudGenerator.Core.ViewModels
{
    public class ProjectGeneratorViewModel : ViewModelBase
    {
        private bool _generatingClasses;
        private Thread _threadGenerateClasses;

        private bool _generatingProject;
        private Thread _threadGenerateProject;

        private DatabaseTypes _selectedDatabaseType;
        private MySqlSchemaInformation _mySqlSchemaInformation;
        private PostgreSqlSchemaInformation _postgreSqlSchemaInformation;
        private SqliteSchemaInformation _sqliteSchemaInformation;
        private SqlServerSchemaInformation _sqlServerSchemaInformation;
        private IMessageDialog _messageDialog;
        private ISelectFolderDialog _selectFolderDialog;

        private string _defaultNamespace;
        private string _projectFolder;
        private string _projectName;
        private ModelGenerator _modelGenerator;
        private ApplicationGenerator _applicationGenerator;
        private GeneratedClass _selectedGenerated;

        public ProjectGeneratorViewModel()
        {
        }

        [Injectable]
        public ProjectGeneratorViewModel(
            MySqlSchemaInformation mySqlSchemaInformation,
            PostgreSqlSchemaInformation postgreSqlSchemaInformation,
            SqliteSchemaInformation sqliteSchemaInformation,
            SqlServerSchemaInformation sqlServerSchemaInformation,
            IMessageDialog messageDialog,
            ISelectFolderDialog selectFolderDialog)
        {
            _mySqlSchemaInformation = mySqlSchemaInformation;
            _mySqlSchemaInformation.SchemaTableMappingsObservableCollection.CollectionChanged += SchemaTableMappingsObservableCollectionCollectionChanged;

            _postgreSqlSchemaInformation = postgreSqlSchemaInformation;
            _postgreSqlSchemaInformation.SchemaTableMappingsObservableCollection.CollectionChanged += SchemaTableMappingsObservableCollectionCollectionChanged;

            _sqliteSchemaInformation = sqliteSchemaInformation;
            _sqliteSchemaInformation.SchemaTableMappingsObservableCollection.CollectionChanged += SchemaTableMappingsObservableCollectionCollectionChanged;

            _sqlServerSchemaInformation = sqlServerSchemaInformation;
            _sqlServerSchemaInformation.SchemaTableMappingsObservableCollection.CollectionChanged += SchemaTableMappingsObservableCollectionCollectionChanged;

            _messageDialog = messageDialog;
            _selectFolderDialog = selectFolderDialog;

            PropertyChanged += ProjectGeneratorViewModelPropertyChanged;
        }

        protected ISchemaInformation SchemaInformation
        {
            get
            {
                if (SelectedDatabaseType == DatabaseTypes.MySql)
                    return _mySqlSchemaInformation;
                else if (SelectedDatabaseType == DatabaseTypes.PostgreSql)
                    return _postgreSqlSchemaInformation;
                else if (SelectedDatabaseType == DatabaseTypes.Sqlite)
                    return _sqliteSchemaInformation;
                else if (SelectedDatabaseType == DatabaseTypes.SqlServer)
                    return _sqlServerSchemaInformation;

                return null;
            }
        }

        public DatabaseTypes SelectedDatabaseType
        {
            get => _selectedDatabaseType;
            set => PropertyChangedDispatcher.SetProperty(ref _selectedDatabaseType, value);
        }

        public bool GeneratingClasses
        {
            get => _generatingClasses;
            set => PropertyChangedDispatcher.SetProperty(ref _generatingClasses, value);
        }

        public bool GeneratingProject
        {
            get => _generatingProject;
            set => PropertyChangedDispatcher.SetProperty(ref _generatingProject, value);
        }

        public MySqlSchemaInformation MySqlSchemaInformation
        {
            get
            {
                return _mySqlSchemaInformation;
            }

            set
            {
                if (_mySqlSchemaInformation != value)
                {
                    PropertyChangedDispatcher.SetProperty(ref _mySqlSchemaInformation, value);
                    if (_selectedDatabaseType == DatabaseTypes.MySql)
                        PropertyChangedDispatcher.Notify(nameof(SchemaInformation));
                }
            }
        }

        public PostgreSqlSchemaInformation PostgreSqlSchemaInformation
        {
            get
            {
                return _postgreSqlSchemaInformation;
            }

            set
            {
                if (_postgreSqlSchemaInformation != value)
                {
                    PropertyChangedDispatcher.SetProperty(ref _postgreSqlSchemaInformation, value);
                    if (_selectedDatabaseType == DatabaseTypes.PostgreSql)
                        PropertyChangedDispatcher.Notify(nameof(SchemaInformation));
                }
            }
        }

        public SqliteSchemaInformation SqliteSchemaInformation
        {
            get
            {
                return _sqliteSchemaInformation;
            }

            set
            {
                if (_sqliteSchemaInformation != value)
                {
                    PropertyChangedDispatcher.SetProperty(ref _sqliteSchemaInformation, value);
                    if (_selectedDatabaseType == DatabaseTypes.Sqlite)
                        PropertyChangedDispatcher.Notify(nameof(SchemaInformation));
                }
            }
        }

        public SqlServerSchemaInformation SqlServerSchemaInformation
        {
            get
            {
                return _sqlServerSchemaInformation;
            }

            set
            {
                if (_sqlServerSchemaInformation != value)
                {
                    PropertyChangedDispatcher.SetProperty(ref _sqlServerSchemaInformation, value);
                    if (_selectedDatabaseType == DatabaseTypes.SqlServer)
                        PropertyChangedDispatcher.Notify(nameof(SchemaInformation));
                }
            }
        }

        public IMessageDialog MessageDialog
        {
            get => _messageDialog;
            set => PropertyChangedDispatcher.SetProperty(ref _messageDialog, value);
        }

        public ISelectFolderDialog SelectFolderDialog
        {
            get => _selectFolderDialog;
            set => PropertyChangedDispatcher.SetProperty(ref _selectFolderDialog, value);
        }

        public string DefaultNamespace
        {
            get => _defaultNamespace;
            set => PropertyChangedDispatcher.SetProperty(ref _defaultNamespace, value);
        }

        public string ProjectFolder
        {
            get => _projectFolder;
            set => PropertyChangedDispatcher.SetProperty(ref _projectFolder, value);
        }

        public string ProjectName
        {
            get => _projectName;
            set => PropertyChangedDispatcher.SetProperty(ref _projectName, value);
        }

        public ModelGenerator ModelGenerator
        {
            get => _modelGenerator;
            set => PropertyChangedDispatcher.SetProperty(ref _modelGenerator, value);
        }

        public ApplicationGenerator ApplicationGenerator
        {
            get => _applicationGenerator;
            set => PropertyChangedDispatcher.SetProperty(ref _applicationGenerator, value);
        }

        public GeneratedClass SelectedGeneratedClass
        {
            get => _selectedGenerated;
            set => PropertyChangedDispatcher.SetProperty(ref _selectedGenerated, value);
        }

        public List<GeneratedClassGroupedByNamespace> GeneratedClasses =>
            new List<GeneratedClassGroupedByNamespace>(
                GeneratedModelClasses
                    .Concat(GeneratedApplicationClasses));

        public IEnumerable<GeneratedClassGroupedByNamespace> GeneratedModelClasses =>
            ModelGenerator?.GeneratedClasses?
                .GroupBy(gc => gc.ClassNameSpace)
                .Select(g => new GeneratedClassGroupedByNamespace(g)) ?? new GeneratedClassGroupedByNamespace[0];

        public IEnumerable<GeneratedClassGroupedByNamespace> GeneratedApplicationClasses =>
            ApplicationGenerator?.GeneratedClasses?
                .GroupBy(gc => gc.ClassNameSpace)
                .Select(g => new GeneratedClassGroupedByNamespace(g)) ?? new GeneratedClassGroupedByNamespace[0];

        public void SelectProjectFolder()
        {
            ProjectFolder = SelectFolderDialog.SelectFolder();
        }

        public async Task GenerateClasses()
        {
            if (string.IsNullOrEmpty(ProjectName))
            {
                await MessageDialog.ShowAsync(Messages.SelectProjectName, Messages.ErrorTitle, EventLevel.Error);
                return;
            }

            if ((!_generatingClasses) && (_threadGenerateClasses != null) && _threadGenerateClasses.IsAlive)
            {
                _threadGenerateClasses.Abort();
                _threadGenerateClasses = null;
            }

            if (ModelGenerator != null)
                ModelGenerator.Dispose();

            ModelGenerator = new ModelGenerator(SchemaInformation);
            PropertyChangedDispatcher.Notify(nameof(ModelGenerator));

            if (ApplicationGenerator != null)
                ApplicationGenerator.Dispose();

            ApplicationGenerator = new ApplicationGenerator(SchemaInformation);
            PropertyChangedDispatcher.Notify(nameof(ApplicationGenerator));

            GeneratingClasses = true;
            _threadGenerateProject = new Thread(new ParameterizedThreadStart(DoGenerateClasses));
            _threadGenerateProject.Start(
                new object[]
                {
                    _projectName,
                    _defaultNamespace,
                    _modelGenerator,
                    _applicationGenerator,
                });
        }

        public async Task GenerateProject()
        {
            if (string.IsNullOrEmpty(ProjectName))
            {
                await MessageDialog.ShowAsync(Messages.SelectProjectName, Messages.ErrorTitle, EventLevel.Error);
                return;
            }

            if (string.IsNullOrEmpty(ProjectFolder))
            {
                await MessageDialog.ShowAsync(Messages.SelectProjectFolder, Messages.ErrorTitle, EventLevel.Error);
                return;
            }

            if ((!_generatingProject) && (_threadGenerateProject != null) && _threadGenerateProject.IsAlive)
            {
                _threadGenerateProject.Abort();
                _threadGenerateProject = null;
            }

            GeneratingProject = true;
            _threadGenerateProject = new Thread(new ParameterizedThreadStart(DoGenerateProject));
            _threadGenerateProject.Start(
                new object[]
                {
                    _projectFolder,
                    _projectName,
                    _defaultNamespace,
                    ModelGenerator.GeneratedClasses,
                    ApplicationGenerator.GeneratedClasses,
                });
        }

        private void DoGenerateClasses(object args)
        {
            try
            {
                object[] argsArray = args as object[];
                string projectName = argsArray[0]?.ToString();
                string nameSpace = argsArray[1]?.ToString();
                ModelGenerator modelGenerator = argsArray[2] as ModelGenerator;
                ApplicationGenerator applicationGenerator = argsArray[3] as ApplicationGenerator;

                modelGenerator.GenerateClasses(projectName, string.IsNullOrEmpty(nameSpace) ? projectName : $"{projectName}.{nameSpace}");
                applicationGenerator.GenerateClasses(projectName, string.IsNullOrEmpty(nameSpace) ? projectName : $"{projectName}.{nameSpace}");
            }
            finally
            {
                PropertyChangedDispatcher.Notify(nameof(SelectedGeneratedClass));
                PropertyChangedDispatcher.Notify(nameof(GeneratedModelClasses));
                PropertyChangedDispatcher.Notify(nameof(GeneratedApplicationClasses));

                GeneratingClasses = false;
            }
        }

        private void DoGenerateProject(object args)
        {
            try
            {
                object[] argsArray = args as object[];
                string projectFolder = argsArray[0]?.ToString();
                string projectName = argsArray[1]?.ToString();
                string nameSpace = argsArray[2]?.ToString();

                ObservableCollection<GeneratedClass> generatedModelClasses =
                    argsArray[3] as ObservableCollection<GeneratedClass> ?? new ObservableCollection<GeneratedClass>();

                ObservableCollection<GeneratedClass> generatedApplicationClasses =
                    argsArray[4] as ObservableCollection<GeneratedClass> ?? new ObservableCollection<GeneratedClass>();

                string solutionPath = Path.Combine(projectFolder, projectName);
                if (Directory.Exists(solutionPath))
                    Directory.Delete(solutionPath, true);

                Directory.CreateDirectory(solutionPath);

                string srcPath = Path.Combine(solutionPath, "src");
                if (Directory.Exists(srcPath))
                    Directory.Delete(srcPath, true);

                Directory.CreateDirectory(srcPath);

                GenerateModelProject(
                    srcPath,
                    nameSpace,
                    projectName,
                    generatedModelClasses);

                GenerateApplicationProject(
                    srcPath,
                    nameSpace,
                    projectName,
                    generatedApplicationClasses);
            }
            finally
            {
                GeneratingProject = false;
            }
        }

        private void GenerateModelProject(
            string srcPath,
            string nameSpace,
            string projectName,
            ObservableCollection<GeneratedClass> modelClasses)
        {
            #region Models Project
            string modelProjectPath = string.IsNullOrEmpty(nameSpace)
                ? Path.Combine(srcPath, $"{projectName}.{ModelGenerator.ModelProjectSufix}")
                : Path.Combine(srcPath, $"{projectName}.{nameSpace}.{ModelGenerator.ModelProjectSufix}");

            if (Directory.Exists(modelProjectPath))
                Directory.Delete(modelProjectPath, true);

            Directory.CreateDirectory(modelProjectPath);

            TreeList<string> namespaceModelTreeList = new TreeList<string>(projectName);

            string modelClassesNamespace = string.IsNullOrEmpty(nameSpace)
                ? $"{projectName}.{ModelGenerator.ModelProjectSufix}"
                : $"{projectName}.{nameSpace}.{ModelGenerator.ModelProjectSufix}";

            foreach (GeneratedClass generatedClass in modelClasses)
            {
                string modelsProjectFolderPath = modelProjectPath;

                List<string> partialNamespaceList = new List<string>(
                    generatedClass.ClassNameSpace == modelClassesNamespace
                        ? Enumerable.Empty<string>()
                        : generatedClass.ClassNameSpace.Replace($"{modelClassesNamespace}.", "").Split('.'));

                TreeNode<string> currentTreeNode = namespaceModelTreeList.Root;
                while (partialNamespaceList.Count > 0)
                {
                    string partialNamespace = partialNamespaceList[0];
                    TreeNode<string> treeNode = currentTreeNode.Find(partialNamespace);
                    if (treeNode == null)
                    {
                        currentTreeNode = currentTreeNode.AddOrGet(partialNamespace);

                    }
                    else
                        currentTreeNode = treeNode;

                    modelsProjectFolderPath = Path.Combine(modelsProjectFolderPath, partialNamespace);
                    if (!Directory.Exists(modelsProjectFolderPath))
                        Directory.CreateDirectory(modelsProjectFolderPath);

                    partialNamespaceList.RemoveAt(0);
                }

                StreamWriter streamWriterClass = new StreamWriter(Path.Combine(modelsProjectFolderPath, $"{generatedClass.ClassName}.cs"));
                streamWriterClass.Write(generatedClass.ClassContent);
                streamWriterClass.Close();
                streamWriterClass.Dispose();
            }

            StreamWriter streamWriterProject =
                string.IsNullOrEmpty(nameSpace)
                ? new StreamWriter(Path.Combine(modelProjectPath, $"{projectName}.{ModelGenerator.ModelProjectSufix}.csproj"))
                : new StreamWriter(Path.Combine(modelProjectPath, $"{projectName}.{nameSpace}.{ModelGenerator.ModelProjectSufix}.csproj"));
            streamWriterProject.Write(
@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFrameworks>netstandard2.0;netstandard2.1;net461;net462</TargetFrameworks>
    <Nullable>disable</Nullable>
    <LangVersion>10.0</LangVersion>
  </PropertyGroup>

</Project>");
            streamWriterProject.Close();
            streamWriterProject.Dispose();
            #endregion
        }

        private void GenerateApplicationProject(
            string srcPath,
            string nameSpace,
            string projectName,
            ObservableCollection<GeneratedClass> applicationClasses)
        {
            #region Applications Project
            string applicationProjectPath = string.IsNullOrEmpty(nameSpace)
                ? Path.Combine(srcPath, $"{projectName}.{ApplicationGenerator.ApplicationProjectSufix}")
                : Path.Combine(srcPath, $"{projectName}.{nameSpace}.{ApplicationGenerator.ApplicationProjectSufix}");

            if (Directory.Exists(applicationProjectPath))
                Directory.Delete(applicationProjectPath, true);

            Directory.CreateDirectory(applicationProjectPath);

            TreeList<string> namespaceApplicationTreeList = new TreeList<string>(projectName);

            string applicationClassesNamespace = string.IsNullOrEmpty(nameSpace)
                ? $"{projectName}.{ApplicationGenerator.ApplicationProjectSufix}"
                : $"{projectName}.{nameSpace}.{ApplicationGenerator.ApplicationProjectSufix}";

            foreach (GeneratedClass generatedClass in applicationClasses)
            {
                string applicationsProjectFolderPath = applicationProjectPath;

                List<string> partialNamespaceList = new List<string>(
                    generatedClass.ClassNameSpace == applicationClassesNamespace
                        ? Enumerable.Empty<string>()
                        : generatedClass.ClassNameSpace.Replace($"{applicationClassesNamespace}.", "").Split('.'));

                TreeNode<string> currentTreeNode = namespaceApplicationTreeList.Root;
                while (partialNamespaceList.Count > 0)
                {
                    string partialNamespace = partialNamespaceList[0];
                    TreeNode<string> treeNode = currentTreeNode.Find(partialNamespace);
                    if (treeNode == null)
                    {
                        currentTreeNode = currentTreeNode.AddOrGet(partialNamespace);
                    }
                    else
                        currentTreeNode = treeNode;

                    applicationsProjectFolderPath = Path.Combine(applicationsProjectFolderPath, partialNamespace);
                    if (!Directory.Exists(applicationsProjectFolderPath))
                        Directory.CreateDirectory(applicationsProjectFolderPath);

                    partialNamespaceList.RemoveAt(0);
                }

                StreamWriter streamWriterClass = new StreamWriter(Path.Combine(applicationsProjectFolderPath, $"{generatedClass.ClassName}.cs"));
                streamWriterClass.Write(generatedClass.ClassContent);
                streamWriterClass.Close();
                streamWriterClass.Dispose();
            }

            StreamWriter streamWriterProject =
                string.IsNullOrEmpty(nameSpace)
                ? new StreamWriter(Path.Combine(applicationProjectPath, $"{projectName}.{ApplicationGenerator.ApplicationProjectSufix}.csproj"))
                : new StreamWriter(Path.Combine(applicationProjectPath, $"{projectName}.{nameSpace}.{ApplicationGenerator.ApplicationProjectSufix}.csproj"));
            streamWriterProject.Write(
@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFrameworks>netstandard2.0;netstandard2.1;net461;net462</TargetFrameworks>
    <Nullable>disable</Nullable>
    <LangVersion>10.0</LangVersion>
  </PropertyGroup>

</Project>");
            streamWriterProject.Close();
            streamWriterProject.Dispose();
            #endregion
        }

        private void SchemaTableMappingsObservableCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                if ((_modelGenerator != null) && (_modelGenerator.GeneratedClasses != null))
                    _modelGenerator.GeneratedClasses.Clear();

                if ((_applicationGenerator != null) && (_applicationGenerator.GeneratedClasses != null))
                    _applicationGenerator.GeneratedClasses.Clear();
            }
        }

        private void ProjectGeneratorViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedDatabaseType))
            {
                DefaultNamespace = string.Empty;
                ProjectFolder = string.Empty;
                ProjectName = string.Empty;

                PropertyChangedDispatcher.Notify(nameof(SelectedGeneratedClass));
                PropertyChangedDispatcher.Notify(nameof(GeneratedModelClasses));
            }
        }
    }
}
