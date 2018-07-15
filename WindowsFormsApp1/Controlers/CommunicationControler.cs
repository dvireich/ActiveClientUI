using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.Authentication;
using WindowsFormsApp1.LoadUser;
using WindowsFormsApp1.ServiceReference1;

namespace WindowsFormsApp1
{
    public class CommunicationControler : IDisposable
    {
        public IActiveShell _proxyService;
        public IAuthentication _authenticationProxy;
        public ILoadUser _loadUserProxy;

        internal CommunicationControler(string endpointId)
        {
            if (endpointId != null)
            {
                _proxyService = InitializeServiceReferences<IActiveShell>($"ActiveShell/{endpointId}");
            }
            _authenticationProxy = InitializeServiceReferences<IAuthentication>("Authentication");
            _loadUserProxy = InitializeServiceReferences<ILoadUser>("LoadUser");
        }

        public static T InitializeServiceReferences<T>(string wcfEndpointId)
        {
            //Confuguring the Shell service
            var shellBinding = new BasicHttpBinding();
            shellBinding.Security.Mode = BasicHttpSecurityMode.None;
            shellBinding.CloseTimeout = TimeSpan.MaxValue;
            shellBinding.ReceiveTimeout = TimeSpan.MaxValue;
            shellBinding.SendTimeout = new TimeSpan(0, 0, 10, 0, 0);
            shellBinding.OpenTimeout = TimeSpan.MaxValue;
            shellBinding.MaxReceivedMessageSize = int.MaxValue;
            shellBinding.MaxBufferPoolSize = int.MaxValue;
            shellBinding.MaxBufferSize = int.MaxValue;
            //Put Public ip of the server copmuter
            var shellAdress = string.Format("http://localhost:80/ShellTrasferServer/{0}", wcfEndpointId);
            var shellUri = new Uri(shellAdress);
            var shellEndpointAddress = new EndpointAddress(shellUri);
            var shellChannel = new ChannelFactory<T>(shellBinding, shellEndpointAddress);
            return shellChannel.CreateChannel();
        }

        private void CloseAllConnections()
        {
            if ((_proxyService is ICommunicationObject ps) && ps.State == CommunicationState.Opened)
            {
                ps.Close();
            }
            if ((_authenticationProxy is ICommunicationObject ap) && ap.State == CommunicationState.Opened)
            {
                ap.Close();
            }
        }

        public void Dispose()
        {
            CloseAllConnections();
        }
    }
}
