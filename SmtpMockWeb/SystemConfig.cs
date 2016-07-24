using System.IO;
using System.Text;
using SmtpMockWeb.Code;

namespace SmtpMockWeb
{
    public class SystemConfig
    {
        private ICommonLog logger = Ioc.Get<ICommonLog>();
        public static SystemConfig GetFromFile(string filePath)
        {
            SystemConfig config = JsonUtil.Deserialize<SystemConfig>(File.ReadAllText(filePath, Encoding.UTF8));
            return config;
        }

        public int Port { get; set; }
        public string RemoteIp { get; set; }
        public SmtpSetting Smtp { get; set; }
        public SmtpMockServerSetting SmtpMockServer { get; set; }

        public void Update()
        {
            string configFile = Path.Combine(Helper.GetCurrentDirectory(), "config.json");
            logger.Log("更新設定檔 " + configFile);
            File.WriteAllText(configFile, JsonUtil.Serialize(this));
        }
    }

    public class SmtpSetting
    {
        public string Host { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public bool Enabled { get; set; }
    }

    public class SmtpMockServerSetting
    {
        public string HostIp { get; set; }
        public int Port { get; set; }
    }
}