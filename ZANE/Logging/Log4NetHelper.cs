using log4net;
using ZANE.Logging.Enums;
using ZANE.Logging.Models;

namespace ZANE.Logging;

public static class Log4NetHelper
{
    public static Action<string, Log4NetLevelEnum>? PromptAction { get; set; }
    public static Action<string, Log4NetLevelEnum, Log4NetTypeEnum>? DisplayRunLogAction { get; set; }

    private static ILog _runLog = LogManager.GetLogger(Log4NetTypeEnum.运行日志.ToString());
    private static ILog _configLog = LogManager.GetLogger(Log4NetTypeEnum.修改设置日志.ToString());
    private static ILog _deviceAlarmLog = LogManager.GetLogger(Log4NetTypeEnum.设备报警日志.ToString());
    private static ILog _databaseLog = LogManager.GetLogger(Log4NetTypeEnum.数据库日志.ToString());
    private static ILog _databaseTimeoutLog = LogManager.GetLogger(Log4NetTypeEnum.数据库超时日志.ToString());
    private static object _logRunLocked = new object();
    private static object _logDeviceAlarmLocked = new object();

    /// <summary>
    /// 生产工序日志
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="header"></param>
    /// <param name="level"></param>
    /// <param name="isPrompt"></param>
    public static void LogProcess(this string msg, string header, Log4NetLevelEnum level = Log4NetLevelEnum.信息, bool isPrompt = false)
                       => LogRun(msg, level, isPrompt, header);
    /// <summary>
    /// 运行日志
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    /// <param name="isPrompt">是否在右上角弹出提示窗口</param>
    public static void LogRun(this string msg, Log4NetLevelEnum level = Log4NetLevelEnum.信息, bool isPrompt = false, string header = "")
    {
        try
        {
            switch (level)
            {
                case Log4NetLevelEnum.错误:
                    _runLog.Error($"[{level}] {msg}");
                    break;
                case Log4NetLevelEnum.警告:
                    _runLog.Warn($"[{level}] {msg}");
                    break;
                case Log4NetLevelEnum.信息:
                    _runLog.Info($"[{level}] {msg}");
                    break;
                case Log4NetLevelEnum.成功:
                    _runLog.Debug($"[{level}] {msg}");
                    break;
            }
            Task.Run(() =>
            {
                lock (_logRunLocked)
                {
                    DisplayRunLogAction?.Invoke(msg, level, Log4NetTypeEnum.运行日志);
                    if (isPrompt)
                        PromptAction?.Invoke(msg, level);
                }
            });
        }
        catch (Exception ex)
        {
            _runLog.Error($"LogRun 添加日志信息\r\n[{msg}]r\rn时发生异常：\r\n{ex}");
        }
    }

    /// <summary>
    /// 记录修改设置日志
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    /// <param name="isPrompt">是否在右上角弹出提示窗口</param>
    public static void LogSetting(this string msg, Log4NetLevelEnum level = Log4NetLevelEnum.信息, bool isPrompt = false)
    {
        try
        {
            switch (level)
            {
                case Log4NetLevelEnum.错误:
                    _configLog.Error($"[{level}] {msg}");
                    break;
                case Log4NetLevelEnum.警告:
                    _configLog.Warn($"[{level}] {msg}");
                    break;
                case Log4NetLevelEnum.信息:
                    _configLog.Info($"[{level}] {msg}");
                    break;
                case Log4NetLevelEnum.成功:
                    _configLog.Debug($"[{level}] {msg}");
                    break;
            }
            Task.Run(() =>
            {
                DisplayRunLogAction?.Invoke(msg, level, Log4NetTypeEnum.修改设置日志);
                if (isPrompt)
                    PromptAction?.Invoke(msg, level);
            });
        }
        catch (Exception ex)
        {
            _runLog.Error($"LogSetting 添加日志信息\r\n[{msg}]r\rn时发生异常：\r\n{ex}");
        }
    }
    /// <summary>
    /// 设备掉线报警日志
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    /// <param name="isPrompt"></param>
    public static void LogDeviceAlarm(this string msg, Log4NetLevelEnum level = Log4NetLevelEnum.信息, bool isPrompt = false)
    {
        try
        {
            switch (level)
            {
                case Log4NetLevelEnum.错误:
                    _deviceAlarmLog.Error($"[{level}] {msg}");
                    break;
                case Log4NetLevelEnum.警告:
                    _deviceAlarmLog.Warn($"[{level}] {msg}");
                    break;
                case Log4NetLevelEnum.信息:
                    _deviceAlarmLog.Info($"[{level}] {msg}");
                    break;
                case Log4NetLevelEnum.成功:
                    _deviceAlarmLog.Debug($"[{level}] {msg}");
                    break;
            }
            Task.Run(() =>
            {
                lock (_logDeviceAlarmLocked)
                {
                    DisplayRunLogAction?.Invoke(msg, level, Log4NetTypeEnum.设备报警日志);
                    if (isPrompt)
                        PromptAction?.Invoke(msg, level);
                }
            });
        }
        catch (Exception ex)
        {
            _runLog.Error($"LogDeviceAlarm 添加日志信息\r\n[{msg}]r\rn时发生异常：\r\n{ex}");
        }
    }

    /// <summary>
    /// 数据库日志
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    /// <param name="isPrompt">是否在右上角弹出提示窗口</param>
    public static void LogDatabase(this string msg, Log4NetLevelEnum level = Log4NetLevelEnum.信息, bool isPrompt = false)
    {
        try
        {
            switch (level)
            {
                case Log4NetLevelEnum.错误:
                    _databaseLog.Error($"[{level}] {msg}\r\n");
                    break;
                case Log4NetLevelEnum.警告:
                    _databaseLog.Warn($"[{level}] {msg}\r\n");
                    break;
                case Log4NetLevelEnum.信息:
                    _databaseLog.Info($"[{level}] {msg}\r\n");
                    break;
                case Log4NetLevelEnum.成功:
                    _databaseLog.Debug($"[{level}] {msg}\r\n");
                    break;
            }
            Task.Run(() =>
            {
                if (isPrompt)
                    PromptAction?.Invoke(msg, level);
            });
        }
        catch (Exception ex)
        {
            _runLog.Error($"LogSetting 添加日志信息\r\n[{msg}]r\rn时发生异常：\r\n{ex}");
        }
    }
    /// <summary>
    /// 数据库日志,sql语句超一定时间记录
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    /// <param name="isPrompt">是否在右上角弹出提示窗口</param>
    public static void LogDatabaseTimeout(this string msg, Log4NetLevelEnum level = Log4NetLevelEnum.警告, bool isPrompt = false)
    {
        try
        {
            switch (level)
            {
                case Log4NetLevelEnum.错误:
                    _databaseTimeoutLog.Error($"[{level}] {msg}\r\n");
                    break;
                case Log4NetLevelEnum.警告:
                    _databaseTimeoutLog.Warn($"[{level}] {msg}\r\n");
                    break;
                case Log4NetLevelEnum.信息:
                    _databaseTimeoutLog.Info($"[{level}] {msg}\r\n");
                    break;
                case Log4NetLevelEnum.成功:
                    _databaseTimeoutLog.Debug($"[{level}] {msg}\r\n");
                    break;
            }
            Task.Run(() =>
            {
                if (isPrompt)
                    PromptAction?.Invoke(msg, level);
            });
        }
        catch (Exception ex)
        {
            _runLog.Error($"LogSetting 添加日志信息\r\n[{msg}]r\rn时发生异常：\r\n{ex}");
        }
    }
}
