using System;
using System.Collections.Generic;
using System.Text;

namespace QhitChat_Server.Core
{
    class CodeDefinition
    {
        public static string ErrorPrefix = "Err_";

        public static class Authentication
        {
            public static string Logged = $"{ErrorPrefix}Logged";
            public static string NoUser = $"{ErrorPrefix}No_User";
            public static string WrongPassword = $"{ErrorPrefix}Wrong_Password";
        }
    }
}
