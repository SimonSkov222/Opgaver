﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM_Skills.Models
{
    public class PersonModel : ModelSettings
    {

        const int ERRNO_NAME_NULL = 1;

        public const string ERROR_NAME_NULL = "";



        public int? ID { get; protected set; }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                Console.WriteLine("_Name");
                _Name = value;
                NotifyPropertyChanged("CanUpload");
                Console.WriteLine(NotifyPropertyOnAll == null);
                NotifyPropertyOnAll?.Invoke();
            }
        }
        public int TeamID { get; set; }

        public override bool CanUpload
        {
            get
            {
                Console.WriteLine("__CanUpload");

                if (Name == null || Name == "")
                {
                    ErrNo = ERRNO_NAME_NULL;
                    Error = ERROR_NAME_NULL;
                    return false;
                }

                return true;
            }
        }

        protected override bool OnUpload()
        {
            var myDB = Scripts.Database.GetDB();

            if (ID != null)
            {
                return false;
            }
            else
            {
                ID = (int)myDB.Insert("Persons", "Name", Name);
            }
            myDB.Disconnect();

            return true;
        }
    }
}
