using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Tursch.Domain.Models;
using Tursch.Server.Services;
using Tursch.Server.ViewModels;

namespace Tursch.Server.Commands
{
    internal class SendGameServerDealHandsCommand : ICommand
    {
        private readonly ServerViewModel _viewModel;
        private readonly SignalRServerService _serverService;

        public SendGameServerDealHandsCommand(ServerViewModel viewModel, SignalRServerService serverService)
        {
            _viewModel = viewModel;
            _serverService = serverService;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }


        public async void Execute(object? parameter)
        {
            try
            {
                List<string> connectionIDs = new List<string>();
                List<List<string>> hands = new List<List<string>>();
                _viewModel.Messages.Add("Getting IDs and hands");
                foreach (Player player in _viewModel.PlayerList)
                {
                    connectionIDs.Add(player.GetConnectionID());
                    hands.Add(player.GetHandToString());
                }
                Player player1 = _viewModel.ActiveGame.ActivePlayer;
                _viewModel.Messages.Add("Getting seat number");
                
                int activePlayerSeatNumber = (int)parameter;

                _viewModel.Messages.Add("Deal winner is seat: " + activePlayerSeatNumber + ", sending to hub");
                await _serverService.GameServerSendHands(connectionIDs, hands, activePlayerSeatNumber);

                // _viewModel.ErrorMessage = string.Empty;
            }
            catch (Exception)
            {
                _viewModel.Messages.Add("Failed to execute part of command");
                // _viewModel.ErrorMessage = "Unable to register";
            }
        }
    }
}
