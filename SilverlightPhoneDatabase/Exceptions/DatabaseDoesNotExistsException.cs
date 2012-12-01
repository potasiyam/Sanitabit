using System;

namespace SilverlightPhoneDatabase.Exceptions
{
    /// <summary>
    /// Exception that indicates database open error
    /// </summary>
    public class DatabaseDoesNotExistsException : Exception
    {
        /// <summary>
        /// Creates new instance of DatabaseDoesNotExistsException exception
        /// </summary>
        /// <param name="exceptionText">Exception text</param>
        public DatabaseDoesNotExistsException(string exceptionText)
            : base(exceptionText) { }

    }
}
