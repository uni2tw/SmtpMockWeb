using log4net;

namespace SmtpMockWeb.Code
{
    public interface ICommonLog
    {
        void Log(string message);
    }

    public class CommonLogger : ICommonLog
    {
        private ILog logger = LogManager.GetLogger("default");
        public void Log(string message)
        {
            logger.Info(message);
        }
    }

}
