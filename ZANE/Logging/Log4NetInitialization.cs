using System.IO;
using ZANE.Logging.Enums;

namespace ZANE.Logging;

public class Log4NetInitialization
{
    public static string _log4NetConfigName = "log4net.config";
    public static string _log4NetPath = $"{Environment.CurrentDirectory}\\{_log4NetConfigName}";
    public static List<Log4NetConfigModel> Log4NetConfigs { get; set; } = new List<Log4NetConfigModel>();
    public static void SaveLog4NetXmlConfig() => File.WriteAllText($".\\{_log4NetConfigName}", Log4NetAssembleXml.WriteLog4NetXml(Log4NetConfigs));
    public static void Log4NetXmlLoadConfiguration()
    {
        var r = log4net.Config.XmlConfigurator.Configure(new FileInfo(_log4NetPath));
        if (r.Count != 0)
        {
            Log4NetHelper.LogRun($"[Log4Net加载有错误] {string.Join(",", r)}", Log4NetLevelEnum.错误);
        }
    }
}
