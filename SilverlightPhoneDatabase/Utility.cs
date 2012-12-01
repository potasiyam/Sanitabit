using System;

namespace SilverlightPhoneDatabase
{
    /// <summary>
    /// Class to store various utility functions
    /// </summary>
    internal static class Utility
    {
        /// <summary>
        /// Get file name for a table
        /// </summary>
        /// <param name="databaseName">Database name</param>
        /// <param name="tableType">Row type for a table</param>
        /// <param name="tableName">Name of the table</param>
        /// <returns></returns>
        internal static string GetFileName(string databaseName, Type tableType, string tableName)
        {
            string fileName = string.Concat(databaseName, ".", tableType.FullName);
            if (!string.IsNullOrEmpty(tableName))
            {
                fileName = fileName + "[" + tableName + "]";
            }
            return fileName;
        }

        /// <summary>
        /// Parse table definition from stored type
        /// </summary>
        /// <param name="tableDefitionToParse">Definition to parse</param>
        /// <returns>Table definition</returns>
        internal static TableDefinition GetTableDefintion(string tableDefitionToParse)
        {
            string rowType = tableDefitionToParse;
            string tableName = string.Empty;
            if (tableDefitionToParse.Contains("[") && tableDefitionToParse.Contains("]"))
            {
                rowType = tableDefitionToParse.Substring(0, tableDefitionToParse.LastIndexOf("["));
                tableName = tableDefitionToParse.Replace(rowType, string.Empty).Replace("[", "").Replace("]", string.Empty);
            }

            return new TableDefinition(Type.GetType(rowType), tableName);
        }

        /// <summary>
        /// Create a string that represents a table type
        /// </summary>
        /// <param name="rowType">Type of row that is contained within the table</param>
        /// <param name="tableName">Name of the table</param>
        /// <returns>String that represents a table type</returns>
        internal  static string CreateFormattedTableType(Type rowType, string tableName)
        {
            // ReSharper disable PossibleNullReferenceException
            var returnValue = rowType.AssemblyQualifiedName.Substring(0, rowType.AssemblyQualifiedName.LastIndexOf("Version")) + "Version=..., " + rowType.AssemblyQualifiedName.Substring(rowType.AssemblyQualifiedName.LastIndexOf("Culture"));
            if (!string.IsNullOrEmpty(tableName))
            {
                returnValue = returnValue + "[" + tableName + "]";
            }
            // ReSharper restore PossibleNullReferenceException

            return returnValue;
        }
    }
}
