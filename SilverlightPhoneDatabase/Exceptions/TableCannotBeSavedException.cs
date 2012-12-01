using System;

namespace SilverlightPhoneDatabase.Exceptions
{
    /// <summary>
    /// Exception that is thrown when stand-alone table is being
    /// subject to save operation
    /// </summary>
    public class TableCannotBeSavedException : Exception
    {
        /// <summary>
        /// Create new instance of TableCannotBeSavedException exception
        /// </summary>
        /// <param name="exceptionText">Exception text</param>
        public TableCannotBeSavedException(string exceptionText)
            : base(exceptionText) { }

    }
}
