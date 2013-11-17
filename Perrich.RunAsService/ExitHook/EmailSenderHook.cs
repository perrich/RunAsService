using System;
using System.Net;
using System.Net.Mail;
using log4net;

namespace Perrich.RunAsService.ExitHook
{
    /// <summary>
    /// Send an email when an unwanted exit is detected
    /// </summary>
    public class EmailSenderHook : AbstractExitHook, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(EmailSenderHook));

        protected bool Disposed
        {
            get;
            private set;
        }

        public SmtpClient Client { get; protected set; }
        public MailMessage Message { get; protected set; }

        protected override bool Configure(XmlConfig.XmlConfig settings)
        {
            String rootXPath = string.Format("email{0}smtp{0}", XmlConfig.XmlConfig.XPathSeparator);
            String host = settings.GetItem(rootXPath + "host").Value;
            String port = settings.GetItem(rootXPath + "port").Value;
            String login = settings.GetItem(rootXPath + "login").Value;
            String password = settings.GetItem(rootXPath + "password").Value;

            // smtp client is mandatory!
            if (!CreateSmtpClient(host, port, login, password))
            {
                return false;
            }

            rootXPath = string.Format("email{0}address{0}", XmlConfig.XmlConfig.XPathSeparator);
            String toAddress = settings.GetItem(rootXPath + "to").Value;
            String fromAddress = settings.GetItem(rootXPath + "from").Value;
            rootXPath = string.Format("email{0}subject", XmlConfig.XmlConfig.XPathSeparator);
            String subject = settings.GetItem(rootXPath).Value;
            String executable = settings.GetItem("executable").Value;

            // message is mandatory too!
            return PrepareMessage(toAddress, fromAddress, subject, executable, Service.DisplayName);
        }

        private bool CreateSmtpClient(String host, String port, String login, String password)
        {
            if (String.IsNullOrEmpty(host))
            {
                Log.Warn("Cannot send an email: the smtp host is unknown.");
                return false;
            }

            Client = new SmtpClient(host);
            if (!String.IsNullOrEmpty(login) && !String.IsNullOrEmpty(password))
            {
                Client.Credentials = new NetworkCredential(login, password);
            }
            if (!String.IsNullOrEmpty(port))
            {
                int portInt;
                if (!Int32.TryParse(port, out portInt))
                {
                    Log.Warn("Cannot send an email: the smtp port is not well defined");
                    return false;
                }
                Client.Port = portInt;
            }

            return true;
        }


        private bool PrepareMessage(String toAddress, String fromAddress, String subject, String executable, String serviceName)
        {
            if (String.IsNullOrEmpty(toAddress))
            {
                Log.Warn("Cannot send an email: the recipient address is empty.");
                return false;
            }

            if (String.IsNullOrEmpty(fromAddress))
            {
                Log.Warn("Cannot send an email: the caller address is empty.");
                return false;
            }

            if (String.IsNullOrEmpty(executable))
            {
                Log.Warn("Cannot send an email: the executable is empty.");
                return false;
            }

            // we always need a subject, so set a default value if not available
            if (String.IsNullOrEmpty(subject))
            {
                subject = "Warning: {0} [Service] - Process has exited!";
            }

            Message = new MailMessage
            {
                From = new MailAddress(fromAddress),
                Subject = String.Format(subject, serviceName),
                IsBodyHtml = true,
                Body = "<html><body>" +
                       "Executed command : <b>" + executable + "</b><br><br>" +
                       "This process may have been killed.<br>" +
                       "If necessary, please restart the &quot;" + serviceName + "&quot; service.<br>" +
                       "</body></html>"
            };


            foreach (var s in toAddress.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                Message.To.Add(s);
            }

            return true;
        }

        protected override bool Execute()
        {
            if (Client == null || Message == null)
            {
                Log.Debug("email can't be sent: not well configured!");
                return false;
            }

            try
            {
                Client.Send(Message);
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(string.Format("Send a mail (Server: {0}, to:{1})", Client.Host, Message.To));
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Alert mail can't be sent: {0}", ex.Message), ex);
            }

            return false;
        }

        public void Dispose()
        {
            if (Message == null) return;
            Message.Dispose();
            Message = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Dispose();
                }

                Disposed = true;
            }
        }
    }
}
