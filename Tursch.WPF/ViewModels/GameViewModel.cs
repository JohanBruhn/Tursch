using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tursch.WPF.Services;
using Tursch.Domain.Models;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Tursch.WPF.Views;
using System.Windows;
using System.Windows.Input;
using Tursch.WPF.Commands;
using System.Text.Json;

namespace Tursch.WPF.ViewModels
{
    internal class GameViewModel : ViewModelBase
    {
        private SignalRClientService _clientService;
        public SignalRClientService ClientService
        {
            get { return _clientService; }
            set
            {
                _clientService = value;
                OnPropertyChanged(nameof(ClientService));
            }
        }

        private string _tableImagePath;
        public string TableImagePath
        {
            get { return _tableImagePath; }
            set
            {
                _tableImagePath = value;
                OnPropertyChanged(nameof(TableImagePath));
            }
        }

        

        private bool _isSwapRound;
        public bool IsSwapRound
        {
            get { return _isSwapRound; }
            set
            {
                _isSwapRound = value;
                OnPropertyChanged(nameof(IsSwapRound));
                OnPropertyChanged(nameof(ActionButtonText));
                OnPropertyChanged(nameof(ActionButtonVisibility));
            }
        }

        private int _activePlayer;
        public int ActivePlayer
        {
            get { return _activePlayer; }
            set
            {
                _activePlayer = value;
                UpdatePlayerViews(value);
                OnPropertyChanged(nameof(ActivePlayer));
                OnPropertyChanged(nameof(ActionButtonText));
                OnPropertyChanged(nameof(ActionButtonVisibility));
            }
        }

        private void UpdatePlayerViews(int value)
        {
            if (value == 0)
            {
                Seat0.IsActivePlayer = true;
                foreach (OtherPlayerViewModel player in OtherPlayers)
                {
                    player.IsActivePlayer = false;
                }
            }
            else
            {
                Seat0.IsActivePlayer = false;
                foreach (OtherPlayerViewModel player in OtherPlayers)
                {
                    player.IsActivePlayer = false;
                }
                OtherPlayers[value-1].IsActivePlayer = true;
            }
        }

        // Increments current active player
        private void NextPlayer()
        {
            ActivePlayer = (ActivePlayer + 1) % (OtherPlayers.Count + 1);
        }

        public string ActionButtonText
        {
            get
            {
                if (IsSwapRound)
                {
                    return "Swap";
                }
                return "Play";
            }
        }

        public Visibility ActionButtonVisibility
        {
            get
            {
                if ( IsSwapRound || ActivePlayer == 0)
                {
                    return Visibility.Visible;
                }
                return Visibility.Hidden;
            }
        }

        private Visibility _nextRoundButtonVisibility;
        public Visibility NextRoundButtonVisibility
        {
            get
            {
                if (_seatMap[0] == 0) // Ensures button is only displayed for host, but a bit ugly. Command should not be executable and server should not receive command if not from host
                {
                    return _nextRoundButtonVisibility;
                }
                else
                {
                    return Visibility.Hidden;
                }
            }
            set
            {
                _nextRoundButtonVisibility = value;
                OnPropertyChanged(nameof(NextRoundButtonVisibility));
            }
        }

        // Used internally to keep track of game information
        private List<OtherPlayerViewModel> _otherPlayers;
        public List<OtherPlayerViewModel> OtherPlayers
        {
            get { return _otherPlayers; }
            set
            {
                _otherPlayers = value;
                OnPropertyChanged(nameof(OtherPlayers));
            }
        }

        private string _myName;

        // Maps local seat number (ActivePlayer) to player number sent by server
        // _seatMap[0] returns my server-side player number
        // Index of active player in list OtherPlayers is ActivePlayer-1 (since myPlayer is not part of the list)
        // When server sends number n for new active player it should be set as ActivePlayer = _seatMap.IndexOf(n)
        private int[] _seatMap;

        public ICommand SendGameClientPerformActionCommand { get; }
        public ICommand SendGameClientStartGameCommand { get; }


