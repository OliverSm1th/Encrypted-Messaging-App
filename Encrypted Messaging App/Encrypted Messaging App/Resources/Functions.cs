using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Reflection;
using System.Numerics;

namespace UsefulExtensions
{
    public static class Extensions
    {
        public static string[] Remove(this string[] arr, string item)
        {
            List<string> l_array = new List<string>(arr);
            if (l_array.Remove(item))
            {
                return l_array.ToArray();
            }
            else { return null; }
        }
    }
}




namespace Encrypted_Messaging_App.Views
{
    class Functions
    {
        static Dictionary<string, string[]> colourDict = new Dictionary<string, string[]>(){
            {"Values", new string [] {"Primary", "PrimaryLight", "Secondary", "SecondaryLight"} },
            {"Blue",   new string [] {"2196F3",  "6ec5ff",       "77BEF5",    "acf2ff" } },
            //{"Red",    new string [] {"b71c1c",  "f05545",       "f44336",    "ff7961", } }
            {"Red",    new string [] { "d32f2f",  "f05545",       "f44336",    "ff7961", } }
        };




        public static bool isValidEmail(string email, bool allowBlank = false)
        {    /*
             - Must have exactly one @
             - The second part must have a period, but it can't be the last letter */

            if(email == null || email.Length == 0){
                return allowBlank;
            }
            string[] parts = email.Split('@');
            if(parts.Length != 2) { return false; }
            string name = parts[0];
            string domain = parts[1];
            if (domain.Contains(".") && domain[domain.Length - 1] != '.')
            {
                return true;
            }
            else { return false; }
        }

        public static bool isValidUsername(string username, bool allowBlank = false)
        {    /*
             - Must be only one word
             - Min length=4 and Max length=15 */
            char[] bannedChars = new char[] { '\'', '\\', '<', '>', '\"' };

            if (username == null || username.Length == 0) { return allowBlank; }
            if(username.Split(' ').Length != 1) { return false; }
            if(username.Length<4 || username.Length > 15) { return false; }
            if(username.IndexOfAny(bannedChars) != -1) { return false; }
            return true;
        }

        public static void EntryFocused(string nameStart, View Content)
        {
            Label Icon = (Label)Content.FindByName($"{nameStart}Icon");
            Color newColor = Color.FromHex("#2196F3"); // Primary
            Color defaultColor = Color.FromHex("#000000"); // Black
            if (Icon.TextColor == newColor) { Icon.TextColor = defaultColor; }
            else { Icon.TextColor = newColor; }
        }
        public static Label EntryInvalid(string nameFull, View Content, Label Icon, int row)
        {
            Entry main = (Entry)Content.FindByName(nameFull);
            return EntryInvalid(main, Icon, row);
        }
        public static Label EntryInvalid(Entry main, Label Icon, int row)
        {
            if(Icon == null) { Icon = new Label(); }
            main.SetValue(Grid.ColumnSpanProperty, 1);
            Icon.Text = "\uf057";
            Icon.FontFamily = "Icons-R";
            Icon.TextColor = Color.FromHex("#E74C3C");
            Icon.VerticalTextAlignment = TextAlignment.Center;
            Icon.HorizontalTextAlignment = TextAlignment.Center;
            Grid grid = (Grid)main.Parent;
            Grid.SetColumn(Icon, 2);
            Grid.SetRow(Icon, row);
            grid.Children.Add(Icon);
            //Console.WriteLine(Icon.Parent.GetType());
            //Icon.SetValue(Grid.RowProperty, 1);
            //Icon.SetValue(Grid.ColumnProperty, 2);
            Console.WriteLine("Done");
            return Icon;
        }
        public static Label EntryInvalidReset(Entry main, Label Icon)
        {
            Grid grid = (Grid)main.Parent;
            grid.Children.Remove(Icon);
            main.SetValue(Grid.ColumnSpanProperty, 2);
            return null;
        }
    
        public static void IconInvalid(Label Icon)
        {
            Icon.TextColor = Color.FromHex("#E74C3C");
        }
        public static void IconInvalid(Button Icon)
        {
            Icon.TextColor = Color.FromHex("#E74C3C");
        }

