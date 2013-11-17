using FluentAssertions;
using NUnit.Framework;
using Perrich.RunAsService.XmlConfig;
using x = Perrich.RunAsService.XmlConfig;

namespace Perrich.RunAsService.Tests.XmlConfig
{
    [TestFixture]
    class XmlConfigItemTest
    {
        private const string Value = "value";
        
        [Test]
        public void ShouldReturnStringAsValue()
        {
            var item = new XmlConfigItem(Value);
            item.Value.Should().Be(Value);
        }

        [Test]
        public void ShouldReturnIntegerAsValue()
        {
            const int i = 10;
            var item = new XmlConfigItem(i.ToString());
            item.IntValue.Should().Be(i);
        }

        [Test]
        public void ShouldReturnDoubleAsValue()
        {
            const double f = 10.14;
            var item = new XmlConfigItem(f.ToString());
            item.DoubleValue.Should().BeInRange(f, f + 0.001);
        }

        [Test]
        public void ShouldReturnBooleanAsValue()
        {
            const bool b = true;
            var item = new XmlConfigItem(b.ToString());
            item.BoolValue.Should().Be(b);
        }

        [Test]
        [ExpectedException("Perrich.RunAsService.XmlConfig.XmlConfigException")]
        public void ShouldThrowExceptionWhenValueIsNotInteger()
        {
            var item = new XmlConfigItem(Value);
            var v = item.IntValue;
        }

        [Test]
        [ExpectedException("Perrich.RunAsService.XmlConfig.XmlConfigException")]
        public void ShouldThrowExceptionWhenValueIsNotDouble()
        {
            var item = new XmlConfigItem(Value);
            var v = item.DoubleValue;
        }

        [Test]
        [ExpectedException("Perrich.RunAsService.XmlConfig.XmlConfigException")]
        public void ShouldThrowExceptionWhenValueIsNotBoolean()
        {
            var item = new XmlConfigItem(Value);
            var v = item.BoolValue;
        }

        [Test]
        public void ShouldReturnNullAsValueIfCreatedWithNull()
        {
            var item = new XmlConfigItem(null);
            item.Value.Should().BeNull("because created with null");
        }
    }
}
