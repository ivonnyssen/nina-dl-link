using System;
using System.Runtime.CompilerServices;

namespace IgorVonNyssen.NINA.DlLink.DlLinkSequenceItems {

    public interface ILogger {

        void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);

        void Error(string message, Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);

        void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
    }
}