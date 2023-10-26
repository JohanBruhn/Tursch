using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tursch.Domain.Models
{
    public class Player
    {
        private string _connectionID; // For server use
        private string _userID; // For server use

        private List<Card> _dealtDownCards;
        private List<Card> _dealtOpenCards;
        private List<Card> _cards;  
        private List<Card> _playedCards;
        private List<Card> _playedCardHistory;
        private string _name;
        private float _playBalance;

        // Used to keep track of when to end swap round, remember to reset to false when necessary
        private bool _hasActed;
        public bool HasActed
        {
            get { return _hasActed; }
            set { _hasActed = value; }
        }

        private int _seatNumber;
        public int SeatNumber
        {
            get { return _seatNumber; }
            set { _seatNumber = value; }
        }

        public Player(string connectionID, string userID, string name, int seatNumber)
        {
            _connectionID = connectionID;
            _userID = userID;
            _name = name;
            SeatNumber = seatNumber;
            _dealtDownCards = new List<Card>(); 
            _dealtOpenCards = new List<Card>();
            _cards = new List<Card>();
            _playedCards = new List<Card>();
            _playedCardHistory = new List<Card>();
            _playBalance = 0f;
        }

        public Player()
        {
            _name = "";
            _dealtDownCards = new List<Card>();
            _dealtOpenCards = new List<Card>();
            _cards = new List<Card>();
            _playedCards = new List<Card>();
            _playedCardHistory = new List<Card>();
            _playBalance = 0f;
            _connectionID = "";
            _userID = "";
            SeatNumber = -1;
        }

        public string GetName()
        {
            return _name;
        }

        public string GetConnectionID()
        {
            return _connectionID;
        }

        public string GetUserID()
        {
            return _userID;
        }

        public List<Card> GetHand()
        {
            return _cards;
        }

        public List<Card> GetDealtOpenCards()
        {
            return _dealtOpenCards;
        }

        public List<Card> GetLastPlay()
        {
            return _playedCards;
        }


        public List<string> GetHandToString()
        {
            List<string> cardStrings = new List<string>();
            foreach (Card card in _cards)
            {
                cardStrings.Add(card.ToString());
            }
            return cardStrings;
        }

        

        public List<string> GetDealToString()
        {
            List<string> result = new List<string>();
            foreach (Card card in _dealtDownCards)
            {
                // To make all cards visible: set this to "result.Add(card.ToString());"
                result.Add("");
            }
            foreach (Card card in _dealtOpenCards)
            {
                result.Add(card.ToString());
            }
            return result;
        }

        // Returns true if each card represented in list cards exists in player's hand
        public bool HasCards(List<string> cards)
        {
            List<string> hand = this.GetHandToString();
            bool flag = true;
            foreach (string card in cards)
            {
                if (!hand.Contains(card))
                {
                    flag = false;
                }
            }
            return flag;
        }

        public static PlayerInfo GetPlayerInfo(Player player)
        {
            PlayerInfo playerInfo = new PlayerInfo(player.GetName(), "../resources/avatars/defaultavatar.png", 0.0f, player.GetDealToString(), player.SeatNumber);
            return playerInfo;
        }

        public void AddPlayBalance(float value)
        {
            _playBalance += value;
        }

        public void DealCard(Card card, bool visible)
        {
            if (visible)
            {
                _dealtOpenCards.Add(card);
            }
            else
            {
                _dealtDownCards.Add(card);
            }
        }

        public void FlipNextCard()
        {
            _dealtOpenCards.Add(_dealtDownCards.Last());
            Console.WriteLine("Next card is " + _dealtDownCards.Last()); //--------------------------------------------- Testing
            _dealtDownCards.Remove(_dealtDownCards.Last());
        }

        public void DrawDealtCards()
        {
            foreach (Card card in _dealtDownCards)
            {
                _cards.Add(card);
            }
            _dealtDownCards.Clear();
            foreach (Card card in _dealtOpenCards)
            {
                _cards.Add(card);
            }
            _dealtOpenCards.Clear();
        }

        public void AddCard(Card card)
        {
            _cards.Add(card);
        }

        public void ConfirmPlay(List<Card> cards)
        {
            _playedCards = cards;
        }

        public void RemoveCardsToString(List<string> cardStrings)
        {
            List<Card> cards = new();
            foreach (string cardString in cardStrings)
            {
                cards.Add(new Card(cardString));
            }
            this.RemoveCards(cards);
        }

        public void RemoveCards(List<Card> cards)
        {
            List<Card> hand = new List<Card>(_cards);
            foreach (Card handCard in hand)
            {
                foreach (Card card in cards)
                {
                    if (handCard.Equals(card))
                    {
                        _cards.Remove(handCard);
                    }
                }
            }
        }
    }
}
