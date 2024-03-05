using Database.DataMapping;
using System.Windows;
using System.Windows.Controls;

namespace CrudGenerator.Core.Wpf.DataTemplateSelectors
{
    public class DatabaseConnectionDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate None { get; set; }

        public DataTemplate MySql { get; set; }

        public DataTemplate Sqlite { get; set; }

        public DataTemplate SqlServer { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is DatabaseTypes databaseType)
            {
                switch (databaseType)
                {

                    case DatabaseTypes.SqlServer:
                        return SqlServer;

                    case DatabaseTypes.MySql:
                        return MySql;

                    case DatabaseTypes.Sqlite:
                        return Sqlite;

                    case DatabaseTypes.None:
                    case DatabaseTypes.OracleDb:
                    case DatabaseTypes.PostgreSql:
                    case DatabaseTypes.All:
                    default:
                        return None;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}