        public GameViewModel(SignalRClientService clientService, List<PlayerInfo> playerInfoList, string myName, int dealWinner)
        {
            // Define grid in GameView, perhaps add UserControls PlayerView to the seat positions in grid, but with DataContext some PlayerViewModels defined in this class
            // Or have one ObservableCollection<PlayerViewModel> for each seat? and change the viewmodel in it when necessary?
            // If seat is not taken, PlayerViewModel is not made and there is no data for PlayerView to render
            ClientService = clientService;
            TableImagePath = "../resources/table.png"; // TODO: Should be changeable with red felt setting, so the string should be read from a settings file
            OtherPlayers = new List<OtherPlayerViewModel>();
            _myName = myName;
            NextRoundButtonVisibility = Visibility.Hidden;

            // PlayerInfo is purely a communication class, server uses it to send initial data for the playerviews.
            // It should probably be a struct

            PlayerInfo? myPlayer = playerInfoList.Find(playerInfo => playerInfo.PlayerName.Equals(myName, StringComparison.OrdinalIgnoreCase));
            _seatMap = new int[playerInfoList.Count];

            SendGameClientPerformActionCommand = new SendGameClientPerformActionCommand(this, ClientService);
            SendGameClientStartGameCommand = new SendGameClientStartGameCommand(this, ClientService);
            
            // Declares SeatN fields according to playerInfoList with myPlayer at Seat1, adds them to list OtherPlayers,
            // and maps objective seat to local seat (OtherPlayers index) in _seatMap
            this.AssignSeats(myPlayer, playerInfoList);
            ActivePlayer = Array.IndexOf(_seatMap, dealWinner);
            this.ShowDealWinner(true);


            IsSwapRound = true;

            // Listen for relevant server invocations
            ClientService.DealtHandReceived += ClientService_DealtHandReceived;
            ClientService.SwapConfirmationReceived += ClientService_SwapConfirmationReceived;
            ClientService.OtherPlayerSwapReceived += ClientService_OtherPlayerSwapReceived;
            ClientService.BeginPlayReceived += ClientService_BeginPlayReceived;
            ClientService.PlayConfirmationReceived += ClientService_PlayConfirmationReceived;
            ClientService.EndOfGameReceived += ClientService_EndOfGameReceived;
            ClientService.InitialDealReceived += ClientService_InitialDealReceived;
        }


        public override void Dispose()
        {
            ClientService.DealtHandReceived -= ClientService_DealtHandReceived;
            ClientService.SwapConfirmationReceived -= ClientService_SwapConfirmationReceived;
            ClientService.OtherPlayerSwapReceived -= ClientService_OtherPlayerSwapReceived;
            ClientService.BeginPlayReceived -= ClientService_BeginPlayReceived;
            ClientService.PlayConfirmationReceived -= ClientService_PlayConfirmationReceived;
            ClientService.EndOfGameReceived -= ClientService_EndOfGameReceived;
            ClientService.InitialDealReceived -= ClientService_InitialDealReceived;
            base.Dispose();
        }


        // EVENT HANDLERS -----------------------------------------


        // Distributes dealt cards to all players. Cards in this case are represented as strings in the format "SV",
        // where S is the first letter of the suit and V is the numerical value of the card, or its first letter in the case of jacks, queens, kings and aces.
        // An empty string represents an unknown card.
        private void ClientService_InitialDealReceived(List<List<string>> dealtCards, int dealWinner)
        {
            Console.WriteLine("Initial deal event triggered, adding " + dealtCards[_seatMap[0]].Count + " cards to seat 0");
            Seat0.CardsOnTable.Clear();
            Seat0.AddToTable(dealtCards[_seatMap[0]]);
            Seat0.IsWinner = false;

            for (int i = 0; i < OtherPlayers.Count; i++)
            {
                Console.WriteLine("Adding " + dealtCards[_seatMap[1+i]].Count + " cards to player " + (i+1));
                OtherPlayers[i].CardsOnTable.Clear();
                OtherPlayers[i].AddToTable(dealtCards[_seatMap[1 + i]]);
                OtherPlayers[i].IsWinner = false;
            }

            this.ShowDealWinner(true);
            IsSwapRound = true;
        }

        // Handles the end of a game round, displaying all cards as well as the winner, loser, and their respective earnings/losses
        private void ClientService_EndOfGameReceived(int winner, int loser, float balanceChange, List<List<string>> flippedCardLists)
        {
            if (flippedCardLists[_seatMap[0]].Count > 0) // Why is this check needed? TODO: Figure it out and comment
            {
                Seat0.AddToTable(flippedCardLists[_seatMap[0]]); // Display my cards
                    
            }
            for (int i = 0; i < OtherPlayers.Count; i++)
            {
               if(flippedCardLists[_seatMap[1+i]].Count > 0)
                {
                    OtherPlayers[i].AddToTable(flippedCardLists[_seatMap[1 + i]]); // Reveal other players' cards
                }
            }

            int localWinner = Array.IndexOf(_seatMap, winner);
            int localLoser = Array.IndexOf(_seatMap, loser);

            if (localWinner == 0)
            {
                Seat0.IsWinner = true;
                Seat0.Balance += balanceChange;
            }
            else
            {
                OtherPlayers[localWinner - 1].IsWinner = true;
                OtherPlayers[localWinner - 1].Balance += balanceChange;
            }

            if (localLoser == 0)
            {
                Seat0.Balance -= balanceChange;
            }
            else
            {
                OtherPlayers[localLoser - 1].Balance -= balanceChange;
            }
            NextRoundButtonVisibility = Visibility.Visible;
        }

