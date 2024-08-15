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
    internal class SendGameClientStartGameCommand : ICommand
    {
        private readonly GameViewModel _viewModel;
        private readonly SignalRClientService _clientService;

        public SendGameClientStartGameCommand(GameViewModel viewModel, SignalRClientService clientService)
        {
            _viewModel = viewModel;
            _clientService = clientService;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true; // TODO: IMPLEMENT
        }

        public async void Execute(object? parameter)
        {
            try
            {
                Console.WriteLine("Sending start next round command"); // ------------------------------- Temp
                await _clientService.ClientSendStartGameCommand(); // Requests that the server start the next round of play
                _viewModel.NextRoundButtonVisibility = System.Windows.Visibility.Hidden; // Hides the next round button

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
