using System.Xml;

namespace ZANE.Logging;

/// <summary>
/// 组装XML
/// </summary>
internal class Log4NetAssembleXml
{
    public static string WriteLog4NetXml(List<Log4NetConfigModel> list)
    {
        XmlDocument document = new XmlDocument();
        document.AppendChild(document.CreateXmlDeclaration("1.0", "utf-8", null));
        XmlElement configuration = document.CreateElement("configuration");
        #region configSections
        XmlElement configSections = document.CreateElement("configSections");
        XmlElement section = document.CreateElement("section");
        section.SetAttribute("name", "log4net");
        section.SetAttribute("type", "log4net.Config.Log4NetConfigurationSectionHandler,log4net");
        configSections.AppendChild(section);
        configuration.AppendChild(configSections);
        #endregion
        XmlElement log4net = document.CreateElement("log4net");
        WriteNode(list, document, log4net);
        configuration.AppendChild(log4net);
        document.AppendChild(configuration);
        return document.InnerXml;
    }

    private static void WriteNode(List<Log4NetConfigModel> list, XmlDocument document, XmlElement log4net)
    {
        foreach (var item in list)
        {
            XmlElement logger = document.CreateElement("logger");
            XmlElement appender = document.CreateElement("appender");
            XmlElement appender_ref = document.CreateElement("appender-ref");
            logger.SetAttribute("name", item.LoggerName);
            appender_ref.SetAttribute("ref", $"{item.LoggerName.ToArray()[0].ToString().ToUpper()}{item.LoggerName.Substring(1)}Appender");
            logger.InnerXml = appender_ref.OuterXml;
            appender.SetAttribute("name", $"{item.LoggerName.ToArray()[0].ToString().ToUpper()}{item.LoggerName.Substring(1)}Appender");
            appender.SetAttribute("type", "ZANE.Logging.MyRollingFileAppender");
            #region 参数
            XmlElement file = document.CreateElement("file");
            XmlElement appenderToFile = document.CreateElement("appendToFile");
            XmlElement maxSizeRollBackups = document.CreateElement("maxSizeRollBackups");
            XmlElement maximumFileSize = document.CreateElement("maximumFileSize");
            XmlElement rollingStyle = document.CreateElement("rollingStyle");
            XmlElement header = document.CreateElement("header");
            XmlElement datePattern = document.CreateElement("datePattern");
            XmlElement staticLogFileName = document.CreateElement("staticLogFileName");
            XmlElement layout = document.CreateElement("layout");
            XmlElement conversionPattern = document.CreateElement("conversionPattern");
            XmlElement param = document.CreateElement("param");
            param.SetAttribute("name", "Encoding");
            param.SetAttribute("value", "utf-8");
            file.SetAttribute("value", item.SavePath);
            appenderToFile.SetAttribute("value", "true");
            maxSizeRollBackups.SetAttribute("value", item.MaxSizeRollBackups.ToString());
            maximumFileSize.SetAttribute("value", item.MaxFileSize);
            rollingStyle.SetAttribute("value", "Date");
            header.SetAttribute("value", item.Header);
            datePattern.SetAttribute("value", item.DatePattern);
            staticLogFileName.SetAttribute("value", "false");
            conversionPattern.SetAttribute("value", item.ConversionPattern.ToString());

            layout.SetAttribute("type", "log4net.Layout.PatternLayout");
            layout.AppendChild(header);
            layout.AppendChild(conversionPattern);
            appender.AppendChild(param);
            appender.AppendChild(file);
            appender.AppendChild(appenderToFile);
            appender.AppendChild(maxSizeRollBackups);
            appender.AppendChild(maximumFileSize);
            appender.AppendChild(rollingStyle);
            appender.AppendChild(datePattern);
            appender.AppendChild(staticLogFileName);
            appender.AppendChild(layout);
            #endregion
            log4net.AppendChild(logger);
            log4net.AppendChild(appender);
        }
    }
}