        // Updates relevant player view when a play is confirmed by the server
        private void ClientService_PlayConfirmationReceived(List<string> cards)
        {
            Console.WriteLine("Client received play confirmation, ActivePlayer is: " + ActivePlayer);
            Console.WriteLine("_seatMap[0] is: " + _seatMap[0]);
            if (ActivePlayer == 0)
            {
                Console.WriteLine("Editing own player view");
                Seat0.RemoveFromHand(cards);
                Seat0.AddToTable(cards);
                this.NextPlayer();
            }
            else
            {
                Console.WriteLine("Editing player view for player with name: " + OtherPlayers[ActivePlayer-1].PlayerName);
                OtherPlayers[ActivePlayer - 1].RemoveFromHand(cards.Count);
                OtherPlayers[ActivePlayer - 1].AddToTable(cards);
                this.NextPlayer();
            }
        }

        // Begins play once server confirms that the swap round is over; clears cards from the table and sets the local active player to whomever won the deal, using seatmap. 
        private void ClientService_BeginPlayReceived(int activePlayerSeatNumber)
        {
            IsSwapRound = false;
            Seat0.CardsOnTable.Clear();
            foreach (OtherPlayerViewModel player in OtherPlayers)
            {
                player.CardsOnTable.Clear();
            }
            Console.WriteLine("Client received begin play message, active player server number is: " + activePlayerSeatNumber);
            ActivePlayer = Array.IndexOf(_seatMap, activePlayerSeatNumber);
            Console.WriteLine("After use of seatMap, ActivePlayer is: " + ActivePlayer);
        }

        // During swapround: add swappedNumber of face down cards on table at ActivePlayer, then update ActivePlayer
        private void ClientService_OtherPlayerSwapReceived(int swappedNumber) // Probably add parameter seatnumber (so client can verify that activeplayer is correct)
        {
            OtherPlayers[ActivePlayer - 1].AddToTable(swappedNumber);
            this.NextPlayer();
        }

        // During swapround: remove cards from hand, add newCards to hand. Add cards.Count of face down cards on table, then update ActivePlayer
        private void ClientService_SwapConfirmationReceived(List<string> cards, List<string> newCards)
        {
            Seat0.RemoveFromHand(cards);
            Seat0.AddToTable(cards.Count);
            Seat0.AddToHand(newCards);
            this.NextPlayer();
        }

        
        // During swapround: receives the deal from the server. Adds dealt cards to own hand and the same number of unknown cards to opposing players' hands. Sets active player to whomever won the deal.
        private void ClientService_DealtHandReceived(List<string> hand, int activePlayerSeatNumber)
        {
            this.ShowDealWinner(false);
            Seat0.DrawHand(hand);
            
            foreach (OtherPlayerViewModel player in OtherPlayers)
            {
                player.DrawHand();
            }
            ActivePlayer = Array.IndexOf(_seatMap, activePlayerSeatNumber);
        }


        // METHODS

