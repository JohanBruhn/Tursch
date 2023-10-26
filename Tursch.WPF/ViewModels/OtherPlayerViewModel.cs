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
    internal class OtherPlayerViewModel : ViewModelBase
    {
        private string _backgroundImagePath;
        public string BackgroundImagePath
        {
            get { return _backgroundImagePath; }
            set
            {
                _backgroundImagePath = value;
                OnPropertyChanged(BackgroundImagePath);
            }
        }

        private string _avatarImagePath;
        public string AvatarImagePath
        {
            get { return _avatarImagePath; }
            set {
                _avatarImagePath = value;
                OnPropertyChanged(AvatarImagePath);
            }
        }

        private string _playerName;
        public string PlayerName
        {
            get { return _playerName; }
            set
            {
                _playerName = value;
                OnPropertyChanged(PlayerName);
            }
        }

        private float _balance;
        public float Balance {
            get { return _balance; } 
            set 
            { 
                _balance = value;
                OnPropertyChanged(nameof(Balance));
                OnPropertyChanged(nameof(BalanceString));
                OnPropertyChanged(nameof(BalanceColor));
            } 
        }

        public string BalanceString
        {
            get
            {
                return Balance.ToString("C2", CultureInfo.CreateSpecificCulture("en-US"));
            }
        }

        public string BalanceColor
        {
            get
            {
                if (Balance >= 0)
                {
                    return "Green";
                }
                return "Red";
            }
        }

        public ObservableCollection<CardViewModel> CardsOnHand { get; }
        public ObservableCollection<CardViewModel> CardsOnTable { get; }

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


        public OtherPlayerViewModel()
        {
            BackgroundImagePath = "../resources/playerframe.png";
            AvatarImagePath = "../resources/defaultavatar.png";
            PlayerName = "DefaultName";
            Balance = 0F;
            CardsOnHand = new ObservableCollection<CardViewModel>();
            CardsOnTable = new ObservableCollection<CardViewModel>();
            
        }

        public OtherPlayerViewModel(PlayerInfo playerInfo, int seatNumber)
        {
            BackgroundImagePath = "../resources/playerframe.png";
            AvatarImagePath = playerInfo.AvatarPath;
            PlayerName = playerInfo.PlayerName;
            Balance = playerInfo.Balance;
            CardsOnHand = new ObservableCollection<CardViewModel>();
            CardsOnTable = new ObservableCollection<CardViewModel>();
            TablePileCanvasTop = 0d;
            TablePileCanvasLeft = 0d;




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

            foreach (string card in playerInfo.CardsOnTable)
            {
                this.CardsOnTable.Add(new CardViewModel(card));
            }
        }

        public void DrawHand()
        {
            this.CardsOnTable.Clear();
            for(int i = 0; i < 5; i++)
            {
                CardsOnHand.Add(new CardViewModel());
            }
        }

        // To make all cards visible: Take List as parameter instead of int and add like in OwnPlayerViewModel
        public void AddToHand(int nCards)
        {
            for (int i = 0; i < nCards; i++)
            {
                CardsOnHand.Add(new CardViewModel());
            }
        }

        // To make all cards visible: Take list as parameter here too
        public void RemoveFromHand(int nCards)
        {
            for (int i = 0; i < nCards; i++)
            {
                CardsOnHand.RemoveAt(0);
            }
        }

        // Add face up cards to table
        public void AddToTable(List<string> cards)
        {
            foreach (string card in cards)
            {
                CardsOnTable.Add(new CardViewModel(card));
            }
        }
        // Add face down cards to table
        public void AddToTable(int nCards)
        {
            for (int i = 0; i < nCards; i++)
            {
                CardsOnTable.Add(new CardViewModel());
            }
        }
    }
}
