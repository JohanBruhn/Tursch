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
    internal class SendGameServerEndOfGameCommand : ICommand
    {
        private readonly ServerViewModel _viewModel;
        private readonly SignalRServerService _serverService;

        public SendGameServerEndOfGameCommand(ServerViewModel viewModel, SignalRServerService serverService)
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
                (int winner, int loser, float balanceChange, List<List<string>> flippedCardLists) = ((int, int, float, List<List<string>>))parameter;
                await _serverService.GameServerSendEndOfGameMessage(winner, loser, balanceChange, flippedCardLists);

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
