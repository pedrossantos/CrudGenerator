﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CrudGenerator.Core {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Messages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Messages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CrudGenerator.Core.Messages", typeof(Messages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Database Connection Configuration.
        /// </summary>
        public static string DatabaseConnectionConfiguration {
            get {
                return ResourceManager.GetString("DatabaseConnectionConfiguration", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please enter the Database Name!.
        /// </summary>
        public static string EnterDatabaseName {
            get {
                return ResourceManager.GetString("EnterDatabaseName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please enter the Server Name or Server IP Address!.
        /// </summary>
        public static string EnterServerNameOrIpAddres {
            get {
                return ResourceManager.GetString("EnterServerNameOrIpAddres", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please enter the User ID!.
        /// </summary>
        public static string EnterUserId {
            get {
                return ResourceManager.GetString("EnterUserId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error to open database:
        ///{0}.
        /// </summary>
        public static string ErrorOnOpenSqliteDatabase {
            get {
                return ResourceManager.GetString("ErrorOnOpenSqliteDatabase", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error.
        /// </summary>
        public static string ErrorTitle {
            get {
                return ResourceManager.GetString("ErrorTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MySql Database Connection Configuration.
        /// </summary>
        public static string MySqlDatabaseConnectionConfiguration {
            get {
                return ResourceManager.GetString("MySqlDatabaseConnectionConfiguration", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to PostgreSql Database Connection Configuration.
        /// </summary>
        public static string PostgreSqlDatabaseConnectionConfiguration {
            get {
                return ResourceManager.GetString("PostgreSqlDatabaseConnectionConfiguration", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please select the File Path!.
        /// </summary>
        public static string SelectFilePath {
            get {
                return ResourceManager.GetString("SelectFilePath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please select the Project Folder!.
        /// </summary>
        public static string SelectProjectFolder {
            get {
                return ResourceManager.GetString("SelectProjectFolder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please type the Project Name!.
        /// </summary>
        public static string SelectProjectName {
            get {
                return ResourceManager.GetString("SelectProjectName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sqlite Database Connection Configuration.
        /// </summary>
        public static string SqliteDatabaseConnectionConfiguration {
            get {
                return ResourceManager.GetString("SqliteDatabaseConnectionConfiguration", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SqlServer Database Connection Configuration.
        /// </summary>
        public static string SqlServerDatabaseConnectionConfiguration {
            get {
                return ResourceManager.GetString("SqlServerDatabaseConnectionConfiguration", resourceCulture);
            }
        }
    }
}