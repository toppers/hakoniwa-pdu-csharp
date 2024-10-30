using System;
namespace hakoniwa.environment.interfaces
{
    public interface IEnvironmentService
    {
        IFileLoader GetFileLoader();
        ICommunicationService GetCommunication();
    }
}
