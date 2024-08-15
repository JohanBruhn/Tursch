﻿using System;
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
    internal class SendGameServerInitialDealCommand : ICommand
    {
        private readonly ServerViewModel _viewModel;
        private readonly SignalRServerService _serverService;

        public SendGameServerInitialDealCommand(ServerViewModel viewModel, SignalRServerService serverService)
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
                List<List<string>> dealtCards = new();

                foreach (Player player in _viewModel.PlayerList)
                {
                    dealtCards.Add(player.GetDealToString());
                }
                int dealWinner = (int)parameter;

                _viewModel.Messages.Add("Sending initial deal to " + dealtCards.Count + " players"); // ------------------------------- Temp
                await _serverService.GameServerSendInitialDeal(dealtCards, dealWinner);

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
