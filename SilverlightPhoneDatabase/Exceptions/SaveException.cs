using System;
using System.Net;
using SilverlightPhoneDatabase.Resources;

namespace SilverlightPhoneDatabase.Exceptions
{
    /// <summary>
    /// Save exception class
    /// </summary>
    public class SaveException : Exception
    {
        /// <summary>
        /// Create new instance of SaveException
        /// </summary>
        /// <param name="innerException">Inner Exception</param>
        public SaveException(Exception innerException)
            : base(DatabaseResources.SaveError,innerException) { }
    }
}
