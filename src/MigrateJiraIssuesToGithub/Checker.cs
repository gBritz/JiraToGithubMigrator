using System;

namespace MigrateJiraIssuesToGithub
{
    internal class Checker
    {
        public static void IsNull(object value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        public static void IsNull(string value, string parameterName)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}