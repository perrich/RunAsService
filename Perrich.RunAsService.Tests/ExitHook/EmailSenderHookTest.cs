using System;
using System.Net;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Perrich.RunAsService.ExitHook;

namespace Perrich.RunAsService.Tests.ExitHook
{
    [TestFixture]
    class EmailSenderHookTest
    {
        private const string Localhost = "localhost";
        private const string ToEmail1 = "to@b1.com";
        private const string ToEmail2 = "to@b2.com";
        private const string ToEmail = ToEmail1 + "," + ToEmail2;
        private const string FromEmail = "me@b.com";

        private EmailSenderHook _hook;
        private Perrich.RunAsService.XmlConfig.XmlConfig _settings;
        private IRunAsService _service;

        [SetUp]
        public void Init()
        {
            _service = A.Fake<IRunAsService>();
            _settings = new Perrich.RunAsService.XmlConfig.XmlConfig();
            _hook = new EmailSenderHook();
        }

        [Test]
        public void ShouldHostMandatory()
        {
            _settings.LoadXmlFromString("<configuration></configuration>");
            Assert.False(_hook.Init(_settings, _service));
        }

        [Test]
        public void ShouldBeInitializedWithMandatoryField()
        {
            // executable, smpt/host, email/to and email/from are mandatory
            _settings.LoadXmlFromString(GetConfiguration(null));
            Assert.True(_hook.Init(_settings, _service));
        }

        [Test]
        public void ShouldBeInitializedIfValidPortIsDefined()
        {
            _settings.LoadXmlFromString(GetConfiguration("10"));
            Assert.True(_hook.Init(_settings, _service));
        }

        [Test]
        public void ShouldRejectedIfInvalidPortIsDefined()
        {
            _settings.LoadXmlFromString(GetConfiguration("a"));
            Assert.False(_hook.Init(_settings, _service), "");
        }

        [Test]
        public void ShouldRejectedIfToAddressIsMissing()
        {
            _settings.LoadXmlFromString(GetConfiguration(null, toDefined: false));
            Assert.False(_hook.Init(_settings, _service), "");
        }

        [Test]
        public void ShouldRejectedIfFromAddressIsMissing()
        {
            _settings.LoadXmlFromString(GetConfiguration(null, fromDefined: false));
            Assert.False(_hook.Init(_settings, _service), "");
        }

        [Test]
        public void ShouldRejectedIfexecutableIsMissing()
        {
            _settings.LoadXmlFromString(GetConfiguration(null, executableDefined: false));
            Assert.False(_hook.Init(_settings, _service), "");
        }

        [Test]
        public void ShouldInitializeUsingConfiguration()
        {
            const int port = 10;
            const string subject = "My subject";
            const string login = "always_myself";
            const string password = "it's me!";

            _settings.LoadXmlFromString(GetConfiguration(port: port.ToString(), subject: subject, login: login, password: password));
            Assert.True(_hook.Init(_settings, _service));
            _hook.Client.Should().NotBeNull();
            _hook.Client.Port.Should().Be(port);
            _hook.Client.Host.Should().Be(Localhost);
            _hook.Client.Credentials.Should().BeOfType<NetworkCredential>();
            _hook.Client.Credentials.As<NetworkCredential>().UserName.Should().Be(login);
            _hook.Client.Credentials.As<NetworkCredential>().Password.Should().Be(password);

            _hook.Message.Should().NotBeNull();
            _hook.Message.Subject.Should().Be(subject);
            _hook.Message.To.Should().HaveCount(2);
            _hook.Message.To[0].Address.Should().Be(ToEmail1);
            _hook.Message.To[1].Address.Should().Be(ToEmail2);
            _hook.Message.From.Address.Should().Be(FromEmail);
        }

        private static string GetConfiguration(String port, bool executableDefined = true, bool toDefined = true, bool fromDefined = true, String subject = null, String login = null, String password = null)
        {
            var config = "<configuration>";
            if (executableDefined) config += "<executable>a.exe</executable>";
            config += "<email><smtp><host>" + Localhost + "</host>";
            if (port != null) config += "<port>" + port + "</port>";
            if (login != null) config += "<login>" + login + "</login>";
            if (password != null) config += "<password>" + password + "</password>";
            config += "</smtp><address>";
            if (toDefined) config += "<to>" + ToEmail + "</to>";
            if (fromDefined) config += "<from>" + FromEmail + "</from>";
            config += "</address>";
            if (subject != null) config += "<subject>" + subject + "</subject>";
            config += "</email></configuration>";

            return config;
        }
    }
}
