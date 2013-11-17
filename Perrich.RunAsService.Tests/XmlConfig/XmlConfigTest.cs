using FluentAssertions;
using NUnit.Framework;
using x = Perrich.RunAsService.XmlConfig;

namespace Perrich.RunAsService.Tests.XmlConfig
{
    [TestFixture]
    class XmlConfigTest
    {
        private const string BValue = "something";
        private const string CValue = "v";
        private Perrich.RunAsService.XmlConfig.XmlConfig _settings;

        [SetUp]
        public void Init()
        {
            _settings = new x.XmlConfig();
            _settings.LoadXmlFromString("<a><b>" + BValue + "</b><c value=\"" + CValue + "\"/><d foo=\"bar\"/><e value=\"e1\" /><e value=\"e2\" /></a>");
        }

        [Test]
        [ExpectedException("System.Xml.XmlException")]
        public void ShouldNotOpenAnUnknownFile()
        {
            _settings = new x.XmlConfig("unknown.files");
        }

        [Test]
        public void ShouldReturnValueForExistingXmlNodes()
        {
            _settings.GetItem("b").Value.Should().Be(BValue);
            _settings.GetItem("c").Value.Should().Be(CValue);
        }

        [Test]
        public void ShouldReturnEmptyValueForUnknownNode()
        {
            _settings.GetItem("f").Value.Should().BeEmpty();
        }

        [Test]
        public void ShouldReturnEmptyValueForMissingValueAttribute()
        {
            _settings.GetItem("d").Value.Should().BeEmpty();
        }

        [Test]
        public void ShouldAcceptReset()
        {
            _settings.GetItem("/a").Value.Should().NotBeEmpty();
            _settings.ResetXml("a");
            _settings.GetItem("/a").Value.Should().BeEmpty();
        }

        [Test]
        public void ShouldReturnedItemsContainsAllExistingValues()
        {
            var list = _settings.GetItems("e");
            list.Should().NotBeNull();
            list.Should().HaveCount(2);
            list[0].Value.Should().Be("e1");
            list[1].Value.Should().Be("e2");
        }

        [Test]
        public void ShouldReturnedItemsContainsOneValueWhenOnlyOneNodeExists()
        {
            var list = _settings.GetItems("c");
            list.Should().NotBeNull();
            list.Should().HaveCount(1);
            list[0].Value.Should().Be(CValue);
            _settings.GetItem("c").Value.Should().Be(CValue);
        }

        [Test]
        public void ShouldReturnedItemsIsEmptyForUnknownValue()
        {
            var list = _settings.GetItems("f");
            list.Should().NotBeNull();
            list.Should().HaveCount(0);
        }
    }
}
