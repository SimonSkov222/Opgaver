﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace DM_Skills.Models
{
    class SettingsModel : INotifyPropertyChanged
    {
        public static SettingsModel Singleton { get; private set; }
        public SettingsModel()
        {
            Singleton = this;

            Client = new Scripts.JsonClient(converterTCP, dataReceuveClient);
            Client.OnConnected      += delegate() { IsClient = true; InvokeConnection(); };
            Client.OnDisconnected   += (byUser) => { IsClient = false; InvokeDisconnection(byUser); };

            Server = new Scripts.JsonServer(converterTCP, dataReceuveServer);
            Server.OnStarted += delegate() { IsServer = true; InvokeConnection(); };
            Server.OnStopped += delegate() { IsServer = false; InvokeConnection(); };
        }

        private Scripts.IConverterJsonTCP converterTCP = new Scripts.JsonObjectConverter();
        private Scripts.IDataReceive dataReceuveClient = new Scripts.DataReceivedClient();
        private Scripts.IDataReceive dataReceuveServer = new Scripts.DataReceivedServer();


        public string Version { get { return "1.6"; } }
        public string Author { get { return "Kim Danborg & Simon Skov"; } }
        public string Copyright
        {
            get
            {
                int created = 2017;
                string value = $"Copyright {created}";
                if(DateTime.Now.Year > created)
                {
                    value += $" - {DateTime.Now.Year}";
                }
                return value;
            }
        }
        

        public string Debug { get; set; }

        public Func<string> OnGetTimeStatus;
        public Action<double,double, bool> OnSetTimeStatus;
        public event Action OnSchoolsChanged;
        public event Action OnConnection;
        public event Action<bool> OnDisconnection;
        public event Action<LocationModel> OnLocationChanged;
        public event Action OnTimerStarted;
        public event Action OnTimerStopped;
        public event Action OnTimerReset;
        public event Action<MouseButtonEventArgs> OnMouseClick;

        public event Action OnUpload;

        public bool HasConnection { get { return IsServer || IsClient; } }
        public bool HasLostConnection { get { return !HasConnection && HasConnectionBefore; } }
        private bool HasConnectionBefore = false;

        public bool IsServer
        {
            get { return _IsServer; }
            set
            {
                _IsServer = value;
                if (value)
                {
                    HasConnectionBefore = true;
                }
                NotifyPropertyChanged();
                NotifyPropertyChanged("HasConnection");
                NotifyPropertyChanged("HasLostConnection");
                NotifyPropertyChanged(nameof(CanUseTimerControlButtons));
                NotifyPropertyChanged(nameof(CanChangeLocation));
            }
        }
        public bool IsClient
        {
            get { return _IsClient; }
            set
            {
                _IsClient = value;
                if (value)
                {
                    HasConnectionBefore = true;
                }
                NotifyPropertyChanged();
                NotifyPropertyChanged("HasConnection");
                NotifyPropertyChanged("HasLostConnection");
                NotifyPropertyChanged(nameof(CanUseTimerControlButtons));
                NotifyPropertyChanged(nameof(CanChangeLocation));
            }
        }
        private bool _IsServer = false;
        private bool _IsClient = false;

        public Scripts.JsonClient Client;
        public Scripts.JsonServer Server;

        public string FileNameLocalDB { get { return System.IO.Directory.GetCurrentDirectory() + @"\DMiSkillsLocalDB.sqlite"; } }

        public string FileNameDefaultDB = System.IO.Directory.GetCurrentDirectory() + @"\DMiSkillsDB.sqlite";

        private string _FileNameDB = null;
        public string FileNameDB
        {
            get
            {
                if (_FileNameDB == null)
                {
                    var db = Scripts.Database.GetLocalDB("Get FileNameDB ID:" + System.Threading.Thread.CurrentThread.Name);
                    _FileNameDB = db.GetRow("Settings", "Value", "WHERE `Name` = 'LocationDB'")[0].ToString();
                    db.Disconnect();
                }
                
                return _FileNameDB;
            }
            set
            {
                _FileNameDB = null;
                var db = Scripts.Database.GetLocalDB("Set FileNameDB");
                db.Update("Settings", "Value", value, (object)"LocationDB");
                db.Disconnect();
                NotifyPropertyChanged();
            }
        }
        public string PrefixDB = "DM_Test5_";

        private int _TableCnt = 3;
        private int _OverTimeMin = 3;
        public string _Index;
        private LocationModel _Location;
        private ObservableCollection<LocationModel> _AllLocations = null;
        private ObservableCollection<SchoolModel> _AllSchools = null;

        public double OverTimeMill { get { return TimeSpan.FromMinutes(OverTimeMin).TotalMilliseconds; } }
        public string Index
        {
            get
            {
                if (_Index == null)
                {
                    var row = Scripts.Database.GetLocalDB("").GetRow("Settings", "Value", "WHERE `Name` = 'Index'");
                    _Index = row[0].ToString();
                }
                return _Index;
            }
            set
            {
                Scripts.Database.GetLocalDB("").Update("Settings", "Value", value, (object)"Index");
                _Index = null;
                NotifyPropertyChanged();
            }
        }
        public int TableCnt
        {
            get { return _TableCnt; }
            set
            {
                _TableCnt = value;
                NotifyPropertyChanged();
            }
        }
        public int OverTimeMin
        {
            get { return _OverTimeMin; }
            set
            {
                _OverTimeMin = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("OverTimeMill");
                
            }
        }
        public LocationModel Location
        {
            get { return _Location; }
            set
            {

                _Location = value;
                NotifyPropertyChanged(nameof(HasLocation));
                NotifyPropertyChanged();
            }
        }
        public ObservableCollection<LocationModel> AllLocations {
            get
            {
                if (_AllLocations == null)
                {
                    _AllLocations = LocationModel.GetAll();
                    _AllLocations.Insert(0, new Models.LocationModel() { ID = -1, Name = "Vælg lokation" });
                }
                return _AllLocations;
            }
        }
        public ObservableCollection<SchoolModel> AllSchools
        {
            get
            {
                //Console.WriteLine("Call All_Schools");
                if (_AllSchools == null)
                {
                    //Console.WriteLine("Get Schools");
                    _AllSchools = SchoolModel.GetAll();
                }
                return _AllSchools;
            }
        }

        

        public void InvokeSchoolsChanged()
        {
            NotifyPropertyChanged(nameof(AllSchools));
            OnSchoolsChanged?.Invoke();
        }
        public void InvokeConnection()
        {
            OnConnection?.Invoke();
        }


        public void InvokeDisconnection(bool disconnectedByUser)
        {
            OnDisconnection?.Invoke(disconnectedByUser);
        }

        public void InvokeUpload()
        {
            //Console.WriteLine("InvokeUpload");
            OnUpload?.Invoke();
        }

        public void InvokeLocationChanged(object newLocation)
        {

            System.Windows.Application.Current.Dispatcher.Invoke(delegate ()
            {
                //NotifyPropertyChanged();
                OnLocationChanged?.Invoke((LocationModel)newLocation);
            });
        }

        public void InvokeTimerReset()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(delegate ()
            {
                OnTimerReset?.Invoke();
            });
        }
        public void InvokeTimerStarted()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(delegate ()
            {
                OnTimerStarted?.Invoke();
            });
        }
        public void InvokeTimerStopped()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(delegate ()
            {
                OnTimerStopped.Invoke();
            });
        }
        public void InvokeMouseClick(MouseButtonEventArgs e)
        {
            OnMouseClick?.Invoke(e);
        }
        public string InvokeGetTime() { return OnGetTimeStatus?.Invoke() ?? ""; }
        public void InvokeSetTime(double msec, double sliderTime, bool started)
        {
            if(UseGetTime)
                OnSetTimeStatus?.Invoke(msec, sliderTime, started);
        }


        public bool HasLocation
        {
            get
            {
                return _Location != null && _Location.ID != -1;
            }
        }
        private bool _UseGetTime = true;
        private bool _UseGetLocation = true;
        public bool UseGetTime { get { return _UseGetTime; } set { _UseGetTime = value; NotifyPropertyChanged(nameof(CanUseTimerControlButtons)); } }
        public bool UseGetLocation { get { return _UseGetLocation; } set { _UseGetLocation = value; NotifyPropertyChanged(nameof(CanChangeLocation)); } }
        public bool CanUseTimerControlButtons { get { return !(IsClient && UseGetTime); } }
        public bool CanChangeLocation { get { return !(IsClient && UseGetLocation); } }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            switch (propertyName)
            {
                case nameof(AllLocations): _AllLocations = null; break;
                case nameof(AllSchools): _AllSchools = null; break;
            }
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
