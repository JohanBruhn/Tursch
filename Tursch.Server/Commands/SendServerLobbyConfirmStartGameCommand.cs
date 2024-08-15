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
    internal class SendServerLobbyConfirmStartGameCommand : ICommand
    {
        private readonly ServerViewModel _viewModel;
        private readonly SignalRServerService _serverService;

        public SendServerLobbyConfirmStartGameCommand(ServerViewModel viewModel, SignalRServerService serverService)
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
                List<PlayerInfo> playerInfo = _viewModel.GetPlayerInfoList();
                int dealWinner = (int)parameter;
                _viewModel.Messages.Add("Sending start game confirmation with info list of length " + playerInfo.Count + ", dealWinner is " + dealWinner); // ------------------------------- Temp
                // ^ This happens
                await _serverService.ServerSendStartGameConfirmation(playerInfo, dealWinner);

                // _viewModel.ErrorMessage = string.Empty;
            }
            catch (Exception e)
            {
                _viewModel.Messages.Add("Unable to send confirmation, error: " + e); // ----------------------- Temp
                // _viewModel.ErrorMessage = "Unable to register";
            }
        }
    }
}
