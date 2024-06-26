using System;
using UnityEngine;
using Object = UnityEngine.Object;
namespace ProceduralWorlds.GTS
{
    public static class GTSDebug
    {
        public const string TITLE = "GTS";
        #region Definitions
        public delegate void LogCallback(LogType logType, string message, Object context = null);
        public delegate void LogFormatCallback(LogType logType, string format, params object[] args);
        #endregion
        #region Variables
        private static bool m_enabled = false;
        private static LogCallback m_logCallback;
        private static LogFormatCallback m_logFormatCallback;
        #endregion
        #region Properties
        public static bool Enabled
        {
            get => m_enabled;
            set => m_enabled = value;
        }
        public static LogCallback OnLogMessage
        {
            // Setup Default
            get => m_logCallback ?? (m_logCallback = DefaultLogCallback);
            set => m_logCallback = value;
        }
        public static LogFormatCallback OnLogMessageFormat
        {
            // Setup Default
            get => m_logFormatCallback ?? (m_logFormatCallback = DefaultLogFormatCallback);
            set => m_logFormatCallback = value;
        }
        #endregion
        #region Methods
        private static string Prefix(string message) => $"[{TITLE}]: {message}";
        public static void LogAssert(string message, Object context = null)
        {
            if (!Enabled)
                return;
            OnLogMessage(LogType.Assert, $"[Assert] {Prefix(message)}", context);
        }
        public static void LogAssertFormat(string format, params object[] objects)
        {
            if (!Enabled)
                return;
            OnLogMessageFormat(LogType.Assert, $"[Assert] {Prefix(format)}", objects);
        }
        public static void LogError(string message, Object context = null)
        {
            if (!Enabled)
                return;
            OnLogMessage(LogType.Error, $"[Error] {Prefix(message)}", context);
        }
        public static void LogErrorFormat(string format, params object[] objects)
        {
            if (!Enabled)
                return;
            OnLogMessageFormat(LogType.Error, $"[Error] {Prefix(format)}", objects);
        }
        public static void LogException(string message, Object context = null)
        {
            if (!Enabled)
                return;
            OnLogMessage(LogType.Exception, $"[Exception] {Prefix(message)}", context);
        }
        public static void Log(string message, Object context = null)
        {
            OnLogMessage(LogType.Log, $"[Log] {Prefix(message)}", context);
        }
        public static void LogFormat(string format, params object[] objects)
        {
            if (!Enabled)
                return;
            OnLogMessageFormat(LogType.Log, $"[Log] {Prefix(format)}", objects);
        }
        public static void LogWarning(string message, Object context = null)
        {
            OnLogMessage(LogType.Warning, $"[Warning] {Prefix(message)}", context);
        }
        public static void LogWarningFormat(string format, params object[] objects)
        {
            OnLogMessageFormat(LogType.Warning, $"[Warning] {Prefix(format)}", objects);
        }
        #endregion
        #region Default Callbacks
        public static void DefaultLogCallback(LogType logType, string message, Object context = null)
        {
            switch (logType)
            {
                case LogType.Assert:
                    Debug.LogAssertion(message, context);
                    break;
                case LogType.Error:
                    Debug.LogError(message, context);
                    break;
                case LogType.Exception:
                    Debug.LogException(new Exception(message), context);
                    break;
                case LogType.Log:
                    Debug.Log(message, context);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message, context);
                    break;
            }
        }
        public static void DefaultLogFormatCallback(LogType logType, string format, params object[] args)
        {
            switch (logType)
            {
                case LogType.Assert:
                    Debug.LogAssertionFormat(format, args);
                    break;
                case LogType.Error:
                    Debug.LogErrorFormat(format, args);
                    break;
                // TODO : Manny : Exception format doesn't exist!
                // case LogType.Exception:
                //     Debug.LogException(new Exception(format), args);
                //     break;
                case LogType.Log:
                    Debug.LogFormat(format, args);
                    break;
                case LogType.Warning:
                    Debug.LogWarningFormat(format, args);
                    break;
            }
        }
        #endregion
    }
}