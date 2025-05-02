namespace DHI.Services.Jobs.Automations.Expressions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class ExpressionParsingException : Exception
    {
        public ExpressionParsingException()
        {
        }

        public ExpressionParsingException(string message) : base(message)
        {
        }

        public ExpressionParsingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExpressionParsingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
