using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Tursch.Server.Services;
using Tursch.Server.ViewModels;

namespace Tursch.Server.Commands
{
    internal class SendServerLobbyConfirmLeaveCommand : ICommand
    {
        private readonly ServerViewModel _viewModel;
        private readonly SignalRServerService _serverService;

        public SendServerLobbyConfirmLeaveCommand(ServerViewModel viewModel, SignalRServerService serverService)
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
                string connectionID = (string)parameter;
                if (connectionID == null)
                {
                    throw new ArgumentNullException();
                }
                _viewModel.Messages.Add("Sending leave confirmation"); // ------------------------------- Temp
                await _serverService.ServerSendLeaveLobbyConfirmation(connectionID, _viewModel.GetPlayerNameList());

                // _viewModel.ErrorMessage = string.Empty;
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to confirm registration"); // ----------------------- Temp
                // _viewModel.ErrorMessage = "Unable to register";
            }
        }
    }
}
