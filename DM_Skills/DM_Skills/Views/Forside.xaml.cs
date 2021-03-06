﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Drawing.Imaging;

namespace DM_Skills.Views
{
    /// <summary>
    /// Interaction logic for Forside.xaml
    /// </summary>
    public partial class Forside : UserControl, INotifyPropertyChanged
    {
        
        public int NumbOfTables
        {
            get { return (int)GetValue(NumbOfTablesProperty); }
            set { SetValue(NumbOfTablesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NumbOfTables.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumbOfTablesProperty =
            DependencyProperty.Register(
                "NumbOfTables", 
                typeof(int), 
                typeof(Forside), 
                new PropertyMetadata(
                    3, 
                    new PropertyChangedCallback(CallBackProperty)
                    )
                );

        private Models.SettingsModel Settings;

        public static void CallBackProperty(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Forside element = sender as Forside;

            if (e.Property == NumbOfTablesProperty)
            {
                element.UpdateTableLayout((int)e.NewValue);
            }
        }

        private void UpdateTableLayout(int numb)
        {
            if (numb < 1)
            {
                numb = 1;
            }
            else if (numb > 10)
            {
                numb = 10;
            }
            var visibleCnt = listOfTables.Children.Cast<UIElement>().Count(o => o.Visibility == Visibility.Visible);
            
            //Fjern sidste bord
            for (int i = visibleCnt -1; i >= numb; i--)
            {
                listOfTables.Children[i].Visibility = Visibility.Collapsed;
                if (listOfTables.Children[i] is Controls.TablesControl)
                {
                    (listOfTables.Children[i] as Controls.TablesControl).Reset();
                }
            }

            visibleCnt = listOfTables.Children.Cast<UIElement>().Count(o => o.Visibility == Visibility.Visible);
            int all = listOfTables.Children.Count;

            //  V
            //  V
            //  H
            //  i = 2
            //  Numb = 10
            for (int i = visibleCnt; i < numb && i < all; i++)
            {
                listOfTables.Children[i].Visibility = Visibility.Visible;
            }

            //while (visibleCnt > numb)
            //{
            //    listOfTables.Children.RemoveAt(listOfTables.Children.Count - 1);
            //}

            //Tilføj nye bordre
            while (listOfTables.Children.Count < numb)
            {
                Controls.TablesControl table = new Controls.TablesControl();
                table.Title = "Bord " + (listOfTables.Children.Count +1);
                table.Margin = new Thickness(0, 0, 0, 3);
                table.TabIndex = this.TabIndex + 1000 + listOfTables.Children.Count * 100;

                table.SetBinding(Controls.TablesControl.ShowDropLocationProperty, new Binding() {
                    Path = new PropertyPath("IsDraggingItem"),
                    Source = LapList
                });
                table.SetBinding(Controls.TablesControl.SchoolsProperty, new Binding() {
                    Source = Settings,
                    Path = new PropertyPath("AllSchools"),
                    Mode = BindingMode.OneWay
                });
                
                listOfTables.Children.Add(table);
            }

            
        }


        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fortæller at en property har ændret sig
        /// </summary>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            {
                //Tjekker om PropertyChanged er sat og kalder den hvis den er
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        

        /// <summary>
        /// Her connecter vi til databasen og opretter den
        /// vi tilføjet også en load event
        /// </summary>
        public Forside()
        {            
            InitializeComponent();
            Settings = (Models.SettingsModel)FindResource("Settings");
            Loaded += (o,e) => UpdateTableLayout(NumbOfTables);
        }



        /// <summary>
        /// StopUr omgang klik.
        /// Tilføj omgang tiden til vores drag and drop list 
        /// </summary>
        private void TimerControl_OnLap(TimeSpan obj)
        {
            LapList.Add(obj);
        }

        /// <summary>
        /// StopUr Nulstil klik.
        /// Gør at vores drag and drop list også bliver resat
        /// </summary>
        private void TimerControl_OnReset()
        {
            //LapList.Reset();
            timerstarted = false;
        }
        

        /// <summary>
        /// Indsend klik
        /// Her tjekker vi om bordene er udfyldt rigtigt
        /// og uploader dem hvis de er
        /// 
        /// Brugeren får også besked om man kunne
        /// uploade eller ej
        /// </summary>
        private void Button_Upload_Click(object sender, RoutedEventArgs e)
        {
            
            //Settings.InvokeUpload();
            if (!Settings.HasConnection)
            {
                MessageBox.Show("Der er ikke nogen database", "Ingen Database", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }
            if (Settings.Location == null || Settings.Location.ID == -1)
            {
                MessageBox.Show("Du har ikke valgt nogen Location", "Vælg Location", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            bool allowUpload = true;
            int cnt = 0;
            foreach (var item in listOfTables.Children)
            {
                if (item is Controls.TablesControl)
                {
                    if ((item as Controls.TablesControl).Model.HasData)
                    {
                        (item as Controls.TablesControl).Model.Location = Settings.Location;
                    }
                    if ((item as Controls.TablesControl).Model.HasData && !(item as Controls.TablesControl).Model.CanUpload)
                    {
                        (item as Controls.TablesControl).Model.FailedUpload = true;
                        allowUpload = false;
                    }
                    if ((item as Controls.TablesControl).Model.HasData && (item as Controls.TablesControl).Model.CanUpload)
                    {
                        cnt++;
                    }
                }
            }

            if (allowUpload && cnt > 0)
            {
                //Scripts.Helper.BackupBitmap(listOfTables);
                Scripts.Helper.BackupPDF(listOfTables);

                for (int i = 0;i < listOfTables.Children.Count; i++)
                {
                    var item = listOfTables.Children[i];
                    if (item is Controls.TablesControl)
                    {
                        if ((item as Controls.TablesControl).Model.CanUpload)
                        {
                            //Console.WriteLine("### Upload Table");
                            (item as Controls.TablesControl).Model.Team.UniqueID = $"{Settings.Index}_{i}_{DateTimeOffset.Now.ToUnixTimeSeconds().ToString()}";
                            (item as Controls.TablesControl).Model.Upload();
                        }
                    }
                }
                Models.TableModelN.RequestBroadcast(Scripts.JsonCommandIDs.Broadcast_UploadTables);
                Button_Reset_Click(null, null);
            }
            else
            {
                foreach (var i in listOfTables.Children)
                {
                    if (i is Controls.TablesControl)
                    {
                        (i as Controls.TablesControl).Model.Location = new Models.LocationModel();
                    }
                }
            }

        }



        private Random autoFill_r;
        private int autoFill_cID = 0;
        private int autoFill_sID = 0;
        public void AutoFill()
        {
            if (autoFill_r == null)
            {
                autoFill_r = new Random();
                autoFill_cID = autoFill_r.Next(2, 99);
            }
            foreach (var item in listOfTables.Children)
            {
                autoFill_sID++;
                if (item is Controls.TablesControl)
                {
                    var mod = new Models.TableModelN();
                    mod.Team.Class = autoFill_cID.ToString();
                    mod.Team.Time = $"{autoFill_r.Next(0, 23).ToString().PadLeft(2, '0')}:{autoFill_r.Next(0, 59).ToString().PadLeft(2, '0')}:{autoFill_r.Next(0, 59).ToString().PadLeft(2, '0')}";
                    mod.School.Name = "Min Skole #" + autoFill_sID;
                    for (int i = 0; i < 3; i++)
                    {
                        var p = new Models.PersonModel();
                        p.Name = $"Person{i}_{autoFill_sID}";
                        mod.Persons.Add(p);
                    }
                    (item as Controls.TablesControl).Model = mod;
                }

            }
        }

        /// <summary>
        /// Nulstiling af runde klik.
        /// </summary>
        
        private void Button_Reset_Click(object sender, RoutedEventArgs e)
        {
            timerstarted = false;
            foreach (var item in listOfTables.Children)
            {
                if (item is Controls.TablesControl)
                {
                    //(item as Controls.TablesControl).Model = new Models.TableModelN();
                    (item as Controls.TablesControl).Reset();
                }
                
            }

            stopwatch.Reset();
            LapList.Reset();
        }

        private bool timerstarted = false;

        private void TimerControl_OnStop()
        {
            if (timerstarted)
            {
                LapList.Add(stopwatch.DisplayTime);
                timerstarted = false;
            }
        }

        private void TimerControl_OnStart()
        {
            timerstarted = true;

        }
    }



}
