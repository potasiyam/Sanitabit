using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Xml.Serialization;
using SilverlightPhoneDatabase.Core;
using SilverlightPhoneDatabase.Exceptions;
using SilverlightPhoneDatabase.Resources;

namespace SilverlightPhoneDatabase
{
    /// <summary>
    /// Database class - contains tables
    /// </summary>
    public class Database
    {
        // ReSharper disable UnusedMember.Local
        private Database() { }
        // ReSharper restore UnusedMember.Local
        private Database(string databaseName, string password, bool useLazyLoading)
        {
            _databaseName = databaseName;
            _password = password;
            _useLazyLoading = useLazyLoading;
            _tables = new ReadOnlyCollection<ITable>(new List<ITable>());
            _loadedTables = new Dictionary<TableDefinition, bool>();
        }

        private readonly string _password = string.Empty;

        private readonly string _databaseName = string.Empty;

        private readonly bool _useLazyLoading;

        // ReSharper disable FieldCanBeMadeReadOnly.Local
        private Dictionary<TableDefinition, bool> _loadedTables;
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        /// <summary>
        /// Database name
        /// </summary>
        public string DatabaseName { get { return _databaseName; } }

        /// <summary>
        /// Password that the database is encrypted with.
        /// Blank if no password is used
        /// </summary>
        public string Password { get { return _password; } }

        private ReadOnlyCollection<ITable> _tables;
        /// <summary>
        /// List of tables that belong to this database
        /// </summary>
        public ReadOnlyCollection<ITable> Tables
        {
            get
            {
                return _tables;
            }
        }

        /// <summary>
        /// Create new database instance with specified name
        /// </summary>
        /// <param name="databaseName">Database name</param>
        /// <returns></returns>
        public static Database CreateDatabase(string databaseName)
        {
            return CreateDatabase(databaseName, string.Empty);
        }

        /// <summary>
        /// Create new database instance with specified name
        /// </summary>
        /// <param name="databaseName">Database name</param>
        /// <param name="password">Password to encrypt database with</param>
        /// <returns></returns>
        public static Database CreateDatabase(string databaseName, string password)
        {
            if (DoesDatabaseExists(databaseName))
            {
                throw new DatabaseExistsException(string.Format(DatabaseResources.DatabaseExistsExceptionText, databaseName));
            }
            return new Database(databaseName, password, false);
        }



        /// <summary>
        /// Open database from Isolated Storage
        /// </summary>
        /// <param name="databaseName">Name of database to open</param>
        /// <exception cref="SilverlightPhoneDatabase.Exceptions.OpenException">Thrown when an error occurs</exception>
        /// <returns>Instance of the database</returns>
        public static Database OpenDatabase(string databaseName)
        {
            return OpenDatabase(databaseName, string.Empty, false);
        }

        /// <summary>
        /// Open database from Isolated Storage
        /// </summary>
        /// <param name="databaseName">Name of database to open</param>
        /// <param name="password">Password to use for encryption</param>
        /// <exception cref="SilverlightPhoneDatabase.Exceptions.OpenException">Thrown when an error occurs</exception>
        /// <returns>Instance of the database</returns>
        public static Database OpenDatabase(string databaseName, string password)
        {
            return OpenDatabase(databaseName, password, false);
        }

