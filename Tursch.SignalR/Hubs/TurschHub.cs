using Microsoft.AspNetCore.SignalR;
using Tursch.Domain.Models;

namespace Tursch.SignalR.Hubs
{
    public class TurschHub : Hub
    {

        // Sets calling server class as the only connection in Group "Server", so that clients may message only the server
        // Note: Establish server before any client connections are made
        public async Task EstablishGameServer()
        {
            Console.WriteLine("Establishing GameServer with connectionID: " + Context.ConnectionId); // -------------------------------------------- Testing
            await Groups.AddToGroupAsync(Context.ConnectionId, "Server");
        }

        public async Task ServerSendStartedServerConfirmation()
        {
            Console.WriteLine("Sending started server confirmation");
            await Clients.Others.SendAsync("ClientReceiveStartedServerConfirmation");
        }


        // Begin Lobby commands


        // Client sends registry request to server, server saves ConnectionID (for group assignment), UserIdentifier (for private Server-Client communication),
        // and playerName for internal reference and for playerNameList generation
        public async Task ClientSendRegisterRequest(string playerName)
        {
            Console.WriteLine("Hub forwarding register request to server"); // -------------------------------------------- Testing
            await Clients.Group("Server").SendAsync("ServerReceiveRegisterRequest", Context.ConnectionId, Context.UserIdentifier, playerName);
            //await Clients.Others.SendAsync("ServerReceiveRegisterRequest", (Context.ConnectionId, Context.UserIdentifier, playerName));
            //await Clients.Group("Server").SendAsync("ServerReceiveRegisterRequest", (Context.ConnectionId, Context.UserIdentifier, playerName));
        }

        public async Task ClientSendLeaveLobbyRequest()
        {
            Console.WriteLine("Hub forwarding leave request");
            
            await Clients.Group("Server").SendAsync("ServerReceiveLeaveRequest", Context.ConnectionId);
        }

        public async Task ClientSendDisbandLobbyCommand()
        {
            Console.WriteLine("Hub forwarding disband lobby command to all clients");

            await Clients.GroupExcept("Clients", Context.ConnectionId).SendAsync("ClientReceiveDisbandLobby");
            await Clients.Caller.SendAsync("ClientReceiveDisbandLobby");
        }

        public async Task ClientSendStartGameCommand()
        {
            Console.WriteLine("Hub forwarding start game command to server");
            await Clients.Group("Server").SendAsync("ServerReceiveStartGameCommand");
        }


        // If client registry request was successful, assign client connection to Group "Clients" and send updated playerNameList to all clients
        public async Task ServerSendRegisterConfirmation(string clientConnectionID, List<string> playerNameList)
        {
            Console.WriteLine("Hub adding player to client group and forwarding playerNameList to clients"); // -------------------------------------------- Testing
            await Groups.AddToGroupAsync(clientConnectionID, "Clients");

            await Clients.Group("Clients").SendAsync("ClientReceivePlayerList", playerNameList);
        }

        public async Task ServerSendLeaveLobbyConfirmation(string clientConnectionID, List<string> playerNameList)
        {
            Console.WriteLine("Hub removing player from Clients list and forwarding updated playerNameList");
            await Groups.RemoveFromGroupAsync(clientConnectionID, "Clients");
            await Clients.Group("Clients").SendAsync("ClientReceivePlayerList", playerNameList);
        }


        public async Task ServerSendStartGameConfirmation(List<string> jsonPlayerInfo)
        {
            Console.WriteLine("Hub forwarding start game confirmation to clients, along with player info list (json strings)");
            await Clients.Group("Clients").SendAsync("ClientReceiveStartGame", jsonPlayerInfo);
        }

        

        // End lobby commands


        // Begin game commands

        //public async Task ClientSendGameStateRequest(){ }

        public async Task GameServerSendHands(List<string> connectionIDs, List<List<string>> hands, int activePlayerSeatNumber)
        {
            Console.WriteLine("Hub forwarding dealt hands to all players");
            for (int i = 0; i < connectionIDs.Count; i++)
            {
                await Clients.Client(connectionIDs[i]).SendAsync("GameClientReceiveHand", hands[i], activePlayerSeatNumber);
            }


        }

        public async Task GameClientSendPerformActionRequest(List<string> cards)
        {
            Console.WriteLine("Hub forwarding action request to server");
            await Clients.Group("Server").SendAsync("GameServerReceivePerformActionRequest", Context.ConnectionId, cards);
        }

        public async Task GameServerSendSwapConfirmation(string connectionID, List<string> cards, List<string> newCards)
        {
            Console.WriteLine("Hub forwarding swap confirmation to clients");
            await Clients.Client(connectionID).SendAsync("GameClientReceiveSwapConfirmation", cards, newCards);
            await Clients.GroupExcept("Clients", connectionID).SendAsync("GameClientReceiveOtherPlayerSwap", cards.Count);
        }

        public async Task GameServerSendBeginPlayMessage(int activePlayerSeatNumber)
        {
            Console.WriteLine("Hub forwarding round start message to clients");
            await Clients.Group("Clients").SendAsync("GameClientReceiveBeginPlayMessage", activePlayerSeatNumber);
        }

        public async Task GameServerSendPlayConfirmation(List<string> cards)
        {
            Console.WriteLine("Hub forwarding play confirmation to clients");
            await Clients.Group("Clients").SendAsync("GameClientReceivePlayConfirmation", cards);
        }

        public async Task GameServerSendEndOfGameMessage(int winner, int loser, float balanceChange, List<List<string>> flippedCardLists)
        {
            Console.WriteLine("Hub forwarding end of game message to all clients, with winner " + winner + " getting paid " + balanceChange + " dolars from " + loser);
            await Clients.Group("Clients").SendAsync("GameClientReceiveEndOfGame", winner, loser, balanceChange, flippedCardLists);
        }

        public async Task GameServerSendInitialDeal(List<List<string>> dealtCards)
        {
            Console.WriteLine("Hub forwarding initial deal to clients, along with dealt cards");
            await Clients.Group("Clients").SendAsync("GameClientReceiveInitialDeal", dealtCards);
        }
    }
}