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
    internal class SendHostLobbyDisbandLobbyCommand : ICommand
    {
        private readonly HostLobbyViewModel _viewModel;
        private readonly SignalRClientService _clientService;

        public SendHostLobbyDisbandLobbyCommand(HostLobbyViewModel viewModel, SignalRClientService clientService)
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
                Console.WriteLine("Sending disband command"); // ------------------------------- Temp
                await _clientService.ClientSendDisbandLobbyCommand(); // Requests server to disband the lobby

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