        /// <summary>
        /// Open database from Isolated Storage
        /// </summary>
        /// <param name="databaseName">Name of database to open</param>
        /// <param name="password">Password to use for encryption</param>
        /// <param name="useLazyLoading">If true, tables are not open immediately, but instead loaded on demand when accessed</param>
        /// <exception cref="SilverlightPhoneDatabase.Exceptions.OpenException">Thrown when an error occurs</exception>
        /// <returns>Instance of the database</returns>
        public static Database OpenDatabase(string databaseName, string password, bool useLazyLoading)
        {
            if (!DoesDatabaseExists(databaseName))
            {
                throw new DatabaseDoesNotExistsException(string.Format(DatabaseResources.DatabaseDoesNotExistsExceptionText, databaseName));
            }
            try
            {
                var returnValue = new Database(databaseName, password, useLazyLoading);
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string[] parts;
                    using (var stream = new IsolatedStorageFileStream(databaseName, FileMode.Open, store))
                    {
                        var reader = new StreamReader(stream);
                        string content = reader.ReadToEnd();
                        if (!string.IsNullOrEmpty(password))
                        {
                            content = Cryptography.Decrypt(content, password);
                        }
                        parts = content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    }

                    for (int counter = 1; counter < parts.Length; counter++)
                    {
                        string rowTypeString = parts[counter];
                        var tableDefinition = Utility.GetTableDefintion(rowTypeString);
                        var tableType = typeof(Table<>).MakeGenericType(new[] { tableDefinition.TableRowType });
                        if (!useLazyLoading)
                        {
                            Debug.Assert(tableDefinition.TableRowType != null, "Table row type is not defined.");
                            string fileName = Utility.GetFileName(databaseName, tableDefinition.TableRowType, tableDefinition.TableName);
                            using (var tableStream = new IsolatedStorageFileStream(fileName, FileMode.Open, store))
                            {
                                returnValue.LoadTable(tableStream, tableType, tableDefinition.TableName);
                            }
                        }
                        else
                        {
                            var currentTable = (ITable)Activator.CreateInstance(tableType);
                            currentTable.SetTableDefinition(returnValue._databaseName, returnValue._password, tableDefinition.TableName);
                            // ReSharper disable UseObjectOrCollectionInitializer
                            var tables = new List<ITable>(returnValue._tables);
                            // ReSharper restore UseObjectOrCollectionInitializer
                            tables.Add(currentTable);
                            returnValue._tables = new ReadOnlyCollection<ITable>(tables);
                            returnValue._loadedTables.Add(tableDefinition, false);
                        }
                    }
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                throw new OpenException(ex);
            }

        }


        internal void LoadTable(IsolatedStorageFileStream tableStream, Type tableType, string tableName)
        {
            string content;
            using (var reader = new StreamReader(tableStream))
            {
                content = reader.ReadToEnd();

                if (!string.IsNullOrEmpty(_password))
                {
                    content = Cryptography.Decrypt(content, _password);
                }
            }

            using (var stringReader = new StringReader(content))
            {
                var serializer = new XmlSerializer(tableType);
                ITable table;
                if (!string.IsNullOrEmpty(content))
                {
                    table = (ITable)serializer.Deserialize(stringReader);
                }
                else
                {
                    table = (ITable)Activator.CreateInstance(tableType);
                }
                table.SetTableDefinition(_databaseName, _password, tableName);

                var tables = new List<ITable>(_tables);
                var tableToRemove = (from oneTable in tables
                                     where oneTable.RowType == table.RowType && oneTable.TableName == tableName
                                     select oneTable).FirstOrDefault();
                tables.Remove(tableToRemove);
                tables.Add(table);
                _tables = new ReadOnlyCollection<ITable>(tables);
            }
        }

        /// <summary>
        /// Create new table from predefined table content
        /// This content must be created using the same table type
        /// using XmlSerializer without encryption
        /// </summary>
        /// <param name="xmlTableContent">Xml string that describes the table</param>
        /// <param name="rowType">Type of a row in the table</param>
        /// <param name="tableName">Name of the table to create</param>
        public void CreateNewTable(string xmlTableContent, Type rowType, string tableName = "")
        {
            using (var stringReader = new StringReader(xmlTableContent))
            {
                var tableType = typeof(Table<>).MakeGenericType(new[] { rowType });
                var serializer = new XmlSerializer(tableType);
                var table = (ITable)serializer.Deserialize(stringReader);
                table.SetTableDefinition(_databaseName, _password, tableName);

                // ReSharper disable UseObjectOrCollectionInitializer
                var tables = new List<ITable>(_tables);
                // ReSharper restore UseObjectOrCollectionInitializer
                tables.Add(table);
                _tables = new ReadOnlyCollection<ITable>(tables);
            }
        }

