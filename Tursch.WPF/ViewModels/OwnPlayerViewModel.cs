using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tursch.Domain.Models;

namespace Tursch.WPF.ViewModels
{
    internal class OwnPlayerViewModel : PlayerViewModelBase
    {
        

        public List<CardViewModel> SelectedCards { get; set; } // UNIQUE

        // Default constructor
        public OwnPlayerViewModel() : base()
        {
            SelectedCards = new List<CardViewModel>(); // UNIQUE
        }

        // Specific constructor
        public OwnPlayerViewModel(PlayerInfo playerInfo) : base(playerInfo)
        {
            SelectedCards = new List<CardViewModel>(); // UNIQUE
        }

        // Replace current hand with the specified hand
        // UNIQUE
        public void DrawHand(List<string> hand)
        {
            this.CardsOnTable.Clear();
            foreach (string card in hand)
            {
                this.CardsOnHand.Add(new CardViewModel(card));
            }
        }

        // Add specified cards to the hand
        // UNIQUE
        public void AddToHand(List<string> cards)
        {
            foreach (string card in cards)
            {
                this.CardsOnHand.Add(new CardViewModel(card));
            }
        }

        // Remove specified cards from the hand
        // UNIQUE
        public void RemoveFromHand(List<string> cards)
        {
            foreach (string card in cards)
            {
                this.CardsOnHand.Remove( this.CardsOnHand.ToList().Find(x => x.CardName.Equals(card)) );
            }
        }
    }
}
