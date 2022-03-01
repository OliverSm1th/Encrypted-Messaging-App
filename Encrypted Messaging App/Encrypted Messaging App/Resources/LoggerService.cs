using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Xamarin.Forms;

namespace Encrypted_Messaging_App
{
    public static class LoggerService
    {   public static IToastMessage toast = DependencyService.Resolve<IToastMessage>();

        public static void Error(string message, int indentationLvl = 0, [CallerLineNumber] int lineNumber = 0)
        {

            StackFrame stackFrame = new StackTrace().GetFrame(1);

            string fileName = stackFrame.GetFileName();
            string className = stackFrame.GetMethod().DeclaringType.Name;
            string methodName = stackFrame.GetMethod().Name;


            Console.WriteLine($"{new string('ㅤ', indentationLvl*2)}Error: {message}".PadRight(45) +$"{fileName}/{className}/{methodName}:{lineNumber}");
        }
        public static void ErrorToast(string toastMessage) {
            toast.LongAlert($"{toastMessage}");
        }

        public static void Debug(string message, int indentationLvl = 0, bool includeMethod = false, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller=null, [CallerFilePathAttribute] string filePath=null)
        {           
            Console.WriteLine($"{new string('ㅤ', indentationLvl*2)}{message}" +  (includeMethod ? $"    ({caller}:{lineNumber})".PadLeft(Math.Max((45-message.Length- indentationLvl*2), 0)) : "") );
        }

        public static void Log(string message)
        {
            Console.WriteLine($"Log: {message}");
        }

    }
}