        internal void LoadTable(string content, Type tableType, string tableName = "")
        {

            using (var stringReader = new StringReader(content))
            {
                var serializer = new XmlSerializer(tableType);
                var table = (ITable)serializer.Deserialize(stringReader);
                table.SetTableDefinition(_databaseName, _password);

                // ReSharper disable SuggestUseVarKeywordEvident
                List<ITable> tables = new List<ITable>(_tables);
                // ReSharper restore SuggestUseVarKeywordEvident
                var tableToRemove = (from oneTable in tables
                                     where oneTable.RowType == table.RowType && oneTable.TableName == tableName
                                     select oneTable).FirstOrDefault();
                tables.Remove(tableToRemove);
                tables.Add(table);
                _tables = new ReadOnlyCollection<ITable>(tables);
            }
        }

        /// <summary>
        /// Delete database
        /// </summary>
        /// <param name="databaseName">Name of database to delete</param>
        public static void DeleteDatabase(string databaseName)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string[] files = store.GetFileNames();
                if (files.Length > 0 && files[0] != null)
                {
                    foreach (var file in files)
                    {
                        if (file == databaseName)
                            store.DeleteFile(file);
                        else if (file.StartsWith(databaseName + "."))
                            store.DeleteFile(file);
                    }
                }
            }
        }

        /// <summary>
        /// Create new table inside database
        /// </summary>
        /// <typeparam name="T">Type of object that this table contains</typeparam> 
        /// <param name="tableName">Table name (optional) to create</param>
        public void CreateTable<T>(string tableName = "")
        {
            if (DoesTableExists(typeof(T), tableName))
            {
                throw new DatabaseExistsException(string.Format(DatabaseResources.TableExistsExceptionText, typeof(T).FullName));
            }
            // ReSharper disable UseObjectOrCollectionInitializer
            var tables = new List<ITable>(_tables);
            // ReSharper restore UseObjectOrCollectionInitializer
            tables.Add(SilverlightPhoneDatabase.Table<T>.CreateTable(_databaseName, _password, tableName));
            _tables = new ReadOnlyCollection<ITable>(tables);
        }

        /// <summary>
        /// FInd instance of a table that contains specific row type
        /// </summary>
        /// <typeparam name="T">Type of object that this table contains</typeparam>
        /// <param name="tableName">Name of a table to open.  This is optional parameter.
        /// It can be used if you want to have multiple tables that store the same data
        /// </param>
        /// <returns></returns>
        public Table<T> Table<T>(string tableName = "")
        {
            var returnValue = (from oneTable in Tables
                               where oneTable.RowType == typeof(T) && oneTable.TableName == tableName
                               select oneTable).FirstOrDefault();
            var tableDefinition = new TableDefinition(typeof(T), tableName);
            if (_useLazyLoading && !_loadedTables[tableDefinition])
            {
                Type tableType = typeof(Table<>).MakeGenericType(new[] { typeof(T) });
                string fileName = string.Concat(_databaseName, ".", typeof(T).FullName);
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var tableStream = new IsolatedStorageFileStream(fileName, FileMode.Open, store))
                    {
                        LoadTable(tableStream, tableType, tableName);
                    }
                }
                _loadedTables[tableDefinition] = true;
                returnValue = (from oneTable in Tables
                               where oneTable.RowType == typeof(T) && oneTable.TableName == tableName
                               select oneTable).FirstOrDefault();
            }
            return (Table<T>)returnValue;

        }

        /// <summary>
        /// Cancel the pending changes to a table.
        /// </summary>
        /// <typeparam name="T">Type of class that table contains</typeparam>
        public void CancelChanges<T>(string tableName = "")
        {
            var tableDefinition = new TableDefinition(typeof(T), tableName);
            if (_useLazyLoading)
            {
                if (_loadedTables[tableDefinition])
                {
                    ReloadTable<T>(tableName);
                }
            }
            else
            {
                ReloadTable<T>(tableName);
            }
        }

        private void ReloadTable<T>(string tableName = "")
        {
            var tableType = typeof(Table<>).MakeGenericType(new[] { typeof(T) });
            var fileName = Utility.GetFileName(_databaseName, typeof(T), tableName);
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var tableStream = new IsolatedStorageFileStream(fileName, FileMode.Open, store))
                {
                    LoadTable(tableStream, tableType, tableName);
                }
            }
        }

        /// <summary>
        /// Save database and all tables within it to Isolated Storage
        /// </summary>
        /// <exception cref="SilverlightPhoneDatabase.Exceptions.SaveException">Thrown when an error occurs during save</exception>
        public void Save()
        {
            try
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {

                    if (store.FileExists(_databaseName))
                    {
                        store.DeleteFile(_databaseName);
                    }

                    using (var stream = new IsolatedStorageFileStream(_databaseName, FileMode.OpenOrCreate, store))
                    {

                        WriteDatabaseToStream(stream);
                    }
                }
                foreach (var item in Tables)
                {
                    if (_useLazyLoading)
                    {
                        var tableDefinition = new TableDefinition(item.RowType, item.TableName);
                        if (_loadedTables[tableDefinition])
                        {
                            item.Save();
                        }
                    }
                    else
                    {
                        item.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SaveException(ex);
            }
        }


        /// <summary>
        /// Asynchronously save table le to the Isolated Storage
        /// </summary>
        /// <param name="callback">Function  to be invoked after save complete</param>
        public void BeginSave(Action<SaveResult> callback)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += WorkerDoWork;
            worker.RunWorkerCompleted += (o, e) =>
            {
                SaveResult result;
                if (e.Result is Exception)
                {
                    result = new SaveResult(e.Result as Exception);
                }
                else
                {
                    result = new SaveResult(null);
                }
                callback(result);
            };
            worker.RunWorkerAsync();
        }

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Save();
                e.Result = null;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        /// <summary>
        /// Write content of a database to a stream
        /// </summary>
        /// <param name="stream">Stream to write database to</param>
        public void WriteDatabaseToStream(Stream stream)
        {
            string serilizedInfo = _databaseName;
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var item in _tables)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                serilizedInfo = string.Concat(
                    serilizedInfo,
                    Environment.NewLine,
                    Utility.CreateFormattedTableType(item.RowType, item.TableName));
            }
            if (!string.IsNullOrEmpty(_password))
            {
                serilizedInfo = Cryptography.Encrypt(serilizedInfo, _password);
            }


            using (var writer = new StreamWriter(stream))
            {
                writer.Write(serilizedInfo);
                writer.Flush();
            }
        }


        #region Exists
        /// <summary>
        /// Check if table that stores data of specified row type exists
        /// </summary>
        /// <param name="rowType">Type to store in this table</param>
        /// <param name="tableName">Name of the table</param>
        /// <returns>True if table exists, false otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when rowType parameter is null</exception>
        public bool DoesTableExists(Type rowType, string tableName = "")
        {
            if (rowType == null) throw new ArgumentNullException("rowType");

            bool returnValue = false;
            foreach (var item in Tables)
            {
                if (item.RowType == rowType && item.TableName == tableName)
                {
                    returnValue = true;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Checks for existence of a database
        /// </summary>
        /// <param name="databaseName">Database name to check</param>
        /// <returns>True if database exists, false otherwise</returns>
        public static bool DoesDatabaseExists(string databaseName)
        {
            bool returnValue = false;
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string[] files = store.GetFileNames();
                if (files.Length > 0 && files[0] != null)
                {
                    returnValue = (from aFile in files
                                   where aFile == databaseName
                                   select aFile).Any();
                }
            }
            return returnValue;
        }
        #endregion
    }
}
