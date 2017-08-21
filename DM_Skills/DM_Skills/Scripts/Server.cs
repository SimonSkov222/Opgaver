﻿//using GodSharp;
using SimpleTCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DM_Skills.Scripts
{
    class Server
    {
        //SocketServer server;
        private Models.SettingsModel Settings;
        SimpleTcpServer Host;

        public Server()
        {
            Settings = (Models.SettingsModel)Application.Current.FindResource("Settings");
            Application.Current.MainWindow.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Stop();
        }

        public void Start(int port = 7788)
        {
            Host = new SimpleTcpServer();
            Host.DataReceived += Host_DataReceived;
            Host.ClientConnected += (o, e) => { Console.WriteLine("Client Connected"); };

            
            
            Host.Start(port);
            Settings.IsServer = true;
            Settings.InvokeConnection();
        }

        public void Stop()
        {
            Host.Stop();
            Host = null;
            Settings.IsServer = false;
            Settings.InvokeDisconnection(true);
        }


        public void Broadcast(PacketType type)
        {
            var packet = new Packet()
            {
                Type = type,
                ID = -1,
                Data = null,
                BroadcastType = null
            };
            Application.Current.Dispatcher.Invoke(delegate ()
            {
                switch (type)
                {
                    case PacketType.Broadcast_UploadTables:
                        Console.WriteLine("Broadcast_UploadTables");
                        Settings.InvokeSchoolsChanged();
                        Settings.InvokeUpload();
                        break;
                    case PacketType.Broadcast_UploadSchools:
                        Console.WriteLine("Broadcast_UploadSchools");
                        Settings.InvokeSchoolsChanged();
                        break;
                }
            });


            Host.Broadcast(Helper.ObjectToByteArray(packet));
        }

        private void Host_DataReceived(object sender, Message e)
        {
            Packet packet;

            try
            {
                packet = Helper.ByteArrayToObject(e.Data) as Packet;
            }
            catch (Exception)
            {
                //Code fundet i Client.cs
                var code = System.Text.RegularExpressions.Regex.Match(e.MessageString, "#\\d+").Value;
                if (code == "#875120")
                {
                    e.Reply(Helper.ObjectToByteArray(new Packet() { Type = PacketType.Ping }));
                }
                return;
            }
            var reply = new Packet() { ID = packet.ID, Type = packet.Type };

            switch (packet.Type)
            {
                case PacketType.Read:

                    var myDB = Database.GetDB();
                    var dt = myDB.ExecuteQuery(packet.Data as string);
                    myDB.Disconnect();

                    reply.Data = dt;
                    e.Reply(Helper.ObjectToByteArray(reply));
                    break;
                case PacketType.Broadcast_UploadTables:
                    Broadcast(packet.Type);
                    break;
                case PacketType.Broadcast_UploadSchools:
                    Broadcast(packet.Type);
                    break;
            }
        }





    }
}
