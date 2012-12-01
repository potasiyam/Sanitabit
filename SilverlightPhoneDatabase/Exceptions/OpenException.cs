using System;
using SilverlightPhoneDatabase.Resources;

namespace SilverlightPhoneDatabase.Exceptions
{
    /// <summary>
    /// Open exception class
    /// </summary>
    public class OpenException : Exception
    {
        /// <summary>
        /// Create new instance of OpenException
        /// </summary>
        /// <param name="innerException">Inner Exception</param>
        public OpenException(Exception innerException)
            : base(DatabaseResources.OpenError,innerException) { }
    }
}
