using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Tursch.WPF.Services;
using Tursch.WPF.ViewModels;
using System.Diagnostics;
using System.IO; // Temporary for testing
using Tursch.Domain.Models;
using Microsoft.AspNetCore.Http.Connections;

namespace Tursch.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Process? _signalRProcess;
        private static Process? _serverProcess;

        protected override void OnStartup(StartupEventArgs e)
        {
            Process[] signalRProcessArray = Process.GetProcessesByName("Tursch.SignalR");
            if (signalRProcessArray.Count() != 0)
            {
                _signalRProcess = signalRProcessArray[0];
            }
            else
            {
                _signalRProcess = Process.Start("Tursch.SignalR.exe"); // FOR RELEASE
                //_signalRProcess = Process.Start("C:/Users/Johan.Bruhns-dator/source/repos/Tursch/Tursch.SignalR/bin/x64/Release/net6.0/Tursch.SignalR.exe"); // FOR DEBUG
                // _signalRProcess = Process.Start("full/path/to/Tursch.SignalR.exe"); // FOR DEBUG
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serverProcess?.Kill();
            _signalRProcess?.Kill();
            base.OnExit(e);
        }

       

        internal static void CloseServer()
        {
            _serverProcess?.Kill();
        }

        internal static MainWindow GetMainWindow(App app)
        {
            return (MainWindow) app.MainWindow;
        }

        internal static HostLobbyViewModel StartServer(string hostIP, string hostPort, string playerName)
        {
            
            // Default IP and port
            if (string.IsNullOrWhiteSpace(hostIP))
            {
                hostIP = "127.0.0.1";
            }
            if (string.IsNullOrWhiteSpace(hostPort))
            {
                hostPort = "5000";
            }

            // Create connected (to hub) but unregistered (on server) client, which will then wait until it gets server started message before sending register request
            HubConnection clientConnection = new HubConnectionBuilder()
                .WithUrl("http://" + hostIP + ":" + hostPort + "/Tursch", options => {
                    options.SkipNegotiation = true;
                    options.Transports = HttpTransportType.WebSockets;
                }).Build(); // Port 5052 not working? only 5000?

            HostLobbyViewModel lobbyViewModel = HostLobbyViewModel.CreateConnectedViewModel(new SignalRClientService(clientConnection), playerName, hostIP, hostPort);


            string[] args = { hostIP, hostPort, playerName };
            _serverProcess = Process.Start("Tursch.Server.exe", args); // FOR RELEASE
            //_serverProcess = Process.Start("C:/Users/Johan.Bruhns-dator/source/repos/Tursch/Tursch.Server/bin/x64/Debug/net6.0-windows/Tursch.Server.exe", args); // FOR DEBUG
            // _signalRProcess = Process.Start("full/path/to/Tursch.Server.exe"); // FOR DEBUG



            return lobbyViewModel;
        }

        internal static JoinLobbyViewModel JoinServer(string joinIP, string joinPort, string playerName)
        {
            // Default IP and port
            if (string.IsNullOrWhiteSpace(joinIP))
            {
                joinIP = "127.0.0.1";
            }
            if (string.IsNullOrWhiteSpace(joinPort))
            {
                joinPort = "5000";
            }

            HubConnection connection = new HubConnectionBuilder()
                .WithUrl("http://" + joinIP + ":" + joinPort + "/Tursch", options => {
                    options.SkipNegotiation = true;
                    options.Transports = HttpTransportType.WebSockets;
                }).Build(); // Port 5052 not working? only 5000?

            JoinLobbyViewModel lobbyViewModel = JoinLobbyViewModel.CreateConnectedViewModel(new SignalRClientService(connection), playerName, joinIP, joinPort);

            return lobbyViewModel;
        }

        internal static GameViewModel StartGame(SignalRClientService clientService, List<PlayerInfo> playerInfo, string playerName, int dealWinner)
        {
            return new GameViewModel(clientService, playerInfo, playerName, dealWinner);
        }
    }
}