        private static void ObjInvalidReset(Object[] Icons)
        {
            Color black = Color.FromHex("#000000");
            Color red = Color.FromHex("#E74C3C");
            foreach (object Icon in Icons)
            {
                if ((Color)Icon.GetType().GetProperty("TextColor").GetValue(Icon, null) == red)
                {
                    Icon.GetType().GetProperty("TextColor").SetValue(Icon, black);
                }
            }
        }
        public static void IconInvalidReset(Label[] Icons) {
            ObjInvalidReset(Icons);
        }
        public static void IconInvalidReset(Button[] Icons)
        {
            ObjInvalidReset(Icons);
        }

        public static void IconInvalidReset(Label Icon)
        {
            IconInvalidReset(new Label[] { Icon });
        }
        public static void IconInvalidReset(Button Icon)
        {
            IconInvalidReset(new Button[] { Icon });
        }


        public static void setColour(string colourName)
        {
            if (!colourDict.ContainsKey(colourName)) { LoggerService.Error($"Colour: {colourName} not found"); return; }

            for (int i = 0; i < colourDict["Values"].Length; i++)
            {
                string currentColourKey = colourDict["Values"][i];
                string currentColourValue = colourDict[colourName][i];
                if (!App.Current.Resources.ContainsKey(currentColourKey)) { LoggerService.Error($"Colour key: {currentColourKey} not found in Resources"); }
                App.Current.Resources[currentColourKey] = Color.FromHex(currentColourValue);
                LoggerService.Log($"Saved {currentColourValue} to value {currentColourKey}");
            }

            LoggerService.Log("Set all colours");
        }


        private static int maxLength = 25;
        public static void OutputProperties(object instance, int indentation = 0)
        {
            Type instanceType = instance.GetType();
            string indent = new string(' ', indentation * 5);

            foreach(PropertyInfo prop in instanceType.GetProperties())
            {
                string propName = prop.Name;
                if (propName.Length >= maxLength - 2) { propName = propName.Substring(0, maxLength - 2) + ".."; }
                else { propName = propName.PadRight(15); }

                object instanceValue = prop.GetValue(instance);
                if (prop.PropertyType.IsArray)
                {
                    if (instanceValue == null) { Console.WriteLine($"{indent}{propName} : Empty Array"); continue; }

                    object[] instanceArr = (object[])instanceValue;
                    Console.WriteLine($"{indent}{propName}:");
                    for (int i = 0; i < instanceArr.Length; i++)
                    {
                        if (prop.PropertyType.Namespace == "Encrypted_Messaging_App" && indentation < 10) { OutputProperties(instanceArr[i], indentation + 1); }
                        else { Console.WriteLine($"{indent}     {i} : {ConvertValue(instanceArr[i])}"); }
                    }
                } else if (prop.PropertyType.Namespace == "Encrypted_Messaging_App" && indentation < 10)
                {
                    if(instanceValue == null) { Console.WriteLine($"{indent}{propName}: null"); continue; }

                    Console.WriteLine($"{indent}{propName}:");
                    OutputProperties(instanceValue, indentation + 1);
                }
                else
                {
                    Console.WriteLine($"{indent}{propName} : {ConvertValue(instanceValue)}");
                }
            }
        }
        private static string ConvertValue(object value)
        {
            if(value == null) { return "null"; }
            else if (value is string valueStr)
            {
                if(valueStr.Length >= maxLength - 2) { valueStr = valueStr.Substring(0, maxLength - 2) + ".."; }
                return valueStr;
            }
            else if (value is DateTime dateTime)
            {
                return dateTime.ToString();
            }
            else if (value is BigInteger bigInt)
            {
                return bigInt.ToString().Substring(0, maxLength -2) + "..";
            }
            else if (value.GetType()== typeof(int))
            {
                return value.ToString();
            }
            else {
                return null;
            }
        }

    }
    class DebugManager
    {
        public static IToastMessage toast = DependencyService.Resolve<IToastMessage>();
        public static void ErrorToast(string toastMsg, string debugMsg, [CallerFilePath] string sourceFilePath="", [CallerLineNumber] int sourceLineNumber=0)
        {
            if (!GlobalVariables.DeveloperMode) { toast.LongAlert($"{toastMsg}");
            }
            ErrorSilent(debugMsg, sourceFilePath, sourceLineNumber);
        }
        public static void ErrorSilent(string debugMsg, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (GlobalVariables.DeveloperMode) { toast.LongAlert($"⚠️ {debugMsg}"); }
            Console.WriteLine($"[ERROR] {LastOfPath(sourceFilePath)}:{sourceLineNumber}  |  {debugMsg}");

        }

        private static string LastOfPath(string path)
        {
            string[] parts = path.Split('/');
            return parts[parts.Length - 1];
        }

    }
}
