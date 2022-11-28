﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;


namespace SellukeittoSovellus
{
    public partial class MainWindow : Window
    {

        #region CONSTANTS

        // System state
        public const int STATE_FAILSAFE = 0;
        public const int STATE_DISCONNECTED = 1;
        public const int STATE_IDLE = 2;
        public const int STATE_RUNNING = 3;

        #endregion


        #region CLASS VARIABLES

        ProcessClient mProcessClient = new ProcessClient();

        public int State = STATE_DISCONNECTED; // Controller state

        public double Cooking_time;
        public double Cooking_temperature;
        public double Cooking_pressure;
        public double Impregnation_time;

        #endregion


        #region CONFIGURABLE PARAMETERS

        private const int THREAD_DELAY_MS = 10;

        #endregion


        #region DELEGATES

        private delegate void UpdateControl_callback();
        private delegate void UpdateValues_callback();

        #endregion


        //#################


        private void ControlThread()
        {
            try
            {
                while (true) // TODO close
                {
                    Thread.Sleep(THREAD_DELAY_MS);
                    //Console.WriteLine("{0} ControlThread cycle", DateTime.Now.ToString("hh:mm:ss"));

                    CheckConnectionStatus();

                    // Update controls
                    Dispatcher.BeginInvoke(new UpdateControl_callback(UpdateControl), DispatcherPriority.Render, new object[] { });

                    // Update values
                    Dispatcher.BeginInvoke(new UpdateValues_callback(UpdateValues), DispatcherPriority.Render, new object[] { });

                    switch (State)
                    {
                        case STATE_FAILSAFE:

                            break;
                        case STATE_DISCONNECTED:

                            break;
                        case STATE_IDLE:

                            break;
                        case STATE_RUNNING:

                            break;
                        default:
                            Console.WriteLine("ERROR: UpdateValues() switch default statement called");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void CheckConnectionStatus()
        {
            if (mProcessClient.mConnectionState)
            {
                if (State == STATE_DISCONNECTED)
                {
                    State = STATE_IDLE;
                }
            }
            else
            {
                if (State == STATE_RUNNING || State == STATE_IDLE)
                {
                    State = STATE_FAILSAFE;
                }
            }
        }
    }
}
