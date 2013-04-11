using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net;


namespace UDPServer
{
    static class Constants
    {
        public const int WARNING = 0;
        public const int RECEIVE = 1;
        public const int SENDING = 2;
        public const int BUTTONS = 3;
    }

    class ServerThread
    {
        BackgroundWorker bw;                //background worker
        private bool serverStatus;          //server status
        private byte[] data;                //data buffer
        private IPEndPoint ipep;            //receiving information
        private UdpClient newsock;          //UDP socket
        private IPEndPoint sender;          //sender information

        //Constructor
        public ServerThread(int port)
        {
            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.DoWork +=new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged +=new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerCompleted +=new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

            serverStatus = false;
            data = new byte[1024];
            ipep = new IPEndPoint(IPAddress.Any, port);
            sender = new IPEndPoint(IPAddress.Any, 0);
        }

        //Background worker delegate
        private void bw_DoWork(object senderx, DoWorkEventArgs e)
        {
            BackgroundWorker worker = senderx as BackgroundWorker;
            worker.ReportProgress(Constants.BUTTONS, "Server started");
            while(true)
            {
                if ((worker.CancellationPending == true))
                {
                    Debug.WriteLine("cancel pending");
                    e.Cancel = true;
                    newsock.Close();
                    serverStatus = false;
                    break;
                }
                else
                {
                    try
                    {
                        Debug.WriteLine("waiting for reponse..");
                        data = newsock.Receive(ref sender);
                        Console.WriteLine("Message received from {0}:", sender.ToString());
                        Console.WriteLine(Encoding.ASCII.GetString(data, 0, data.Length));
                        worker.ReportProgress(Constants.WARNING,"data received");
                        string received = Encoding.ASCII.GetString(data, 0, data.Length);
                        received = sender.ToString() + ":" + "-" + received;
                        worker.ReportProgress(Constants.RECEIVE, received);

                        //newsock.Send(data, data.Length, sender);
                        string response = "Welcome to my test server";
                        response = "Server: " + "-" + response;
                        data = Encoding.ASCII.GetBytes(response);
                        newsock.Send(data, data.Length, sender);
                        worker.ReportProgress(Constants.SENDING, response);
                    }
                    catch (SocketException socketEx)
                    {
                        //To catch breaking of udp polling
                    }
                    
                }
            }
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == Constants.WARNING)
            {
                TinyAlertView.Show(TinyAlertView.StateTinyAlert.WARNING, e.UserState as String);
            }
            else if (e.ProgressPercentage == Constants.RECEIVE)
            {
                string response = e.UserState as String;
                EventHandlers.UpdateReceived(response);
            }
            else if (e.ProgressPercentage == Constants.SENDING)
            {
                string response = e.UserState as String;
                EventHandlers.UpdateSend(response);
            }
            else if (e.ProgressPercentage == Constants.BUTTONS)
            {
                TinyAlertView.Show(TinyAlertView.StateTinyAlert.SUCCESS, e.UserState as String);
                EventHandlers.UpdateButtonState(2);
            }
            
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                //this.tbProgress.Text = "Canceled!";
                TinyAlertView.Show(TinyAlertView.StateTinyAlert.FAILURE, "Server Stopped");
                EventHandlers.UpdateButtonState(1);
            }

            else if (!(e.Error == null))
            {
                TinyAlertView.Show(TinyAlertView.StateTinyAlert.SUCCESS, "Error");
            }

            else
            {
                TinyAlertView.Show(TinyAlertView.StateTinyAlert.SUCCESS, "Done");
            }
        }

        //Set server port
        public void SetPort(int port)
        {
            ipep = new IPEndPoint(IPAddress.Any, port);
        }

        //Start server
        public void StartServer()
        {
            serverStatus = true;
            Debug.WriteLine("Listening now..");
            newsock = new UdpClient(ipep);
            newsock.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);

            if (bw.IsBusy != true)
            {
                TinyAlertView.Show(TinyAlertView.StateTinyAlert.WARNING, "Starting..");
                EventHandlers.UpdateButtonState(0);
                bw.RunWorkerAsync();
            }

        }

        //Stop Server
        public void StopServer()
        {
            if (bw.WorkerSupportsCancellation == true)
            {
                TinyAlertView.Show(TinyAlertView.StateTinyAlert.WARNING, "Stopping..");
                Debug.WriteLine("attempting cancel");
                EventHandlers.UpdateButtonState(0);
                bw.CancelAsync();
            }
            
        }

        //Get Server Start Stop status
        public bool GetStatus()
        {
            return serverStatus;
        }


    }
}
