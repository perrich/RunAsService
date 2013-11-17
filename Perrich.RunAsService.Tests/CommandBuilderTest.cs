using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Perrich.RunAsService.Process;

namespace Perrich.RunAsService.tests
{
    [TestFixture]
    internal class CommandBuilderTest
    {
        #region Setup/Teardown

        [SetUp]
        public void Init()
        {
            _processManager = A.Fake<IProcessManager>();

            _settings = new XmlConfig.XmlConfig();
            _builder = new CommandBuilder(_processManager);
        }

        #endregion

        private XmlConfig.XmlConfig _settings;
        private const string Executable = "sample.exe";
        private const string Parameters = "param";
        private const string Name = "my sample";
        private const bool KillChildren = true;

        private IProcessManager _processManager;
        private CommandBuilder _builder;

        [Test]
        public void ShouldCreateACommandUsingAllSettings()
        {
            _settings.LoadXmlFromString("<configuration><executable>" + Executable + "</executable><parameters>" +
                                        Parameters + "</parameters><killProcessTree>" + KillChildren +
                                        "</killProcessTree></configuration>");
            var command = _builder.BuildCommand(_settings, Name);

            command.Should().NotBeNull();
            command.Executable.Should().Be(Executable);
            command.Name.Should().Be(Name);
            command.Parameters.Should().Be(Parameters);
            command.KillChildren.Should().Be(KillChildren);
        }

        [Test]
        public void ShouldCreateACommandIfManadatorySettingsAreDefined()
        {
            _settings.LoadXmlFromString("<configuration><executable>" + Executable + "</executable></configuration>");
            var command = _builder.BuildCommand(_settings, Name);

            command.Should().NotBeNull();
            command.Executable.Should().Be(Executable);
            command.Name.Should().Be(Name);
            command.Parameters.Should().BeBlank();
            command.KillChildren.Should().Be(false);
        }

        [Test]
        [ExpectedException("Perrich.RunAsService.XmlConfig.XmlConfigException")]
        public void ShouldExecutableSettingCannotBeEmpty()
        {
            _settings.LoadXmlFromString("<configuration><executable></executable></configuration>");
            _builder.BuildCommand(_settings, Name);
        }

        [Test]
        [ExpectedException("Perrich.RunAsService.XmlConfig.XmlConfigException")]
        public void ShouldExecutableSettingIsMandatory()
        {
            _settings.LoadXmlFromString("<configuration></configuration>");
            _builder.BuildCommand(_settings, Name);
        }

        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public void ShouldThrowExceptionIfSettingsIsNull()
        {
            _builder.BuildCommand(null, Name);
        }
    }
}