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
    internal class OtherPlayerViewModel : PlayerViewModelBase
    {
        // Coordinates for placement on the GameView
        private double _tablePileCanvasTop;
        public double TablePileCanvasTop
        {
            get { return _tablePileCanvasTop; }
            set
            {
                _tablePileCanvasTop = value;
                OnPropertyChanged(nameof(TablePileCanvasTop));
            }
        }

        private double _tablePileCanvasLeft;
        public double TablePileCanvasLeft
        {
            get { return _tablePileCanvasLeft; }
            set
            {
                _tablePileCanvasLeft = value;
                OnPropertyChanged(nameof(TablePileCanvasLeft));
            }
        }

        // Default constructor
        public OtherPlayerViewModel() : base() { }

        // Specific constructor
        public OtherPlayerViewModel(PlayerInfo playerInfo, int seatNumber) : base(playerInfo)
        {
            TablePileCanvasTop = 0d; // UNIQUE
            TablePileCanvasLeft = 0d; // UNIQUE

            // Sets coordinates depending on seatNumber
            // UNIQUE
            switch (seatNumber)
            {
                case 1:
                    TablePileCanvasTop = -60d;
                    TablePileCanvasLeft = 200d;
                    break;
                case 2:
                    TablePileCanvasTop = -30d;
                    TablePileCanvasLeft = 200d;
                    break;
                case 3:
                    TablePileCanvasTop = 70d;
                    TablePileCanvasLeft = -70d;
                    break;
                case 4:
                    TablePileCanvasTop = -30d;
                    TablePileCanvasLeft = -340d;
                    break;
                case 5:
                    TablePileCanvasTop = -60d;
                    TablePileCanvasLeft = -340d;
                    break;
            }
        }

        // Replace current hand with 5 unknown cards
        // TODO: To make all cards visible (for spectating) take List as parameter and add like in OwnPlayerViewModel
        public void DrawHand()
        {
            this.CardsOnTable.Clear();
            for(int i = 0; i < 5; i++)
            {
                CardsOnHand.Add(new CardViewModel());
            }
        }

        // Add nCards unknown cards to the hand
        // TODO: To make all cards visible, take List as parameter instead of int
        public void AddToHand(int nCards)
        {
            for (int i = 0; i < nCards; i++)
            {
                CardsOnHand.Add(new CardViewModel());
            }
        }

        // Remove nCards unknown cards from the hand
        // TODO: to make all cards visible, take list as parameter here too
        public void RemoveFromHand(int nCards)
        {
            for (int i = 0; i < nCards; i++)
            {
                CardsOnHand.RemoveAt(0);
            }
        }
    }
}
