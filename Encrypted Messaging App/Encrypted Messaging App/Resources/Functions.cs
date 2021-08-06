using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace Encrypted_Messaging_App.Views
{
    class Functions
    {
        public static bool isValidEmail(string email, bool excludeBlank = false)
        {
            if(email == null || email.Length == 0){
                if (!excludeBlank) { return true; }
                else { return false; }
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
        public static void IconInvalidReset(Label[] Icons)
        {
            Color black = Color.FromHex("#000000");
            Color red = Color.FromHex("#E74C3C");
            foreach (Label Icon in Icons)
            {
                if(Icon.TextColor == red)
                {
                    Icon.TextColor = black;
                }
            }
        }
        public static void IconInvalidReset(Label Icon)
        {
            IconInvalidReset(new Label[] { Icon });
        }
    
    }
}
