using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModelsToJson.Extension
{
    public static class ComboxBehaviorHelper
    {
        public static DependencyProperty SourceValueProperty =
            DependencyProperty.RegisterAttached(
                "SelectedValue",
                typeof(string),
                typeof(ComboxBehaviorHelper),
                new PropertyMetadata(string.Empty));

        public static string GetSourceValue(DependencyObject obj)
        {
            return (string)obj.GetValue(SourceValueProperty);
        }

        public static void SetSourceValue(DependencyObject obj, string value)
        {
            obj.SetValue(SourceValueProperty, value);
        } 
    }
}
