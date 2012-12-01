using System;

namespace SilverlightPhoneDatabase.Core
{
    /// <summary>
    /// Contains results of asynchronous Save operation
    /// </summary>
    public class SaveResult
    {
        internal SaveResult(Exception error) 
        {
            Error = error;
        }
        /// <summary>
        /// Exception that occurred during save
        /// </summary>
        public Exception Error { get; private set; }
    }
}
