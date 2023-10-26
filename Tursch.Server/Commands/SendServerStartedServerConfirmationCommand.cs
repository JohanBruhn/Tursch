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
    internal class SendServerStartedServerConfirmationCommand : ICommand
    {
        private readonly ServerViewModel _viewModel;
        private readonly SignalRServerService _serverService;

        public SendServerStartedServerConfirmationCommand(ServerViewModel viewModel, SignalRServerService serverService)
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
                await _serverService.ServerSendStartedServerConfirmation();

                // _viewModel.ErrorMessage = string.Empty;
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to confirm server start"); // ----------------------- Temp
                // _viewModel.ErrorMessage = "Unable to register";
            }
        }


    }
}
