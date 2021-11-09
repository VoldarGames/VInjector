using System;

namespace VoldarGames.NetCore.VInjector.Exceptions
{
    public class UnRegisteredTypeVInjectorException : Exception
    {
        public UnRegisteredTypeVInjectorException(Type interfaceType, string registrationName) : base($"Interface {interfaceType.Name} {RegistrationNameMessage(registrationName)}is not registered") { }

        private static string RegistrationNameMessage(string registrationName)
        {
            return registrationName != null ? $"with registration name {registrationName} " : string.Empty;
        }
    }
}