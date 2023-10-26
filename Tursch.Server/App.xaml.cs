using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Tursch.Server.Services;
using Tursch.Server.ViewModels;

namespace Tursch.Server
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            string hostIP = e.Args[0];
            string hostPort = e.Args[1];

            HubConnection serverConnection = new HubConnectionBuilder()
                .WithUrl("http://" + hostIP + ":" + hostPort + "/Tursch", options => {
                    options.SkipNegotiation = true;
                    options.Transports = HttpTransportType.WebSockets;
                }).Build(); // Port 5052 not working? only 5000?
            ServerViewModel serverViewModel = ServerViewModel.CreateConnectedServerViewModel(new SignalRServerService(serverConnection));

            MainWindow window = new MainWindow
            {
                DataContext = new MainViewModel(serverViewModel)
            };

            window.Show();
        }
        

    }
}
