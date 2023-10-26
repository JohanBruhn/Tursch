using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tursch.Domain.Models;

namespace Tursch.WPF.Services
{
    internal class SignalRClientService
    {
        private readonly HubConnection _connection;

        // Listen for server start confirmation (when hosting a lobby)
        public event Action? StartedServerConfirmationReceived;

        // Listen for player list update
        public event Action<List<string>>? PlayerListUpdateReceived;

        // Listen for lobby closure
        public event Action? DisbandLobbyReceived;

        // Listen for game start, receive serialized playerinfolist
        public event Action<List<string>>? StartGameReceived;

        // Listen for dealt hand
        public event Action<List<string>, int>? DealtHandReceived;

        // Listen for swap confirmation
        public event Action<List<string>, List<string>>? SwapConfirmationReceived;
        // Listen for other player swap
        public event Action<int> OtherPlayerSwapReceived;

        // Listen for begin play message
        public event Action<int> BeginPlayReceived;

        public event Action<List<string>> PlayConfirmationReceived;

        public event Action<int, int, float, List<List<string>>> EndOfGameReceived;

        public event Action<List<List<string>>> InitialDealReceived;
        

        public SignalRClientService(HubConnection connection)
        {
            _connection = connection;

            // Lobby listeners
            _connection.On("ClientReceiveStartedServerConfirmation", () => StartedServerConfirmationReceived?.Invoke());
            // Listens for ReceivePlayerList and invokes RegistryResultReceived in Host- and JoinLobbyViewModel classes
            _connection.On<List<string>>("ClientReceivePlayerList", (playerNameList) => PlayerListUpdateReceived?.Invoke(playerNameList));
            // _connection.On<GameState>("MethodName", (data) => EventName?.Invoke(data));
            _connection.On("ClientReceiveDisbandLobby", () => DisbandLobbyReceived?.Invoke());

            // Json string is deserialized in event handler (in HostLobbyViewModel and JoinLobbyViewModel)
            _connection.On<List<string>>("ClientReceiveStartGame", (jsonPlayerInfo) => StartGameReceived?.Invoke(jsonPlayerInfo));

            _connection.On<List<string>, int>("GameClientReceiveHand", (hand, activePlayerSeatNumber) => DealtHandReceived?.Invoke(hand, activePlayerSeatNumber));

            _connection.On<List<string>, List<string>>("GameClientReceiveSwapConfirmation", (cards, newCards) => SwapConfirmationReceived?.Invoke(cards, newCards));
            _connection.On<int>("GameClientReceiveOtherPlayerSwap", (swappedNumber) => OtherPlayerSwapReceived?.Invoke(swappedNumber));

            _connection.On<int>("GameClientReceiveBeginPlayMessage", (activePlayerSeatNumber) => BeginPlayReceived?.Invoke(activePlayerSeatNumber));

            _connection.On<List<string>>("GameClientReceivePlayConfirmation", (cards) => PlayConfirmationReceived?.Invoke(cards));

            _connection.On<int, int, float, List<List<string>>>("GameClientReceiveEndOfGame", (winner, loser, balanceChange, flippedCardLists) => EndOfGameReceived?.Invoke(winner, loser, balanceChange, flippedCardLists));

            _connection.On<List<List<string>>>("GameClientReceiveInitialDeal", (dealtCards) => InitialDealReceived?.Invoke(dealtCards));
        }

        internal async Task GameClientSendPerformActionRequest(List<string> cardStrings)
        {
            await _connection.SendAsync("GameClientSendPerformActionRequest", cardStrings);
        }

        public async Task Connect()
        {
            await _connection.StartAsync();
        }

        public async Task ClientSendRegisterRequest(string playerName)
        {
            Console.WriteLine("ClientService sending register request for player " + playerName); // ------------------------------------- Testing
            await _connection.SendAsync("ClientSendRegisterRequest", playerName);
        }

        public async Task ClientSendLeaveLobbyRequest()
        {
            Console.WriteLine("ClientService sending leave request");
            await _connection.SendAsync("ClientSendLeaveLobbyRequest");
        }

        public async Task ClientSendDisbandLobbyCommand()
        {
            Console.WriteLine("ClientService sending disband lobby command");
            await _connection.SendAsync("ClientSendDisbandLobbyCommand");
        }

        public async Task ClientSendStartGameCommand()
        {
            Console.WriteLine("ClientService sending start game command");
            await _connection.SendAsync("ClientSendStartGameCommand");
        }
    }
}
