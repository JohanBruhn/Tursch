using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tursch.Domain.Models
{
    
    
    public class Game
    {
        private List<Player> _players;
        private float _bet;
        private int _handSize;
        private List<Card> _deck;

        private Player _activePlayer;
        public Player ActivePlayer
        {
            get { return _activePlayer; }
            set { _activePlayer = value; }
        }

        private bool _swapRound;
        public bool SwapRound
        {
            get { return _swapRound; }
            set { _swapRound = value; }
        }

        private int _swapNumber;

        private Trick? _activeTrick;
        public Trick ActiveTrick
        {
            get { return _activeTrick; }
            set { _activeTrick = value; }
        }

        private bool _roundIsOver;
        public bool RoundIsOver
        {
            get { return _roundIsOver; }
            set { _roundIsOver = value; }
        }

        public Game(List<Player> players, float bet)
        {
            _deck = new List<Card>();
            _players = players;
            _bet = bet;
            _handSize = 5;
            _swapNumber = -1;
            _activePlayer = new Player();
            // Open lobby, host connects automatically, wait for more connections
        }

        

        // Generate deck, deal cards, find winner, then wait for server to send out deal info before drawing cards
        public int StartGame() // Add case for deck running out (add variable discard pile, shuffle and deal from there if needed?)
        {
            // Generate deck, shuffle
            _deck = GenerateDeck();
            // deal 4 down to all
            this.DealAll(_handSize - 1, false);
            // deal 1 up to all
            this.DealAll(1, true);
            // determine winner, display crown above highest card for 2 sec
            _activePlayer = this.FindHighestDeal(_players);

            return _activePlayer.SeatNumber;
        }

        // Add slask eventually (if highest value in hand is 10, display trash button for 2? 3? sec. If trashed, hand is shown to all players for a sec or two.

        // begin swap round, send prompt to player. int -1 = won the deal, can swap any number of cards.
        // Add 1-up swaps
        //_swapNumber = this.SwapRound(_activePlayer, -1);

        public void DrawDealtCards()
        {
            foreach (Player player in _players)
            {
                player.DrawDealtCards();
            }
            SwapRound = true;
        }

        public (bool, List<string>) RequestSwap(List<string> cards)
        {
            List<string> newCards = new();
            if (ActivePlayer.HasCards(cards))
            {
                if (_swapNumber == -1) { _swapNumber = cards.Count; }

                if (cards.Count == 0 || cards.Count == _swapNumber || cards.Count == ActivePlayer.GetHand().Count)
                {
                    
                    this.ActivePlayer.RemoveCardsToString(cards);
                    newCards = this.AddToPlayer(cards.Count, ActivePlayer);
                    ActivePlayer.HasActed = true;

                    // If next player has swapped then swap round is over, so set SwapRound to false and reset HasSwapped properties so that swaps will work next round
                    this.NextPlayer();
                    if (ActivePlayer.HasActed)
                    {
                        _swapRound = false;
                        foreach (Player player in _players)
                        {
                            player.HasActed = false;
                        }
                        this.ActiveTrick = new Trick(ActivePlayer);
                    }
                    return (true, newCards);
                }
            }
            return (false, newCards);
        }

        public (bool, bool, bool) RequestPlay(List<string> cards)
        {
            if (ActivePlayer.HasCards(cards))
            {
                bool validPlay = this.ActiveTrick.Play(this.ActivePlayer, cards);
                bool trickIsOver = false;
                bool gameIsOver = false;
                if (validPlay)
                {
                    ActivePlayer.HasActed = true;

                    // If next player has acted, current trick is done and next one is initiated by the winner of this one, unless there are no cards left to play
                    this.NextPlayer();
                    if (ActivePlayer.HasActed)
                    {
                        foreach (Player player in _players)
                        {
                            player.HasActed = false;
                        }
                        
                        // If current player has no cards, round is over and winner + loser are decided
                        if (ActivePlayer.GetHand().Count == 0)
                        {
                            gameIsOver = true;
                        }
                        else
                        {
                            trickIsOver = true;
                            this.ActivePlayer = this.ActiveTrick.CurrentLeader;
                            this.ActiveTrick = new Trick(this.ActivePlayer);
                        }
                    }
                }
                return (validPlay, trickIsOver, gameIsOver);
            }
            return (false, false, false);
        }

        public (Player winner, Player loser, float balanceChange, List<List<string>> flippedCardLists) EndOfRound()
        {
            List<Player> potentialWinners = new();
            List<Player> potentialLosers = new();
            int playValue = _activePlayer.GetLastPlay().Sum(card => card.GetValue());
            int runningLowestValue = playValue;
            int runningHighestValue = playValue;

            

            // Find potential winners and losers
            foreach (Player player in _players)
            {
                playValue = player.GetLastPlay().Sum(card => card.GetValue());

                // Potential winners
                if (playValue < runningLowestValue)
                {
                    potentialWinners.Clear();
                    potentialWinners.Add(player);
                    runningLowestValue = playValue;
                }
                else if (playValue == runningLowestValue)
                {
                    potentialWinners.Add(player);
                }

                // Potential losers
                if (playValue > runningHighestValue)
                {
                    potentialLosers.Clear();
                    potentialLosers.Add(player);
                    runningHighestValue = playValue;
                }
                else if (playValue == runningHighestValue)
                {
                    potentialLosers.Add(player);
                }
            }

            // Determine singular winner and loser (function works identically to function above except it loops until only 1 remains, playValue is defined by the flip
            // and compared against a new runningFlipValue to preserve running low and high for payout stage
            // Calculate everything first, then send end of game command with list of flipped cards for all players, winning and losing seat, and balance updates.
            // All dramatic animations for displaying winners and losers are displayed client-side
            // After this, server should wait for start game message from host (re-use same as for initial start?)
            List<List<string>> flippedCardLists = new();
            foreach (Player player in _players)
            {
                flippedCardLists.Add(new List<string>());
            }


            int runningFlipValue = 0;
            while (potentialWinners.Count > 1 && potentialWinners.Count < _players.Count)
            {
                List<Player> localPotentialWinners = new List<Player>(potentialWinners);
                // Flip cards
                foreach (Player player in potentialWinners)
                {
                    Card card = this.DealOne(player, true);
                    flippedCardLists[player.SeatNumber].Add(card.ToString());

                    playValue = card.GetValue();
                    if (playValue > runningFlipValue)
                    {
                        localPotentialWinners.Clear();
                        localPotentialWinners.Add(player);
                        runningFlipValue = playValue;
                    }
                    else if (playValue == runningFlipValue)
                    {
                        localPotentialWinners.Add(player);
                    }
                }
                potentialWinners = localPotentialWinners;
            }

            runningFlipValue = 15;
            while (potentialLosers.Count > 1 && potentialLosers.Count < _players.Count)
            {
                List<Player> localPotentialLosers = new List<Player>(potentialLosers);
                // Flip cards
                foreach (Player player in potentialLosers)
                {
                    Card card = this.DealOne(player, true);
                    flippedCardLists[player.SeatNumber].Add(card.ToString());

                    playValue = card.GetValue();
                    if (playValue < runningFlipValue)
                    {
                        localPotentialLosers.Clear();
                        localPotentialLosers.Add(player);
                        runningFlipValue = playValue;
                    }
                    else if (playValue == runningFlipValue)
                    {
                        localPotentialLosers.Add(player);
                    }
                }
                potentialLosers = localPotentialLosers;
            }

            // Pay out
            float balanceChange = this._bet * (runningHighestValue - runningLowestValue);
            potentialWinners.First().AddPlayBalance(_bet * (runningHighestValue - runningLowestValue));
            potentialLosers.First().AddPlayBalance(-1 * _bet * (runningHighestValue - runningLowestValue));


            // Ask players if they want to go next(?), then loop
            return (potentialWinners.First(), potentialLosers.First(), balanceChange, flippedCardLists);
        }


        // Generates ordered deck
        private static List<Card> GenerateDeck()
        {
            List<Card> deck = new();
            string suits = "HSCD";
            for (int i = 0; i < 4; i++) // for each suit
            {
                for (int j = 2; j < 15; j++)
                {
                    deck.Add(new Card(suits[i], j));
                }
            }
            return Shuffle(deck);
        }
        
        // Shuffles the deck
        private static List<Card> Shuffle(List<Card> deck)
        {
            Random rand = new Random();
            List<Card> newDeck = new();
            int cardIndex = 0;
            while(deck.Count > 0)
            {
                cardIndex = rand.Next(deck.Count);
                newDeck.Add(deck[cardIndex]);
                deck.Remove(deck[cardIndex]);
            }
            return newDeck;
        }

        // Deals specified number of cards to all players, hidden to all or visible to all
        private void DealAll(int cards, bool visible)
        {
            for (int i = 0; i<cards; i++)
            {
                foreach (Player player in _players)
                {
                    this.DealOne(player, visible);
                }
            }
        }

        // Deals specified number of cards to given player, hidden to all or visible to all
        private Card DealOne(Player player, bool visible)
        {
            Card card = _deck.First();
            Card returnCard = new Card(card);
            player.DealCard(card, visible);
            _deck.Remove(card);
            return returnCard;
        }

        private List<string> AddToPlayer(int cards, Player player)
        {
            List<string> newCards = new();
            for (int i = 0; i < cards; i++)
            {
                player.AddCard(_deck.First());
                newCards.Add(_deck.First().ToString());
                _deck.Remove(_deck.First());
            }
            return newCards;
        }

        private Player FindHighestDeal(List<Player> players) // Currently if two players are dealt the same hand the app will crash
        {
            List<Player> contenders = new();
            int currentHighestValue = 0;

            foreach (Player player in players)
            {
                Console.WriteLine("Player has " + player.GetDealtOpenCards().Last().GetValue()); //--------------------------------------------- Testing
                if (player.GetDealtOpenCards().Last().GetValue() > currentHighestValue)
                {
                    Console.WriteLine("Clearing other contenders"); //--------------------------------------------- Testing
                    currentHighestValue = player.GetDealtOpenCards().Last().GetValue();
                    contenders.Clear();
                    contenders.Add(player);
                }
                else if (player.GetDealtOpenCards().Last().GetValue() == currentHighestValue)
                {
                    Console.WriteLine("Added to contenders"); //--------------------------------------------- Testing
                    contenders.Add(player);
                }

            }
            if(contenders.Count > 1)
            {
                // flip next card
                Console.WriteLine("Tied, flipping next card"); //--------------------------------------------- Testing
                foreach (Player player in contenders)
                {
                    player.FlipNextCard();
                }
                return FindHighestDeal(contenders);
            }
            return contenders[0];

        }

        

        

        private void NextPlayer()
        {
            _activePlayer = _players[(_players.IndexOf(_activePlayer) + 1) % _players.Count()];
        }


    }
}
