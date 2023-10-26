using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tursch.Domain.Models
{
    public class Trick
    {
        private Player _currentLeader;
        public Player CurrentLeader
        {
            get { return _currentLeader; }
            set { _currentLeader = value; }
        }

        //private Player _activePlayer;
        //public Player ActivePlayer
        //{
        //    get { return _activePlayer; }
        //    set { _activePlayer = value; }
        //}


        private int _runningHighestValue;
        public int RunningHighestValue
        {
            get { return _runningHighestValue; }
            set { _runningHighestValue = value; }
        }

        private int _cardAmount;
        private bool _validPlay;
        private bool _winnableFlag;

        public string errorMessage;

        public Trick(Player activePlayer)
        {
            CurrentLeader = activePlayer;
            //_activePlayer = activePlayer;
            _runningHighestValue = 0;
            _cardAmount = 0;
            _validPlay = false;
            _winnableFlag = false;
            errorMessage = "";
        }

        public bool Play(Player activePlayer, List<string> playedCardsToString)
        {
            List<Card> playedCards = new();
            foreach (string cardString in playedCardsToString)
            {
                playedCards.Add(new Card(cardString));
            }
            // Assume true
            _validPlay = true;
            // Will be set true if winning play
            _winnableFlag = false;

                
            // If active player has initiative, only check if all cards played have same value
            if (activePlayer.Equals(CurrentLeader))
            {
                if (Card.AreSame(playedCards))
                {
                    _runningHighestValue = playedCards.First().GetValue();
                    _cardAmount = playedCards.Count;
                    _winnableFlag = true;

                }
                else
                {
                    _validPlay = false;
                    errorMessage = "Cards aren't same";
                }

            }
            // Number of cards played must match number of cards played by previous player(s)
            else if (playedCards.Count != _cardAmount)
            {
                _validPlay = false;
                errorMessage = "Wrong amount of cards played";
            }
            // Only winning move is if all cards played are of the same value, higher than the best running value.
            else if (Card.AreSame(playedCards) && playedCards.First().GetValue() >= _runningHighestValue)
            {
                _winnableFlag = true;
            }
            else
            {
                foreach (Card card in playedCards)
                {
                    // If play is not winnable, each played card must be higher than the running value and/or the lowest card in hand to be valid
                    if (!(card.GetValue() >= _runningHighestValue || card.IsLowerThan(Card.GetCardsExcept(activePlayer.GetHand(), playedCards))))
                    {
                        _validPlay = false;
                        errorMessage = "First condition: " + (card.GetValue() >= _runningHighestValue) + " Second condition: " + (card.IsLowerThan(Card.GetCardsExcept(activePlayer.GetHand(), playedCards)));
                    }
                }
            }

            if (_winnableFlag)
            {
                CurrentLeader = activePlayer;
                _runningHighestValue = playedCards.First().GetValue();
            }

            if (_validPlay)
            {
                // Send confirmation of play validity
                activePlayer.ConfirmPlay(playedCards);
                activePlayer.RemoveCards(playedCards);
            }

            // Set next active player
            return _validPlay;


        }
    }
}
