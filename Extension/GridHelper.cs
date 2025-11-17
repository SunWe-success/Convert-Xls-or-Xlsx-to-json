using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ModelsToJson.Extension
{
    internal static class GridHelper
    {
       public static DependencyProperty RowDefinitionsProperty =
           DependencyProperty.RegisterAttached(
               "RowDefinitions",
               typeof(string),
               typeof(GridHelper),
               new PropertyMetadata(null, OnRowDefinitionsChnaged));

        public static void SetRowDefinitions(DependencyObject element, string value)
        {
            element.SetValue(RowDefinitionsProperty, value);
        }

        public static string GetRowDefinitions(DependencyObject element)
        {
            return (string)element.GetValue(RowDefinitionsProperty);
        }   

        /// <summary>
        /// 行附加属性的变更回调
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void OnRowDefinitionsChnaged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Grid grid) return;
            if (e.NewValue is not string val) return;
            grid.RowDefinitions.Clear();
            var rowDefinitions = val.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in rowDefinitions)
            {
                grid.RowDefinitions.Add(CreatRowDefinition(item));
            }
        }

        private static RowDefinition CreatRowDefinition(string item)
        {
            RowDefinition row = new RowDefinition();
            if (item.EndsWith("*"))
            {
                double height1 = 1;
                string val = item.TrimEnd('*');
                if (item.Length > 1 && double.TryParse(val, out double height2))
                {
                    height1 = height2;
                }
                row.Height = new GridLength(height1, GridUnitType.Star);
            }
            else if (item.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                row.Height = GridLength.Auto;
            }
            else if (double.TryParse(item, out double height3))
            {
                row.Height = new GridLength(height3);
            }
            return row; 
        }


        public static DependencyProperty ColumnDefinitionsProperty =
           DependencyProperty.RegisterAttached(
               "ColumnDefinitions",
               typeof(string),
               typeof(GridHelper),
               new PropertyMetadata(null, OnColumnDefinitionsChnaged));

        public static void SetColumnDefinitions(DependencyObject element, string value)
        {
            element.SetValue(ColumnDefinitionsProperty, value);
        }

        public static string GetColumnDefinitions(DependencyObject element)
        {
            return (string)element.GetValue(ColumnDefinitionsProperty);
        }

        /// <summary>
        /// 列属性的变更回调
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void OnColumnDefinitionsChnaged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Grid grid) return;
            if (e.NewValue is not string val) return;
            grid.ColumnDefinitions.Clear();
            var columnDefinitions = val.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in columnDefinitions)
            {
                grid.ColumnDefinitions.Add(CreatColumnDefinition(item));
            }
        }

        private static ColumnDefinition CreatColumnDefinition(string item)
        {
            ColumnDefinition column = new ColumnDefinition();
            if (item.EndsWith("*"))
            {
                double width1 = 1;
                string val = item.TrimEnd('*');
                if (item.Length > 1 && double.TryParse(val, out double width2))
                {
                    width1 = width2;
                }
                column.Width = new GridLength(width1, GridUnitType.Star);
            }
            else if (item.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                column.Width = GridLength.Auto;
            }
            else if (double.TryParse(item, out double width3))
            {
                column.Width = new GridLength(width3);
            }
            return column;
        }
    }
}
