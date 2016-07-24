using System.IO;
using Ninject;
using SmtpMockWeb.Owins;

namespace SmtpMockWeb.Code
{
    public class Ioc
    {
        private static IKernel _kernel;
        public static void Init()
        {
            _kernel = new StandardKernel();

            _kernel.Bind<ICommonLog>().To<CommonLogger>();

            string configPath = Path.Combine(Helper.GetCurrentDirectory(), "config.json");
            _kernel.Bind<SystemConfig>().ToConstant(SystemConfig.GetFromFile(configPath));            
            _kernel.Bind<IServer>().To<OwinServer>().InSingletonScope();
        }

        public static T Get<T>()
        {
            return _kernel.Get<T>();
        }

        public static void ReBind<T>(T t)
        {
            _kernel.Rebind<T>().ToConstant(t);
        }
    }
}
