﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DM_Skills
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Models.SettingsModel Settings;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            EventManager.RegisterClassHandler(typeof(Window), Window.PreviewMouseDownEvent, new MouseButtonEventHandler(OnPreviewMouseDown));
            Settings = FindResource("Settings") as Models.SettingsModel;
            //MessageBox.Show(DateTimeOffset.Now.ToUnixTimeSeconds().ToString());

            for (int i = 0; i < e.Args.Length; i++)
            {
                if (e.Args[i] == "-ID" && e.Args.Length > i + 1)
                {
                    Settings.Index = e.Args[i + 1];
                }

            }
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            
            Settings.InvokeMouseClick(e);
        }
    }
}
