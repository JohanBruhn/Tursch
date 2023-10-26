﻿using System;
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
    // TODO: Add start game and receive game start feature
    internal class HostLobbyViewModel : ViewModelBase
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


        //private string[] _playerList;
        //public string[] PlayerList
        //{
        //    get { return _playerList; }
        //
        //    set
        //    {
        //        _playerList = value;
        //        OnPropertyChanged(nameof(PlayerList));
        //    }
        //}


        public ObservableCollection<string> PlayerList { get; }

        public ICommand SendHostLobbyRegisterPlayerCommand { get; }

        public ICommand SendHostLobbyDisbandLobbyCommand { get; }

        public ICommand SendHostLobbyStartGameCommand { get; }

        // Static construction method, creates HostLobbyViewModel object connected to SignalR hub but not registered on server.
        public static HostLobbyViewModel CreateConnectedViewModel(SignalRClientService clientService, string playerName)
        {
            HostLobbyViewModel viewModel = new HostLobbyViewModel(clientService, playerName);

            clientService.Connect().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    Console.WriteLine("Unable to connect");
                    Console.WriteLine(task.Exception); // ------------------------------- Testing
                    // viewModel.ErrorMessage = "Unable to connect"
                }
            });

            return viewModel;
        }

        // Constructor
        public HostLobbyViewModel(SignalRClientService clientService, string playerName)
        {
            _clientService = clientService;
            SendHostLobbyRegisterPlayerCommand = new SendHostLobbyRegisterPlayerCommand(this, clientService);
            SendHostLobbyDisbandLobbyCommand = new SendHostLobbyDisbandLobbyCommand(this, clientService);
            SendHostLobbyStartGameCommand = new SendHostLobbyStartGameCommand(this, clientService);

            HostName = playerName;
            LobbyTitle = "Empty lobby";
            PlayerName = playerName;
            PlayerList = new ObservableCollection<string>();

            _clientService.StartedServerConfirmationReceived += ClientService_StartedServerConfirmationReceived;
            _clientService.PlayerListUpdateReceived += ClientService_PlayerListUpdateReceived;
            _clientService.DisbandLobbyReceived += ClientService_DisbandLobbyReceived;
            _clientService.StartGameReceived += ClientService_StartGameReceived;
        }

        

        public override void Dispose()
        {
            _clientService.StartedServerConfirmationReceived -= ClientService_StartedServerConfirmationReceived;
            _clientService.PlayerListUpdateReceived -= ClientService_PlayerListUpdateReceived;
            _clientService.DisbandLobbyReceived -= ClientService_DisbandLobbyReceived;
            _clientService.StartGameReceived -= ClientService_StartGameReceived;

            base.Dispose();
        }

        private void ClientService_StartGameReceived(List<string> jsonPlayerInfo)
        {
            this.Dispose();
            Console.WriteLine("Client received start game confirmation");
            GameView gameView = new GameView();


            // Deserialize list of json strings
            List<PlayerInfo> playerInfo = new();
            PlayerInfo? currentPlayerInfo;
            foreach (string player in jsonPlayerInfo)
            {
                currentPlayerInfo = JsonSerializer.Deserialize<PlayerInfo>(player);
                if(currentPlayerInfo != null)
                {
                    playerInfo.Add(currentPlayerInfo);
                }
            }

            GameViewModel gameViewModel = App.StartGame(ClientService, playerInfo, PlayerName);
            gameView.DataContext = gameViewModel;
            App.GetMainWindow((App)App.Current).MainWindowFrame.NavigationService.Navigate(gameView);
        }



        // When confirmation of server start is received, attempt registration
        private void ClientService_StartedServerConfirmationReceived()
        {
            Console.WriteLine("Received started server confirmation");
            this.SendHostLobbyRegisterPlayerCommand.Execute(null);
            Console.WriteLine("Executed send host lobby register player command");
        }

        // If registration was successful, receive playerNameList and update view
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

        private void ClientService_DisbandLobbyReceived()
        {
            // App.CloseServer();
            this.Dispose();
            App.CloseServer();
            App.GetMainWindow((App)App.Current).MainWindowFrame.NavigationService.Navigate(new HostSetupView(PlayerName));
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
