using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace SilverlightPhoneDatabase.Core
{
    /// <summary>
    /// Interface that is used for tables in a database
    /// </summary>
    public interface ITable
    {
        /// <summary>
        /// If a table is named, contains table name, otherwise empty string
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// Type of object that database contains
        /// </summary>
        Type RowType { get; }

        /// <summary>
        /// Save table le to the Isolated Storage
        /// </summary>
        void Save();

        /// <summary>
        /// Write content of a table to a file stream
        /// </summary>
        /// <param name="stream">Stream to write table to</param>
        void WriteTableToStream(Stream stream);

        /// <summary>
        /// Set internal variables data when a table is de-serialized
        /// </summary>
        /// <param name="databaseName">Database name that table belongs to</param>
        /// <param name="password">Password to use for encryption</param>
        /// <param name="tableName">If a table is named, contains table name, otherwise empty string</param>
        void SetTableDefinition(string databaseName, string password, string tableName = "");

        /// <summary>
        /// Save table le to the Isolated Storage
        /// </summary>
        /// <param name="store">Isolated storage file passed from database</param>
        void Save(IsolatedStorageFile store);

        /// <summary>
        /// Asynchronously save table le to the Isolated Storage
        /// </summary>
        /// <param name="callback">Function  to be invoked after save complete</param>
        void BeginSave(Action<SaveResult> callback);
    }
}
