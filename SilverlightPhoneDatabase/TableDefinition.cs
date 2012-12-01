using System;


namespace SilverlightPhoneDatabase
{
    /// <summary>
    /// Contains definition of a table
    /// </summary>
    internal class TableDefinition
    {
        internal  TableDefinition(Type tableType, string tableName)
        {
            TableRowType = tableType;
            TableName = tableName;
        }
        /// <summary>
        /// Type of a row in a table
        /// </summary>
        internal Type TableRowType { get; private set; }

        /// <summary>
        /// Table name
        /// </summary>
        internal string TableName { get; private set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (TableDefinition)) return false;
            return Equals((TableDefinition) obj);
        }

        public bool Equals(TableDefinition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.TableRowType, TableRowType) && Equals(other.TableName, TableName);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((TableRowType != null ? TableRowType.GetHashCode() : 0)*397) ^ (TableName != null ? TableName.GetHashCode() : 0);
            }
        }
    }
}
