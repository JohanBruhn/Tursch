using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tursch.WPF.ViewModels;
using Tursch.Domain.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Tursch.Server.Services;
using Tursch.Server.Commands;
using System.Threading;

namespace Tursch.Server.ViewModels
{
    internal class ServerViewModel : ViewModelBase
    {
        private SignalRServerService _serverService;

        private Player _host;
        public Player Host
        {
            get { return _host; }
            set
            {
                _host = value;
                OnPropertyChanged(nameof(Host));
            }
        }

        private List<Player> _playerList;
        public List<Player> PlayerList
        {
            get { return _playerList; }
            set
            {
                _playerList = value;
                OnPropertyChanged(nameof(PlayerList));
            }
        }

        private bool _gameStarted;
        public bool GameStarted
        {
            get { return _gameStarted; }
            set
            {
                _gameStarted = value;
                OnPropertyChanged(nameof(GameStarted));
            }
        }

        private Game? _activeGame;
        public Game ActiveGame
        {
            get { return _activeGame; }
            set
            {
                _activeGame = value;
                OnPropertyChanged(nameof(ActiveGame));
            }
        }

        // Used for lobby updates
        public List<string> GetPlayerNameList()
        {
            List<string> collection = new();
            foreach (Player player in _playerList)
            {
                collection.Add(player.GetName());
            }

            return collection;
        }

        // Used at start of game to send player information to clients
        public List<PlayerInfo> GetPlayerInfoList()
        {
            List<PlayerInfo> playerInfo = new List<PlayerInfo>();
            PlayerInfo currentPlayerInfo;
            int i = 0;
            Messages.Add("Generating player info list");
            foreach (Player player in _playerList)
            {
                Messages.Add("Adding info for player: " + player.GetName());
                player.SeatNumber = i;
                currentPlayerInfo = Player.GetPlayerInfo(player);
                playerInfo.Add(currentPlayerInfo);
                
                i++;
            }
            Messages.Add("Returning player info list of length: " + playerInfo.Count);
            return playerInfo;
        }

        // Used for the serverview
        public ObservableCollection<string> Messages { get; }

        // Lobby commands
        public ICommand SendServerLobbyConfirmRegisterCommand { get; }
        public ICommand SendServerStartedServerConfirmationCommand { get; }
        public ICommand SendServerLobbyConfirmLeaveCommand { get; }
        public ICommand SendServerLobbyConfirmStartGameCommand { get; }
        // Game commands
        public ICommand SendGameServerDealHandsCommand { get; }
        public ICommand SendGameServerConfirmSwapCommand { get; }
        public ICommand SendGameServerConfirmPlayCommand { get; }
        public ICommand SendGameServerBeginPlayCommand { get; }
        public ICommand SendGameServerEndOfGameCommand { get; }

        public ICommand SendGameServerInitialDealCommand { get; }

        // Constructor
        public ServerViewModel(SignalRServerService serverService)
        {
            _serverService = serverService;
            SendServerLobbyConfirmRegisterCommand = new SendServerLobbyConfirmRegisterCommand(this, serverService);
            SendServerStartedServerConfirmationCommand = new SendServerStartedServerConfirmationCommand(this, serverService);
            SendServerLobbyConfirmLeaveCommand = new SendServerLobbyConfirmLeaveCommand(this, serverService);
            SendServerLobbyConfirmStartGameCommand = new SendServerLobbyConfirmStartGameCommand(this, serverService);
            SendGameServerDealHandsCommand = new SendGameServerDealHandsCommand(this, serverService);
            SendGameServerConfirmSwapCommand = new SendGameServerConfirmSwapCommand(this, serverService);
            SendGameServerConfirmPlayCommand = new SendGameServerConfirmPlayCommand(this, serverService);
            SendGameServerBeginPlayCommand = new SendGameServerBeginPlayCommand(this, serverService);
            SendGameServerEndOfGameCommand = new SendGameServerEndOfGameCommand(this, serverService);
            SendGameServerInitialDealCommand = new SendGameServerInitialDealCommand(this, serverService);

            Host = new Player();
            PlayerList = new List<Player>();
            Messages = new ObservableCollection<string>();
            GameStarted = false;

            _serverService.RegisterRequestReceived += ServerService_RegisterRequestReceived;
            _serverService.LeaveRequestReceived += ServerService_LeaveRequestReceived;
            _serverService.StartGameReceived += _serverService_StartGameReceived;
            _serverService.PerformActionRequestReceived += _serverService_PerformActionRequestReceived;
        }

        

        public override void Dispose()
        {
            _serverService.RegisterRequestReceived -= ServerService_RegisterRequestReceived;
            _serverService.LeaveRequestReceived -= ServerService_LeaveRequestReceived;
            _serverService.StartGameReceived -= _serverService_StartGameReceived;
            _serverService.PerformActionRequestReceived -= _serverService_PerformActionRequestReceived;

            base.Dispose();
        }

