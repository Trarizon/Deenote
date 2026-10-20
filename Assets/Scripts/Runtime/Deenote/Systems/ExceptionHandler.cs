#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.CoreB.Localization;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Deenote.Systems
{
    public sealed class ExceptionHandler
    {
        public ExceptionHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Application.logMessageReceived += OnUnityLogMessageReceived;
            UniTaskScheduler.UnobservedTaskException += OnUniTaskUnhandledException;
            if (File.Exists(LogFile))
                File.Delete(LogFile);
        }

        private const string LogFile = "exceptions.log";

        public event Action<ExceptionInfo>? UnhandledExceptionOccurred;

        private void OnUnityLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type is LogType.Exception) {
                UnhandledExceptionOccurred?.Invoke(new ExceptionInfo(condition, stackTrace));
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            UnhandledExceptionOccurred?.Invoke(new ExceptionInfo(ex.Message, ex.StackTrace, ex));
        }

        private void OnUniTaskUnhandledException(Exception e)
        {
            UnhandledExceptionOccurred?.Invoke(new ExceptionInfo(e.Message, e.StackTrace, e));
        }

        public record struct ExceptionInfo(string Message, string StackTrace, Exception? Exception = null);
    }
}