﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DM_Skills.Converters
{
    class AutocompleteConverter : IMultiValueConverter
    {
        public const string PARAMS_OPTION = "Option";
        public const string PARAMS_POPUP = "Popup";
        public const string PARAMS_ISSELECTED = "IsSelected";

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((string)parameter)
            {
                case PARAMS_POPUP:
                    var isOpen = values[2] != DependencyProperty.UnsetValue && (bool)values[2];
                    return IsPopupOpen((ItemCollection)values[0], values[1].ToString(), isOpen);
                case PARAMS_OPTION:
                    return GetVisibilityForOption((string)values[0], values[1].ToString());
                case PARAMS_ISSELECTED:
                    return GetIsSelected(values[0], values[1]);
            }

            return null;
        }




        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private bool IsPopupOpen(ItemCollection items, string textbox, bool isOpen)
        {
            var visibleItems = items.Cast<Models.SchoolModel>().Where(o => o.Name.ToLower().Replace(" ", "").StartsWith(textbox.ToLower().Replace(" ", ""))).ToArray();

            if (visibleItems.Length == 1)
            {
                return isOpen && !visibleItems[0].Name.Equals(textbox);
            }
            if (visibleItems.Length > 0)
            {
                return isOpen;
            }
            else
            {
                return false;
            }
        }



        private Visibility GetVisibilityForOption(string textbox, string option)
        {
            textbox = textbox.ToLower().Replace(" ", "");
            option = option.ToLower().Replace(" ", "");
            return option.StartsWith(textbox) ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool GetIsSelected(object textbox, object option)
        {
            if (textbox == null || option == null)
            {
                return false;
            }
            if (!(textbox is Models.SchoolModel) || !(option is string))
            {
                return false;
            }
            
            return (textbox as Models.SchoolModel).Name.Equals(option);
        }
    }
}