        private void _serverService_PerformActionRequestReceived(string connectionID, List<string> cards)
        {
            Messages.Add("Action request received");
            if (ActiveGame.ActivePlayer.GetConnectionID().Equals(connectionID))
            {
                Messages.Add("Player connection ID verified");
                if (ActiveGame.SwapRound)
                {
                    Messages.Add("Swap round detected");
                    (bool result, List<string> newCards) = ActiveGame.RequestSwap(cards);
                    Messages.Add("Handsize after swap = " + ActiveGame.ActivePlayer.GetHand().Count);

                    if (result)
                    {
                        // Send command with cards to remove from hand and cards to add to hand
                        SendGameServerConfirmSwapCommand.Execute((connectionID, cards, newCards));
                        Messages.Add("Next up is player at seat: " + ActiveGame.ActivePlayer.SeatNumber);
                        if (!ActiveGame.SwapRound)
                        {
                            Messages.Add("No more swaps to do, beginning play");
                            // Send begin play command (clients clear CardsOnTable piles and set swapRound to false locally)
                            SendGameServerBeginPlayCommand.Execute(ActiveGame.ActivePlayer.SeatNumber);
                        }
                    }
                }
                else
                {
                    Messages.Add("Requesting play");
                    (bool result, bool trickIsOver, bool gameIsOver) = ActiveGame.RequestPlay(cards);
                    Messages.Add(gameIsOver.ToString());

                    if (result)
                    {
                        Messages.Add("Play legal, new leader is: " + ActiveGame.ActiveTrick.CurrentLeader.GetName() + " with play value: " + ActiveGame.ActiveTrick.RunningHighestValue);
                        // Send command with cards to play from hand to table (to all clients)
                        Messages.Add("Number of cards on hand: " + ActiveGame.ActivePlayer.GetHand().Count);
                        SendGameServerConfirmPlayCommand.Execute(cards);
                    }
                    if (gameIsOver)
                    {
                        Messages.Add("Game is completed");
                        (Player winner, Player loser, float balanceChange, List<List<string>> flippedCardLists) = ActiveGame.EndOfRound();
                        Messages.Add("Game winner is player " + winner.SeatNumber + ", loser is " + loser.SeatNumber);
                        SendGameServerEndOfGameCommand.Execute((winner.SeatNumber, loser.SeatNumber, balanceChange, flippedCardLists));
                    }
                    else if (trickIsOver)
                    {
                        Thread.Sleep(2000);
                        // Send begin play command (to clear CardsOnTable piles and set active player)
                        Messages.Add("Trick over, winner of trick is: " + ActiveGame.ActivePlayer.GetName());
                        SendGameServerBeginPlayCommand.Execute(ActiveGame.ActivePlayer.SeatNumber);
                    }
                }
            }
        }

        private void _serverService_StartGameReceived()
        {
            
            // TODO: Implement different bet sizes
            ActiveGame = new Game(PlayerList, 1);
            int dealWinner = ActiveGame.StartGame();

            if (!GameStarted)
            {
                GameStarted = true;
                // Send back initial gamestate (generate playerinfo list (with initial cards, only cards dealt up/flipped are sent as known))
                SendServerLobbyConfirmStartGameCommand.Execute(dealWinner);
            }
            else
            {
                SendGameServerInitialDealCommand.Execute(dealWinner);
            }
            

            ActiveGame.DrawDealtCards();
            Messages.Add("Dealt cards drawn server side, waiting 2 sec then sending deal hands command");
            Thread.Sleep(2000); // Let deal be displayed
            // Sends each player their 5 cards on hand, when clients receive this command they also set all other player's hands to 5 unknown cards
            // Also sends active player seat to all players
            Messages.Add("Sleep done, sending command");
            SendGameServerDealHandsCommand.Execute(dealWinner); // Redundant  
        }

        



        private void ServerService_RegisterRequestReceived(string connectionID, string userID, string playerName)
        {
            Messages.Add("Server received register request from player: " + playerName);
            if (SendServerLobbyConfirmRegisterCommand.CanExecute(null)) // Check for player name availability too, can probably do bool check locally instead of calling CanExecute()
                // Might as well check so connection is unique as well
            {
                PlayerList.Add(new Player(connectionID, userID, playerName, PlayerList.Count));
                if (PlayerList.Count() == 1)
                {
                    Host = PlayerList.First();
                }
                Messages.Add("Server sending register confirmation");
                SendServerLobbyConfirmRegisterCommand.Execute(connectionID);
            }
            else
            {
                // Add new command: Send back registration denied with message: reason
            }
            
        }

        private void ServerService_LeaveRequestReceived(string connectionID)
        {
            try
            {
                PlayerList.Remove(PlayerList.Find(x => x.GetConnectionID().Equals(connectionID)));
                SendServerLobbyConfirmLeaveCommand.Execute(connectionID);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to remove player");
            }
            
        }

        public static ServerViewModel CreateConnectedServerViewModel(SignalRServerService serverService)
        {
            ServerViewModel viewModel = new ServerViewModel(serverService);

            serverService.Connect().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    Console.WriteLine("Unable to connect server to hub");
                    Console.WriteLine(task.Exception); // ------------------------------- Testing
                    // viewModel.ErrorMessage = "Unable to connect"
                }
            });

            serverService.EstablishGameServer().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    Console.WriteLine("Unable to establish game server at hub");
                    Console.WriteLine(task.Exception); // ------------------------------- Testing
                    // viewModel.ErrorMessage = "Unable to connect"
                }
            });

            viewModel.Messages.Add("Sending server start confirmation");
            viewModel.SendServerStartedServerConfirmationCommand.Execute(null);
            return viewModel;
        }

        

        
    }
}
