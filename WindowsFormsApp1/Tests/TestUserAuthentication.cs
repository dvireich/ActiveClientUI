using System;
using System.ServiceModel;
using WindowsFormsApp1.Authentication;

namespace WindowsFormsApp1
{
    class TestUserAuthentication
    {
        static IAuthentication authenticationService;
        private const string clientType = "ActiveClient";
        public TestUserAuthentication()
        {
            initializeServiceReferences();
        }
        private static void initializeServiceReferences()
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
            var shellAdress = string.Format("http://localhost:80/ShellTrasferServer/Authentication");
            var shellUri = new Uri(shellAdress);
            var shellEndpointAddress = new EndpointAddress(shellUri);
            var shellChannel = new ChannelFactory<IAuthentication>(shellBinding, shellEndpointAddress);
            authenticationService = shellChannel.CreateChannel();
        }

        public static void Test1()
        {
            var resp1 = authenticationService.SignUp(new SignUpRequest()
            {
                userName = "dvir",
                password = "1234"
            });
        }
        
        public static void Test2()
        {
            var resp2 = authenticationService.AuthenticateAndSignIn(new AuthenticateAndSignInRequest()
            {
                userName = "dvir",
                password = "1234",
                userType  = clientType
            });
        }

        public static void Test3()
        {
            var resp3 = authenticationService.ChangeUserPassword(new ChangeUserPasswordRequest()
            {
                userName = "dvir",
                oldPassword = "1234",
                newPassword = "12345"
            });
        }

        public static void Test4()
        {
            var resp4 = authenticationService.AuthenticateAndSignIn(new AuthenticateAndSignInRequest()
            {
                userName = "dvir",
                password = "12345",
                userType = clientType
            });
        }

        public static void Test5()
        {
            var resp5 = authenticationService.SignUp(new SignUpRequest()
            {
                userName = "dvir",
                password = "12347"
            });
        }

        public static void Test6()
        {
            var resp6 = authenticationService.SetSecurityQuestionAndAnswer(new SetSecurityQuestionAndAnswerRequest()
            {
                userName = "dvir",
                password = "12345",
                answer = "dvir",
                question = "reich"
            });
        }

        public static void Test7()
        {
            var resp7 = authenticationService.GetSecurityQuestion(new GetSecurityQuestionRequest()
            {
                userName = "dvir"
            });
        }

        public static void Test8()
        {
            var resp8 = authenticationService.RestorePasswordFromUserNameAndSecurityQuestion(new RestorePasswordFromUserNameAndSecurityQuestionRequest()
            {
                userName = "dvir",
                answer = "reich"
            });
        }

        

        

        


        

        

        

        


    }
}
