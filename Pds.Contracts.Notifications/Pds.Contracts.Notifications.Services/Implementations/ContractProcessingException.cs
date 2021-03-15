﻿using System;
using System.Runtime.Serialization;

namespace Pds.Contracts.Notifications.Services.Implementations
{
    /// <summary>
    /// Indicates that all contracts process failed to complete.
    /// </summary>
    [Serializable]
    internal class ContractProcessingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractProcessingException"/> class.
        /// </summary>
        public ContractProcessingException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractProcessingException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ContractProcessingException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractProcessingException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ContractProcessingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractProcessingException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected ContractProcessingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}