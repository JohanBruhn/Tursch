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
    // Potentially possible to merge RegisterPlayer commands while preserving ViewModel parameter, doable with string parameter but loses functionality (e.g. ErrorMessage)
    internal class SendJoinLobbyRegisterPlayerCommand : ICommand
    {
        private readonly JoinLobbyViewModel _viewModel;
        private readonly SignalRClientService _clientService;

        public SendJoinLobbyRegisterPlayerCommand(JoinLobbyViewModel viewModel, SignalRClientService clientService)
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
                await _clientService.ClientSendRegisterRequest(_viewModel.PlayerName);

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
