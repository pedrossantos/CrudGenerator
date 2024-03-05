using CrudGenerator.Core.ViewModels;
using Database.Sqlite.DataAccess;
using DependencyInversion;
using Framework.Validation;
using System.IO;
using System.Windows;
using System.Windows.Input;
using View.Abstractions;

namespace CrudGenerator.Core.Wpf.Components
{
    public partial class SqliteConnectionConfiguration : DbConnectionConfiguration
    {
        public static readonly DependencyProperty SqliteConnectionConfigurationViewModelProperty =
            DependencyProperty.Register(
                nameof(SqliteConnectionConfigurationViewModel),
                typeof(SqliteConnectionConfigurationViewModel),
                typeof(SqliteConnectionConfiguration),
                new FrameworkPropertyMetadata(null, OnSqliteConnectionConfigurationViewModelChanged));

        public static readonly DependencyProperty OpenFileDialogProperty =
            DependencyProperty.Register(
                nameof(OpenFileDialog),
                typeof(IOpenFileDialog),
                typeof(SqliteConnectionConfiguration),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnOpenFileDialogChanged)));

        public static readonly DependencyProperty SqliteDatabasePathProperty =
            DependencyProperty.Register(
                nameof(SqliteDatabasePath),
                typeof(string),
                typeof(SqliteConnectionConfiguration),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSqliteDatabasePathChanged)));

        public SqliteConnectionConfiguration()
            : base()
        {
            PropertyChanged += SqliteConnectionConfigurationPropertyChanged;

            InitializeComponent();
        }

        public SqliteConnectionConfiguration(
            SqliteSchemaInformation sqliteSchemaInformation,
            IMessageDialog messageDialog,
            IOpenFileDialog openFileDialog)
            : base(sqliteSchemaInformation, messageDialog)
        {
            Requires.NotNull(openFileDialog, nameof(openFileDialog));

            OpenFileDialog = openFileDialog;

            DbConnectionConfigurationViewModel = SqliteConnectionConfigurationViewModel = new SqliteConnectionConfigurationViewModel(
                sqliteSchemaInformation,
                MessageDialog,
                OpenFileDialog);

            PropertyChanged += SqliteConnectionConfigurationPropertyChanged;

            InitializeComponent();
        }

        [Injectable]
        public SqliteConnectionConfiguration(SqliteConnectionConfigurationViewModel sqliteConnectionConfigurationViewModel)
            : base(sqliteConnectionConfigurationViewModel)
        {
            Requires.NotNull(sqliteConnectionConfigurationViewModel, nameof(sqliteConnectionConfigurationViewModel));

            DbConnectionConfigurationViewModel = SqliteConnectionConfigurationViewModel = sqliteConnectionConfigurationViewModel;
            OpenFileDialog = sqliteConnectionConfigurationViewModel.OpenFileDialog;

            PropertyChanged += SqliteConnectionConfigurationPropertyChanged;

            InitializeComponent();
        }

        public SqliteConnectionConfigurationViewModel SqliteConnectionConfigurationViewModel
        {
            get { return GetValue(SqliteConnectionConfigurationViewModelProperty) as SqliteConnectionConfigurationViewModel; }
            set { SetValue(SqliteConnectionConfigurationViewModelProperty, value); }
        }

        public IOpenFileDialog OpenFileDialog
        {
            get { return GetValue(OpenFileDialogProperty) as IOpenFileDialog; }
            set { SetValue(OpenFileDialogProperty, value); }
        }

        public string SqliteDatabasePath
        {
            get { return GetValue(SqliteDatabasePathProperty)?.ToString(); }
            set { SetValue(SqliteDatabasePathProperty, value); }
        }

        private static void OnSqliteConnectionConfigurationViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is SqliteConnectionConfiguration sqliteConnectionConfiguration) && (e.OldValue != e.NewValue))
            {
                if (e.NewValue is SqliteConnectionConfigurationViewModel newSqliteConnectionConfigurationViewModel)
                {
                    sqliteConnectionConfiguration.DbConnectionConfigurationViewModel = newSqliteConnectionConfigurationViewModel;

                    sqliteConnectionConfiguration.SqliteDatabasePath = newSqliteConnectionConfigurationViewModel.SqliteDatabasePath;
                    sqliteConnectionConfiguration.OpenFileDialog = newSqliteConnectionConfigurationViewModel.OpenFileDialog;
                }
            }
        }

        public static void OnOpenFileDialogChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is SqliteConnectionConfiguration sqliteConnectionConfiguration)
            {
                if (sqliteConnectionConfiguration.SqliteConnectionConfigurationViewModel != null)
                    sqliteConnectionConfiguration.SqliteConnectionConfigurationViewModel.OpenFileDialog = sqliteConnectionConfiguration.OpenFileDialog;

                sqliteConnectionConfiguration.PropertyChangedDispatcher.Notify(nameof(OpenFileDialog));
            }
        }

        public static void OnSqliteDatabasePathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is SqliteConnectionConfiguration sqliteConnectionConfiguration)
            {
                if (sqliteConnectionConfiguration.SqliteConnectionConfigurationViewModel != null)
                    sqliteConnectionConfiguration.SqliteConnectionConfigurationViewModel.SqliteDatabasePath = sqliteConnectionConfiguration.SqliteDatabasePath;

                sqliteConnectionConfiguration.PropertyChangedDispatcher.Notify(nameof(SqliteDatabasePath));
            }
        }

        public override void Dispose()
        {
        }

        private async void SelectSqliteDatabasePreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Dictionary<string, Stream> fileSelected = await OpenFileDialog.OpenAsync(false, Enumerable.Empty<string>());
                if (fileSelected.Count > 0)
                {
                    SqliteDatabasePath = fileSelected.ElementAt(0).Key;
                    fileSelected.ElementAt(0).Value.Close();
                    fileSelected.ElementAt(0).Value.Dispose();
                }
            }
            catch (Exception ex)
            {
                if (MessageDialog != null)
                    await MessageDialog.ShowAsync($"Error to open database:\r\n{ex.Message}", "Error", EventLevel.Error);
            }
        }

        private void SqliteConnectionConfigurationPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == nameof(DbConnectionConfigurationViewModel)) && (DbConnectionConfigurationViewModel is SqliteConnectionConfigurationViewModel sqliteConnectionConfigurationViewModel))
            {
                OpenFileDialog = sqliteConnectionConfigurationViewModel.OpenFileDialog;
                SqliteDatabasePath = sqliteConnectionConfigurationViewModel.SqliteDatabasePath;
            }
        }
    }
}
