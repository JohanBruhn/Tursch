using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Tursch.Domain.Models;
using Tursch.WPF.Commands;
using Tursch.WPF.Services;
using Tursch.WPF.Views;

namespace Tursch.WPF.ViewModels
{
    // Will behave very similarly to HostLobbyViewModel except can't start or disband lobby, just leave and listen for lobby updates and game start
    // Generate through App.JoinServer, similarly to how HostLobbyViewModel is created
    // TODO: Add receive game start feature
    internal class JoinLobbyViewModel : ViewModelBase
    
    {
        private SignalRClientService _clientService;
        public SignalRClientService ClientService
        {
            get { return _clientService; }
        }

        private string _lobbyTitle;
        public string LobbyTitle
        {
            get { return _lobbyTitle; }

            set
            {
                _lobbyTitle = value;
                OnPropertyChanged(nameof(LobbyTitle));
            }
        }

        private string _playerName;
        public string PlayerName
        {
            get { return _playerName; }

            set
            {
                _playerName = value;
                OnPropertyChanged(nameof(PlayerName));
            }
        }

        private string _hostName;
        public string HostName
        {
            get { return _hostName; }

            set
            {
                _hostName = value;
                OnPropertyChanged(nameof(HostName));
            }
        }

        public ObservableCollection<string> PlayerList { get; }

        public ICommand SendJoinLobbyRegisterPlayerCommand { get; }

        public ICommand SendJoinLobbyLeaveCommand { get; }


        // Constructor
        public JoinLobbyViewModel(SignalRClientService clientService, string playerName)
        {
            _clientService = clientService;
            SendJoinLobbyRegisterPlayerCommand = new SendJoinLobbyRegisterPlayerCommand(this, clientService);
            SendJoinLobbyLeaveCommand = new SendJoinLobbyLeaveCommand(this, clientService);

            HostName = "";
            LobbyTitle = "Empty lobby";
            PlayerName = playerName;
            PlayerList = new ObservableCollection<string>();

            _clientService.PlayerListUpdateReceived += ClientService_PlayerListUpdateReceived;
            _clientService.DisbandLobbyReceived += ClientService_DisbandLobbyReceived;
            _clientService.StartGameReceived += ClientService_StartGameReceived;
        }

        

        public override void Dispose()
        {
            _clientService.PlayerListUpdateReceived -= ClientService_PlayerListUpdateReceived;
            _clientService.DisbandLobbyReceived -= ClientService_DisbandLobbyReceived;
            _clientService.StartGameReceived -= ClientService_StartGameReceived;

            base.Dispose();
        }

        private void ClientService_StartGameReceived(List<string> jsonPlayerInfo)
        {
            this.Dispose();
            GameView gameView = new GameView();

            // Deserialize list of json strings
            List<PlayerInfo> playerInfo = new();
            foreach (string player in jsonPlayerInfo)
            {
                playerInfo.Add(JsonSerializer.Deserialize<PlayerInfo>(player));
            }

            GameViewModel gameViewModel = App.StartGame(ClientService, playerInfo, PlayerName);
            gameView.DataContext = gameViewModel;
            App.GetMainWindow((App)App.Current).MainWindowFrame.NavigationService.Navigate(gameView);
        }

        private void ClientService_DisbandLobbyReceived()
        {
            App.GetMainWindow((App)App.Current).MainWindowFrame.NavigationService.Navigate(new JoinSetupView(PlayerName));
        }

        public static JoinLobbyViewModel CreateConnectedViewModel(SignalRClientService clientService, string playerName)
        {
            JoinLobbyViewModel viewModel = new JoinLobbyViewModel(clientService, playerName);

            clientService.Connect().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    Console.WriteLine("Unable to connect");
                    Console.WriteLine(task.Exception); // ------------------------------- Testing
                    // viewModel.ErrorMessage = "Unable to connect"
                }
            });

            viewModel.SendJoinLobbyRegisterPlayerCommand.Execute(null);

            return viewModel;
        }


        private void ClientService_PlayerListUpdateReceived(List<string> playerNameList)
        {
            HostName = playerNameList.First();
            LobbyTitle = HostName + "'s lobby";

            PlayerList.Clear();
            foreach (string playerName in playerNameList)
            {
                PlayerList.Add(playerName);
            }

        }

        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }

            set
            {
                _isConnected = value;
                OnPropertyChanged(nameof(IsConnected));
            }
        }
    }
}
