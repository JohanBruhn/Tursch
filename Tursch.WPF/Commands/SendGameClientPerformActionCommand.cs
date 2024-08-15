using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Tursch.WPF.Services;
using Tursch.WPF.ViewModels;

namespace Tursch.WPF.Commands
{
    internal class SendGameClientPerformActionCommand : ICommand
    {
        private readonly GameViewModel _viewModel;
        private readonly SignalRClientService _clientService;

        public SendGameClientPerformActionCommand(GameViewModel viewModel, SignalRClientService clientService)
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
                
                Console.WriteLine("Sending perform action command"); // ------------------------------- Temp
                if(_viewModel.ActivePlayer == 0) // Limits unnecessary server queries
                {
                    List<string> cardStrings = new List<string>(); // Selected cards to send to server

                    foreach (CardViewModel card in _viewModel.Seat0.SelectedCards)
                    {
                        cardStrings.Add(card.CardName);
                    }

                    if(cardStrings.Count > 0 || _viewModel.IsSwapRound) // Limits unnecessary server queries
                    {
                        await _clientService.GameClientSendPerformActionRequest(cardStrings); // Requests to play/swap selected cards
                    }
                    // TODO?: Implement this in clientservice and hub and then receive in serverservice and serverviewmodel and send back gamestate update (player n swaps/plays these cards)

                }

                // _viewModel.ErrorMessage = string.Empty;
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to send command"); // ----------------------- Temp
                // _viewModel.ErrorMessage = "Unable to register";
            }
        }
    }
}
