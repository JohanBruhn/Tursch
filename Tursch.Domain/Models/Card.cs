using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tursch.Domain.Models
{
    public class Card
    {
        private char _suit;
        private int _value;
        private string _imagePath;

        public Card(char suit, int value)
        {
            _suit = char.ToUpper(suit);
            _value = value;
            _imagePath = "resources/cards/" + _suit + "/" + this.ToString() + ".png";
        }

        public Card(string cardString)
        {
            _suit = cardString.First();
            string valueString = cardString.Substring(1);
            switch (valueString)
            {
                case "A":
                    _value = 14;
                    break;
                case "K":
                    _value = 13;
                    break;
                case "Q":
                    _value = 12;
                    break;
                case "J":
                    _value = 11;
                    break;
                default:
                    _value = int.Parse(valueString);
                    break;
            }
            _imagePath = "";
        }

        public Card(Card card)
        {
            _suit = card.GetSuit();
            _value = card.GetValue();
            _imagePath = card.GetImagePath();
        }

        // Unknown card for players
        public Card()
        {
            _suit = ' ';
            _value = 0;
            _imagePath = "resources/cards/cardback.png";
        }

        public override string ToString()
        {
            switch (_value)
            {
                case 0:
                    return "";
                case 11:
                    return _suit + "J";
                case 12:
                    return _suit + "Q";
                case 13:
                    return _suit + "K";
                case 14:
                    return _suit + "A";
                default:
                    return _suit + _value.ToString();
            }
        }

        public override bool Equals(object? obj)
        {
            try
            {
                if (obj is Card card)
                {
                    if (this._suit == card.GetSuit() && this._value == card.GetValue())
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                return true;
            }

            return base.Equals(obj);
        }

        public char GetSuit()
        {
            return _suit;
        }

        public int GetValue()
        {
            return _value;
        }

        public string GetImagePath()
        {
            return _imagePath;
        }

        public static bool AreSame(List<Card> cards)
        {
            int runningValue = cards.First().GetValue();
            foreach (Card card in cards)
            {
                if (card.GetValue() != runningValue)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsLowerThan(List<Card> cards)
        {
            if(cards.Count == 0)
            {
                return true;
            }

            bool isLower = true;
            foreach (Card card in cards)
            {
                if (this._value > card.GetValue())
                {
                    isLower = false;
                }
            }

            return isLower;
        }

        public static List<Card> GetCardsExcept(List<Card> cards, List<Card> cardsToRemove)
        {
            List<Card> hand = new List<Card>(cards);
            foreach (Card handCard in cards)
            {
                foreach (Card card in cardsToRemove)
                {
                    if (handCard.Equals(card))
                    {
                        hand.Remove(handCard);
                    }
                }
            }
            return hand;
        }
    }
}
