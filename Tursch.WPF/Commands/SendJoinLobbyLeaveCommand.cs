using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Tursch.WPF.Services;
using Tursch.WPF.ViewModels;

namespace Tursch.WPF.Commands
{
    internal class SendJoinLobbyLeaveCommand : ICommand
    {
        private readonly JoinLobbyViewModel _viewModel;
        private readonly SignalRClientService _clientService;

        public SendJoinLobbyLeaveCommand(JoinLobbyViewModel viewModel, SignalRClientService clientService)
        {
            _viewModel = viewModel;
            _clientService = clientService;
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
                await _clientService.ClientSendLeaveLobbyRequest();

                // _viewModel.ErrorMessage = string.Empty;
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to register"); // ----------------------- Temp
                // _viewModel.ErrorMessage = "Unable to register";
            }
        }
    }
}