        // Receives the player list with seat assignments from the server, generates the seatMap based on the local player's seat,
        // and displays opposing players at appropriate locations depending on their seat number and the total number of players, using the forbidden dark magic of hardcoding.
        // Meaning: locally, the player is always player 0, and it increments as you go clockwise around the table.
        // From the server's view (and as such in its communication), the host is player 0, thus a mapping is required.
        private void AssignSeats(PlayerInfo myPlayer, List<PlayerInfo> playerInfoList)
        {

            int mySeat = myPlayer.Seat;
            int nSeats = playerInfoList.Count;

            Seat0 = new OwnPlayerViewModel(myPlayer);

            // Map local index to server side seat number
            for(int i = 0; i < nSeats; i++)
            {
                _seatMap[i] = ((mySeat + i) % nSeats);
            }

            // May the lord have mercy upon my soul and forgive me for what follows

            switch (nSeats)
            {
                case 2:
                    Seat3 = new OtherPlayerViewModel(playerInfoList[_seatMap[1]], 3);
                    OtherPlayers.Add(Seat3);
                    break;

                case 3:
                    Seat2 = new OtherPlayerViewModel(playerInfoList[_seatMap[1]], 2);
                    OtherPlayers.Add(Seat2);
                    Seat4 = new OtherPlayerViewModel(playerInfoList[_seatMap[2]], 4);
                    OtherPlayers.Add(Seat4);
                    break;

                case 4:
                    Seat2 = new OtherPlayerViewModel(playerInfoList[_seatMap[1]], 2);
                    OtherPlayers.Add(Seat2);
                    Seat3 = new OtherPlayerViewModel(playerInfoList[_seatMap[2]], 3);
                    OtherPlayers.Add(Seat3);
                    Seat4 = new OtherPlayerViewModel(playerInfoList[_seatMap[3]], 4);
                    OtherPlayers.Add(Seat4);
                    break;

                case 5:
                    Seat1 = new OtherPlayerViewModel(playerInfoList[_seatMap[1]], 1);
                    OtherPlayers.Add(Seat1);
                    Seat2 = new OtherPlayerViewModel(playerInfoList[_seatMap[2]], 2);
                    OtherPlayers.Add(Seat2);
                    Seat4 = new OtherPlayerViewModel(playerInfoList[_seatMap[3]], 4);
                    OtherPlayers.Add(Seat4);
                    Seat5 = new OtherPlayerViewModel(playerInfoList[_seatMap[4]], 5);
                    OtherPlayers.Add(Seat5);
                    break;

                case 6:
                    Seat1 = new OtherPlayerViewModel(playerInfoList[_seatMap[1]], 1);
                    OtherPlayers.Add(Seat1);
                    Seat2 = new OtherPlayerViewModel(playerInfoList[_seatMap[2]], 2);
                    OtherPlayers.Add(Seat2);
                    Seat3 = new OtherPlayerViewModel(playerInfoList[_seatMap[3]], 3);
                    OtherPlayers.Add(Seat3);
                    Seat4 = new OtherPlayerViewModel(playerInfoList[_seatMap[4]], 4);
                    OtherPlayers.Add(Seat4);
                    Seat5 = new OtherPlayerViewModel(playerInfoList[_seatMap[5]], 5);
                    OtherPlayers.Add(Seat5);
                    break;
            }

            // Amen
        }

        // Displays card crown in deal winner's PlayerView
        private void ShowDealWinner(bool visible)
        {
            if (visible)
            {
                if (ActivePlayer == 0)
                {
                    Seat0.CardCrownVisibility = Visibility.Visible;
                }
                else
                {
                    OtherPlayers[ActivePlayer - 1].CardCrownVisibility = Visibility.Visible;
                }
            }
            else
            {
                Seat0.CardCrownVisibility = Visibility.Collapsed;
                foreach (OtherPlayerViewModel player in OtherPlayers)
                {
                    player.CardCrownVisibility = Visibility.Collapsed;
                }
            }
        }


        // Player displays, seat 0 is for the local player, then rest of playerlist clockwise
        private OwnPlayerViewModel _seat0;
        public OwnPlayerViewModel Seat0
        {
            get { return _seat0; }
            set
            {
                _seat0 = value;
                OnPropertyChanged(nameof(Seat0));
            }
        }

        private OtherPlayerViewModel _seat1;
        public OtherPlayerViewModel Seat1
        {
            get { return _seat1; }
            set
            {
                _seat1 = value;
                OnPropertyChanged(nameof(Seat1));
            }
        }

        private OtherPlayerViewModel _seat2;
        public OtherPlayerViewModel Seat2
        {
            get { return _seat2; }
            set
            {
                _seat2 = value;
                OnPropertyChanged(nameof(Seat2));
            }
        }

        private OtherPlayerViewModel _seat3;
        public OtherPlayerViewModel Seat3
        {
            get { return _seat3; }
            set
            {
                _seat3 = value;
                OnPropertyChanged(nameof(Seat3));
            }
        }

        private OtherPlayerViewModel _seat4;
        public OtherPlayerViewModel Seat4
        {
            get { return _seat4; }
            set
            {
                _seat4 = value;
                OnPropertyChanged(nameof(Seat4));
            }
        }

        private OtherPlayerViewModel _seat5;
        public OtherPlayerViewModel Seat5
        {
            get { return _seat5; }
            set
            {
                _seat5 = value;
                OnPropertyChanged(nameof(Seat5));
            }
        }
    }
}
