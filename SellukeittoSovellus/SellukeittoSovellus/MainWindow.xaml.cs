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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region CONSTANTS
        
        // Parameter configuration board parameters
        private const string STATE_FAILSAFE_STRING = "Vikaturvallinen";
        private const string STATE_DISCONNECTED_STRING = "Verkkoyhteydetön";
        private const string STATE_IDLE_STRING = "Odottaa";
        private const string STATE_RUNNING_STRING = "Prosessi käynnissä";
        private const string STATE_CONNECTED_STRING = "Yhdistetty";
        private SolidColorBrush STATE_COLOR_GREEN = Brushes.Green;
        private SolidColorBrush STATE_COLOR_RED = Brushes.Red;

        private const int PARAMETERS_STATE_NOT_CONFIRMED = -1; 
        private const int PARAMETERS_STATE_INCORRECT = 0;
        private const int PARAMETERS_STATE_CONFIRMED = 1;

        // Tank parameters
        private const int TANK_MAX_L_VALUE = 300;
        private const int TANK_MIN_L_VALUE = 0;
        private const int TANK_MAX_P_VALUE = 350;
        private const int TANK_MIN_P_VALUE = 0;
        private const int TANK_MAX_T_VALUE = 80;
        private const int TANK_MIN_T_VALUE = 0;

        // Sequence UI parameters
        private const string PARAMETERS_NOT_CONFIRMED = "Lukitsemattomia muutoksia!";
        private const string PARAMETERS_INCORRECT = "Parametrit virheellisiä.";
        private const string PARAMETERS_CONFIRMED = "Muutokset tallennettu.";

        // Defaul file URL
        public const string PARAMETER_TEXTFILE_PATH = "\\default_parameter_values.txt";

        #endregion


        #region CLASS VARIABLES

        public int parameter_status = 0;

        public double default_Cooking_time;
        public int default_Cooking_time_min;
        public int default_Cooking_time_max;

        public double default_Cooking_temperature;
        public int default_Cooking_temperature_min;
        public int default_Cooking_temperature_max;

        public double default_Cooking_pressure;
        public int default_Cooking_pressure_min;
        public int default_Cooking_pressure_max;

        public double default_Impregnation_time;
        public int default_Impregnation_time_min;
        public int default_Impregnation_time_max;

        #endregion


        //#####################

        public MainWindow()
        {
            Logger logger = new Logger();   // Initializes the logger element.
            logger.WriteLog("Initializing UI...");

            InitializeComponent();

            InitUI(); // Init static UI elemets

            UpdateControl(); // Update system controls

            readDefaultParametersFromFile();    // Load default parameters from the file.

            InitSliders();

            ResetUIParameters();    // Loads the default parameter values

            new Thread(() => // Start control thread
            {
                Thread.CurrentThread.IsBackground = true;
                ControlThread();
            }).Start();

            logger.WriteLog("UI initialized.");
        }


        #region INITILIZATION

        private void InitUI()
        {
            // T100
            progressBar_T100.Maximum = TANK_MAX_L_VALUE;
            progressBar_T100.Minimum = TANK_MIN_L_VALUE;
            label_T100_max.Content = TANK_MAX_L_VALUE;
            label_T100_min.Content = TANK_MIN_L_VALUE;

            // T200
            progressBar_T200.Maximum = TANK_MAX_L_VALUE;
            progressBar_T200.Minimum = TANK_MIN_L_VALUE;
            label_T200_max.Content = TANK_MAX_L_VALUE;
            label_T200_min.Content = TANK_MIN_L_VALUE;

            // T300 - Pressure
            progressBar_T300_pressure.Maximum = TANK_MAX_P_VALUE;
            progressBar_T300_pressure.Minimum = TANK_MIN_P_VALUE;
            label_T300_pressure_max.Content = TANK_MAX_P_VALUE;
            label_T300_pressure_min.Content = TANK_MIN_P_VALUE;

            // T300 - Temperature
            progressBar_T300_temperature.Maximum = TANK_MAX_T_VALUE;
            progressBar_T300_temperature.Minimum = TANK_MIN_T_VALUE;
            label_T300_temperature_max.Content = TANK_MAX_T_VALUE;
            label_T300_temperature_min.Content = TANK_MIN_T_VALUE;

            // T400
            progressBar_T400.Maximum = TANK_MAX_L_VALUE;
            progressBar_T400.Minimum = TANK_MIN_L_VALUE;
            label_T400_max.Content = TANK_MAX_L_VALUE;
            label_T400_min.Content = TANK_MIN_L_VALUE;

           
        }
        
        private void InitSliders()
        {

            // Slider elements
            slider_cooking_time.Maximum = double.Parse(default_Cooking_time_max.ToString());
            slider_cooking_time.Minimum = double.Parse(default_Cooking_time_min.ToString());
            slider_cooking_pressure.Minimum = double.Parse(default_Cooking_pressure_min.ToString());
            slider_cooking_pressure.Maximum = double.Parse(default_Cooking_pressure_max.ToString());
            slider_cooking_temperature.Minimum = double.Parse(default_Cooking_temperature_min.ToString());
            slider_cooking_temperature.Maximum = double.Parse(default_Cooking_temperature_max.ToString());
            slider_impregnation_time.Maximum = double.Parse(default_Impregnation_time_max.ToString());
            slider_impregnation_time.Minimum = double.Parse(default_Impregnation_time_min.ToString());

            // Min and max label elements
            label_cooking_time_min.Content = default_Cooking_time_min.ToString();
            label_cooking_time_max.Content = default_Cooking_time_max.ToString();
            label_cooking_pressure_min.Content = default_Cooking_pressure_min.ToString();
            label_cooking_pressure_max.Content = default_Cooking_pressure_max.ToString();
            label_cooking_temperature_min.Content = default_Cooking_temperature_min.ToString();
            label_cooking_temperature_max.Content = default_Cooking_temperature_max.ToString();
            label_impregnation_time_min.Content = default_Impregnation_time_min.ToString();
            label_impregnation_time_max.Content = default_Impregnation_time_max.ToString();
        }

        #endregion


        #region UPDATES

        private void UpdateControl()
        {
            UpdateParameterUIStatus();
            switch (State)
            {
                case STATE_FAILSAFE:
                    button_connect.IsEnabled = false;
                    button_confirm_initial_state.Visibility = System.Windows.Visibility.Visible;
                    button_interrupt_process.IsEnabled = false;
                    button_set_parameters.IsEnabled = false;
                    button_reset_parameters.IsEnabled = false;
                    UpdateParameterControls(false);
                    label_connection_status.Content = STATE_DISCONNECTED_STRING; // TODO Dynamic value needed
                    label_connection_status.Foreground = STATE_COLOR_RED;
                    label_control_status.Content = STATE_FAILSAFE_STRING;
                    label_control_status.Foreground = STATE_COLOR_RED;
                    break;
                case STATE_DISCONNECTED:
                    button_connect.IsEnabled = true;
                    button_confirm_initial_state.Visibility = System.Windows.Visibility.Hidden;
                    button_interrupt_process.IsEnabled = false;
                    button_set_parameters.IsEnabled = false;
                    button_reset_parameters.IsEnabled = false;
                    UpdateParameterControls(false);
                    label_connection_status.Content = STATE_DISCONNECTED_STRING;
                    label_connection_status.Foreground = STATE_COLOR_RED;
                    label_control_status.Content = STATE_DISCONNECTED_STRING;
                    label_control_status.Foreground = STATE_COLOR_RED;
                    break;
                case STATE_IDLE:
                    button_connect.IsEnabled = false;
                    button_confirm_initial_state.Visibility = System.Windows.Visibility.Hidden;
                    button_interrupt_process.IsEnabled = false;
                    button_set_parameters.IsEnabled = true;
                    button_reset_parameters.IsEnabled = true;
                    UpdateParameterControls(true);
                    label_connection_status.Content = STATE_CONNECTED_STRING;
                    label_connection_status.Foreground = STATE_COLOR_GREEN;
                    label_control_status.Content = STATE_IDLE_STRING;
                    label_control_status.Foreground = STATE_COLOR_GREEN;
                    break;
                case STATE_RUNNING:
                    button_connect.IsEnabled = false;
                    button_confirm_initial_state.Visibility = System.Windows.Visibility.Hidden;
                    button_interrupt_process.IsEnabled = true;
                    button_set_parameters.IsEnabled = false;
                    button_reset_parameters.IsEnabled = false;
                    UpdateParameterControls(false);
                    label_connection_status.Content = STATE_CONNECTED_STRING;
                    label_connection_status.Foreground = STATE_COLOR_GREEN;
                    label_control_status.Content = STATE_RUNNING_STRING;
                    label_control_status.Foreground = STATE_COLOR_GREEN;
                    break;
                default:
                    Console.WriteLine("ERROR: UpdateControl() switch default statement called");
                    break;
            }
        }

        private void UpdateValues()
        {
            //Console.WriteLine("UpdateValues called");

            // Graphics 
            progressBar_T100.Value = mProcessClient.mData.LI100;
            progressBar_T200.Value = mProcessClient.mData.LI200;
            progressBar_T400.Value = mProcessClient.mData.LI400;
            progressBar_T300_pressure.Value = mProcessClient.mData.PI300;
            progressBar_T300_temperature.Value = mProcessClient.mData.TI300;

            // Numerical
            label_T100.Content = mProcessClient.mData.LI100;
            label_T200.Content = mProcessClient.mData.LI200;
            label_T400.Content = mProcessClient.mData.LI400;
            label_T300_pressure.Content = mProcessClient.mData.PI300;
            label_T300_temperature.Content = mProcessClient.mData.TI300;

        }

        private void UpdateParameterControls(bool isEnabled)
        {
            // Update slider controls status
            slider_cooking_pressure.IsEnabled = isEnabled;
            slider_cooking_temperature.IsEnabled = isEnabled;
            slider_cooking_time.IsEnabled = isEnabled;
            slider_impregnation_time.IsEnabled = isEnabled;
            
            // Update textbox slider status
            textBox_cooking_pressure.IsEnabled = isEnabled;
            textBox_cooking_temperature.IsEnabled = isEnabled;
            textBox_cooking_time.IsEnabled = isEnabled;
            textBox_impregnation_time.IsEnabled = isEnabled;
        }

        #endregion

        
        #region USER INTERFACE EVENTS

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Lets also update the sliders.
            try
            {
                slider_cooking_time.Value = Math.Round(double.Parse(textBox_cooking_time.Text), 1, MidpointRounding.ToEven);
                slider_cooking_temperature.Value = Math.Round(double.Parse(textBox_cooking_temperature.Text), 1, MidpointRounding.ToEven);
                slider_cooking_pressure.Value = Math.Round(double.Parse(textBox_cooking_pressure.Text), 1, MidpointRounding.ToEven);
                slider_impregnation_time.Value = Math.Round(double.Parse(textBox_impregnation_time.Text), 1, MidpointRounding.ToEven);

                parameter_status = PARAMETERS_STATE_NOT_CONFIRMED;

                UpdateControl();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBox_cooking_time.Text = slider_cooking_time.Value.ToString();
            textBox_cooking_pressure.Text = slider_cooking_pressure.Value.ToString();
            textBox_cooking_temperature.Text = slider_cooking_temperature.Value.ToString();
            textBox_impregnation_time.Text = slider_impregnation_time.Value.ToString();

            parameter_status = PARAMETERS_STATE_NOT_CONFIRMED;

            UpdateControl();
        }

        private void button_start_process_Click(object sender, RoutedEventArgs e)
        {
            // TODO

            State = STATE_RUNNING;

            UpdateControl();
        }

        private void button_interrupt_process_Click(object sender, RoutedEventArgs e)
        {
            InterruptProcess();
        }

        private void button_connect_Click(object sender, RoutedEventArgs e)
        {
            // TODO 
            mProcessClient.ConnectOPCUA();
            UpdateControl();
        }

        private void button_set_parameters_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cooking_time = double.Parse(textBox_cooking_time.Text);
                Cooking_pressure = double.Parse(textBox_cooking_pressure.Text);
                Cooking_temperature = double.Parse(textBox_cooking_temperature.Text);
                Impregnation_time = double.Parse(textBox_impregnation_time.Text);

                parameter_status = PARAMETERS_STATE_CONFIRMED;

                UpdateControl();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                parameter_status = PARAMETERS_STATE_INCORRECT;

                UpdateControl();
            }

        }

        private void button_reset_parameters_Click(object sender, RoutedEventArgs e)
        {
            ResetUIParameters();

        }

        private void button_confirm_initial_state_Click(object sender, RoutedEventArgs e)
        {
            State = STATE_DISCONNECTED;
        }

        #endregion


        #region PARAMETER HANDLING

        private void readDefaultParametersFromFile()
        {
            try
            {
                string basedirectory = AppDomain.CurrentDomain.BaseDirectory;
                for (int i = 0; i <= 3; i++) // TODO modular
                {
                    basedirectory = Directory.GetParent(basedirectory).ToString();
                }
                string parameters_filepath = basedirectory + PARAMETER_TEXTFILE_PATH;
                Console.WriteLine(parameters_filepath);
                string[] lines = File.ReadAllLines(parameters_filepath);
                string[] parameters;

                foreach (string line in lines)
                {
                    parameters = line.Split('=');
                    declareDefaultParameters(parameters);
                }
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex.Message);
                logger.WriteLog("Using zeros as a default values instead.");

                // Lets disable the sliders, if the default-textfile is corrupted or missing.

                default_Cooking_time = 0;
                default_Cooking_time_min = -1;
                default_Cooking_time_max = 1;
                slider_cooking_time.IsEnabled = false;

                default_Cooking_temperature = 0;
                default_Cooking_temperature_min = -1;
                default_Cooking_temperature_max = 1;
                slider_cooking_temperature.IsEnabled = false;

                default_Cooking_pressure = 0;
                default_Cooking_pressure_min = -1;
                default_Cooking_pressure_max = 1;
                slider_cooking_pressure.IsEnabled = false;

                default_Impregnation_time = 0;
                default_Impregnation_time_min = -1;
                default_Impregnation_time_max = 1;
                slider_impregnation_time.IsEnabled = false;
            }
        }

        private void declareDefaultParameters(string[] parameters)
        {
            string min_and_max_string;
            string[] min_and_max_array;

            // Lets separate the key arguments, default values and default value borders from each other.
            min_and_max_string = parameters[2];
            char[] charsToTrim = { '[', ' ', ']' };
            min_and_max_string = min_and_max_string.Trim(charsToTrim);
            min_and_max_array = min_and_max_string.Split(',');
            
            if (parameters[0] == "default_Cooking_time") // TODO switch?
            {
                default_Cooking_time = double.Parse(parameters[1]);
                default_Cooking_time_min = Int32.Parse(min_and_max_array[0]);
                default_Cooking_time_max = Int32.Parse(min_and_max_array[1]);
                logger.WriteLog("Default cooking time loaded.");
            }
            else if (parameters[0] == "default_Cooking_temperature")
            {
                default_Cooking_temperature = double.Parse(parameters[1]);
                default_Cooking_temperature_min = Int32.Parse(min_and_max_array[0]);
                default_Cooking_temperature_max = Int32.Parse(min_and_max_array[1]);
                logger.WriteLog("Default cooking temperature loaded.");
            }
            else if (parameters[0] == "default_Cooking_pressure")
            {
                default_Cooking_pressure = double.Parse(parameters[1]);
                default_Cooking_pressure_min = Int32.Parse(min_and_max_array[0]);
                default_Cooking_pressure_max = Int32.Parse(min_and_max_array[1]);
                logger.WriteLog("Default cooking pressure loaded.");
            }
            else if (parameters[0] == "default_Impregnation_time")
            {
                default_Impregnation_time = double.Parse(parameters[1]);
                default_Impregnation_time_min = Int32.Parse(min_and_max_array[0]);
                default_Impregnation_time_max = Int32.Parse(min_and_max_array[1]);
                logger.WriteLog("Default Impregnation time loaded.");
            }
        }

        private void UpdateParameterUIStatus()
        {
            if (parameter_status == PARAMETERS_STATE_CONFIRMED)
            {
                textblock_parameter_status.Text = PARAMETERS_CONFIRMED;
                textblock_parameter_status.Foreground = STATE_COLOR_GREEN;
                button_start_process.IsEnabled = true;
            }
            else if (parameter_status == PARAMETERS_STATE_INCORRECT)
            {
                textblock_parameter_status.Text = PARAMETERS_INCORRECT;
                textblock_parameter_status.Foreground = STATE_COLOR_RED;
                button_start_process.IsEnabled = false;
            }
            else if (parameter_status == PARAMETERS_STATE_NOT_CONFIRMED)
            {
                textblock_parameter_status.Text = PARAMETERS_NOT_CONFIRMED;
                textblock_parameter_status.Foreground = STATE_COLOR_RED;
                button_start_process.IsEnabled = false;
            }
        }

        private void ResetUIParameters()
        {
            try
            {
                textBox_cooking_pressure.Text = default_Cooking_pressure.ToString();
                textBox_cooking_time.Text = default_Cooking_time.ToString();
                textBox_cooking_temperature.Text = default_Cooking_temperature.ToString();
                textBox_impregnation_time.Text = default_Impregnation_time.ToString();
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex.Message);
            }
        }

        #endregion

    }
}
