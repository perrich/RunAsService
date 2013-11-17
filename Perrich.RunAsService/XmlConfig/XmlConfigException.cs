using System;
using System.Runtime.Serialization;

namespace Perrich.RunAsService.XmlConfig
{
    /// <summary>
    /// Exception raised when we try to get a configuration item from an XML and the expected type is not verified 
    /// </summary>
    public class XmlConfigException : InvalidOperationException
    {
        public XmlConfigException()
        {
        }

        public XmlConfigException(string message)
            : base(message)
        {
        }

        public XmlConfigException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected XmlConfigException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}