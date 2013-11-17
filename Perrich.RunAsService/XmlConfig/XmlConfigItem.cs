namespace Perrich.RunAsService.XmlConfig
{
    public class XmlConfigItem
    {
        private readonly string _value;
        
        public XmlConfigItem(string value)
        {
            _value = value;
        }

        /// <summary>
        /// string  value of the specific Xml configuration item
        /// </summary>
        public string Value
        {
            get
            {
                return _value;
            }
        }

        /// <summary>
        /// int value of the specific Xml configuration item
        /// </summary>
        public int IntValue
        {
            get
            {
                int i;
                if (!int.TryParse(_value, out i))
                {
                    throw new XmlConfigException(string.Format("cannot convert value '{0}' to integer.", _value));
                }
                return i;
            }
        }

        /// <summary>
        /// bool value of the specific Xml configuration item
        /// </summary>
        public bool BoolValue
        {
            get
            {
                bool b;
                if (!bool.TryParse(_value, out b))
                {
                    throw new XmlConfigException(string.Format("cannot convert value '{0}' to boolean.", _value));
                }
                return b;
            }
        }

        /// <summary>
        /// double value of the specific Xml configuration item
        /// </summary>
        public double DoubleValue
        {
            get
            {
                double f;
                if (!double.TryParse(_value, out f))
                {
                    throw new XmlConfigException(string.Format("cannot convert value '{0}' to double.", _value));
                }
                return f;
            }
        }
    }
}
