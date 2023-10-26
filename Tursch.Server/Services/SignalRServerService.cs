using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tursch.Domain.Models;

namespace Tursch.Server.Services
{
    internal class SignalRServerService
    {
        private readonly HubConnection _connection;

        // Listen for register requests
        public event Action<string, string, string> RegisterRequestReceived;

        // Listen for leave requests
        public event Action<string> LeaveRequestReceived;

        // Listen for start game command
        public event Action StartGameReceived;

        // Listen for GetGameState requests

        // Listen for gameplay input
        public event Action<string, List<string>> PerformActionRequestReceived;

        public SignalRServerService(HubConnection connection)
        {
            _connection = connection;

            // Listens for ReceivePlayerList and invokes RegistryResultReceived in Host- and JoinLobbyViewModel classes
            _connection.On<string, string, string>("ServerReceiveRegisterRequest", (connectionID, userID, playerName) => RegisterRequestReceived?.Invoke(connectionID, userID, playerName));
            // _connection.On<GameState>("MethodName", (data) => EventName?.Invoke(data));
            _connection.On<string>("ServerReceiveLeaveRequest", (connectionID) => LeaveRequestReceived?.Invoke(connectionID));

            _connection.On("ServerReceiveStartGameCommand", () => StartGameReceived?.Invoke());

            _connection.On<string, List<string>>("GameServerReceivePerformActionRequest", (connectionID, cards) => PerformActionRequestReceived?.Invoke(connectionID, cards));

        }

        // Start methods
        public async Task Connect()
        {
            await _connection.StartAsync();
        }

        public async Task EstablishGameServer()
        {
            await _connection.SendAsync("EstablishGameServer");
        }

        public async Task ServerSendStartedServerConfirmation()
        {
            await _connection.SendAsync("ServerSendStartedServerConfirmation");
        }

        // Lobby methods
        public async Task ServerSendRegisterConfirmation(string connectionID, List<string> playerNameList)
        {
            Console.WriteLine("Server sending register confirmation"); // ------------------------------------- Testing
            await _connection.SendAsync("ServerSendRegisterConfirmation", connectionID, playerNameList);
        }

        public async Task ServerSendLeaveLobbyConfirmation(string connectionID, List<string> playerNameList)
        {
            Console.WriteLine("Server sending leave confirmation");
            List<string> bannedNames = new List<string>{ "andrija", "wp" };
            await _connection.SendAsync("ServerSendLeaveLobbyConfirmation", connectionID, playerNameList);
        }

        public async Task ServerSendStartGameConfirmation(List<PlayerInfo> playerInfo)
        {
            Console.WriteLine("Server serializing player info and sending start game confirmation");
            List<string> jsonPlayerInfo = new();
            foreach (PlayerInfo player in playerInfo)
            {
                jsonPlayerInfo.Add(JsonSerializer.Serialize(player));
            }
            await _connection.SendAsync("ServerSendStartGameConfirmation", jsonPlayerInfo);
        }

        // Game methods
        public async Task GameServerSendHands(List<string> connectionIDs, List<List<string>> hands, int activePlayerSeatNumber)
        {
            await _connection.SendAsync("GameServerSendHands", connectionIDs, hands, activePlayerSeatNumber);
            //await _connection.SendAsync("GameServerSendHands", connectionIDs, hands[0], hands[1], hands[2], hands[3], hands[4], hands[5]);
        }

        public async Task GameServerSendSwapConfirmation(string connectionID, List<string> cards, List<string> newCards)
        {
            await _connection.SendAsync("GameServerSendSwapConfirmation", connectionID, cards, newCards);
        }

        public async Task GameServerSendBeginPlayMessage(int activePlayerSeatNumber)
        {
            await _connection.SendAsync("GameServerSendBeginPlayMessage", activePlayerSeatNumber);
        }

        public async Task GameServerSendPlayConfirmation(List<string> cards)
        {
            await _connection.SendAsync("GameServerSendPlayConfirmation", cards);
        }

        public async Task GameServerSendEndOfGameMessage(int winner, int loser, float balanceChange, List<List<string>> flippedCardLists)
        {
            await _connection.SendAsync("GameServerSendEndOfGameMessage", winner, loser, balanceChange, flippedCardLists);
        }

        public async Task GameServerSendInitialDeal(List<List<string>> dealtCards)
        {
            await _connection.SendAsync("GameServerSendInitialDeal", dealtCards);
        }

    }
}
