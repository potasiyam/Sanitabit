using System;

namespace SilverlightPhoneDatabase.Exceptions
{
    /// <summary>
    /// Exception thrown when database with the same already exists
    /// </summary>
    public class DatabaseExistsException : Exception
    {
        /// <summary>
        /// Create new instance of DatabaseExistsException
        /// </summary>
        /// <param name="exceptionText">Exception text</param>
        public DatabaseExistsException(string exceptionText)
            : base(exceptionText) { }

    }
}
