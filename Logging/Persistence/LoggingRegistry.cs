using StructureMap;

namespace Logging
{
    public class LoggingRegistry : Registry
    {
        public LoggingRegistry(string service, string connectionString)
        {
            Scan(r =>
            {
                r.TheCallingAssembly();
                r.WithDefaultConventions();
                For<ILogger>().Use(new Logger(connectionString, service)).Singleton();
            });
        }
    }
}