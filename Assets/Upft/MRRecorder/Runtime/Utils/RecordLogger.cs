using System;
using System.Runtime.CompilerServices;

namespace Upft.MRRecorder.Runtime.Utils
{
    public static class RecordLogger
    {
        private const string LogPrefix = "[MRRecorder]";

        public static void Info(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || MR_RECORDER_FORCE_LOG
            UnityEngine.Debug.Log($"{LogPrefix} {message}\n" +
                $"at {memberName} in {System.IO.Path.GetFileName(sourceFilePath)}:{sourceLineNumber}");
#endif
        }

        public static void Warning(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || MR_RECORDER_FORCE_LOG
            UnityEngine.Debug.LogWarning($"{LogPrefix} {message}\n" +
                $"at {memberName} in {System.IO.Path.GetFileName(sourceFilePath)}:{sourceLineNumber}");
#endif
        }

        public static void Error(
            string message,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var errorMessage = exception == null
                ? message
                : $"{message}\nException: {exception}";

            UnityEngine.Debug.LogError($"{LogPrefix} {errorMessage}\n" +
                $"at {memberName} in {System.IO.Path.GetFileName(sourceFilePath)}:{sourceLineNumber}");
        }

        public static void ErrorWithStackTrace(
            string message,
            Exception exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var stackTrace = exception?.StackTrace ?? Environment.StackTrace;
            UnityEngine.Debug.LogError($"{LogPrefix} {message}\n" +
                $"Exception: {exception}\n" +
                $"StackTrace:\n{stackTrace}\n" +
                $"at {memberName} in {System.IO.Path.GetFileName(sourceFilePath)}:{sourceLineNumber}");
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD || MR_RECORDER_FORCE_LOG
        public static void Debug(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            UnityEngine.Debug.Log($"{LogPrefix} [DEBUG] {message}\n" +
                $"at {memberName} in {System.IO.Path.GetFileName(sourceFilePath)}:{sourceLineNumber}");
        }

        public static void Trace(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            UnityEngine.Debug.Log($"{LogPrefix} [TRACE] {message}\n" +
                $"at {memberName} in {System.IO.Path.GetFileName(sourceFilePath)}:{sourceLineNumber}");
        }
#else
        public static void Debug(string message, 
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0) { }
        
        public static void Trace(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0) { }
#endif
    }
}
