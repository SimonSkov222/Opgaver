﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM_Skills.Models
{
    public class TeamModel : ModelSettings
    {
        const int ERRNO_TIME_NULL = 1;
        const int ERRNO_CLASS_NULL = 2;

        public const string ERROR_TIME_NULL = "";
        public const string ERROR_CLASS_NULL = "";

        public int? ID { get; set; }
        public int SchoolID { get; set; }
        public int LocationID { get; set; }

        private string _Date;
        public string Date
        {
            get { return _Date; }
            set
            {
                _Date = value;
                NotifyPropertyChanged("CanUpload");
                NotifyPropertyOnAll?.Invoke();
            }
        }

        private string _Time;
        public string Time
        {
            get { return _Time; }
            set
            {
                _Time = value;
                NotifyPropertyChanged("CanUpload");
                NotifyPropertyOnAll?.Invoke();
            }
        }

        private string _Class;
        public string Class
        {
            get { return _Class; }
            set
            {
                _Class = value;
                NotifyPropertyChanged("CanUpload");
                NotifyPropertyOnAll?.Invoke();
            }
        }



        public override bool CanUpload
        {
            get
            {
                bool failed = false;
                
                ErrNo = 0;
                Error = "";

                if (Time == null || Time == "")
                {
                    ErrNo += ERRNO_TIME_NULL;
                    Error = ERROR_TIME_NULL;
                    failed = true;
                }
                if (Class == null || Class == "")
                {
                    ErrNo += ERRNO_CLASS_NULL;
                    Error = ERROR_CLASS_NULL;
                    failed = true;
                }


                NotifyPropertyChanged("ErrNo");

                return !failed;
            }
        }


        protected override bool OnUpload()
        {

            if (Date == null || Date == "")
            {
                Date = DateTime.Now.ToShortDateString();
            }
            var myDB = Scripts.Database.GetDB();
            if (ID != null)
            {
                return false;
            }
            else
            {
                ID = (int)myDB.Insert("Teams", new string[] { "Class", "SchoolID", "LocationID", "Date", "Time"}, new string[] { Class });
            }
            myDB.Disconnect();
            return true;


            //return true;


        }
    }
}
