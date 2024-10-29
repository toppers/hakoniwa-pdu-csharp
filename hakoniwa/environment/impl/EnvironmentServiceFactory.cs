using System;
using hakoniwa.environment.impl.local;
using hakoniwa.environment.interfaces;

namespace hakoniwa.environment.impl
{
    public class EnvironmentServiceFactory
    {
        static public IEnvironmentService Create()
        {
            return new EnvironmentService();
        }
    }
    public class EnvironmentService : IEnvironmentService
    {
        IFileLoader file_loader;
        public EnvironmentService()
        {
            file_loader = new LocalFileLoader();
        }
        public IFileLoader GetFileLoader()
        {
            return file_loader;
        }
    }

}
