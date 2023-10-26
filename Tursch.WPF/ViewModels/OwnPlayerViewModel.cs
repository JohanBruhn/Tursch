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
    internal class OwnPlayerViewModel : ViewModelBase
    {
        private string _backgroundImagePath;
        public string BackgroundImagePath
        {
            get { return _backgroundImagePath; }
            set
            {
                _backgroundImagePath = value;
                OnPropertyChanged(nameof(BackgroundImagePath));
            }
        }

        private string _avatarImagePath;
        public string AvatarImagePath
        {
            get { return _avatarImagePath; }
            set {
                _avatarImagePath = value;
                OnPropertyChanged(nameof(AvatarImagePath));
            }
        }

        private string _playerName;
        public string PlayerName
        {
            get { return _playerName; }
            set
            {
                _playerName = value;
                OnPropertyChanged(nameof(PlayerName));
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

        public List<CardViewModel> SelectedCards { get; set; }

        public OwnPlayerViewModel()
        {
            BackgroundImagePath = "../resources/playerframe.png";
            AvatarImagePath = "../resources/defaultavatar.png";
            PlayerName = "DefaultName";
            Balance = 0F;
            CardsOnHand = new ObservableCollection<CardViewModel>();
            CardsOnTable = new ObservableCollection<CardViewModel>();
            SelectedCards = new List<CardViewModel>();
        }

        public OwnPlayerViewModel(PlayerInfo playerInfo)
        {
            BackgroundImagePath = "../resources/playerframe.png";
            AvatarImagePath = playerInfo.AvatarPath;
            PlayerName = playerInfo.PlayerName;
            Balance = playerInfo.Balance;
            CardsOnHand = new ObservableCollection<CardViewModel>();
            CardsOnTable = new ObservableCollection<CardViewModel>();
            SelectedCards = new List<CardViewModel>();
            
            foreach (string card in playerInfo.CardsOnTable)
            {
                this.CardsOnTable.Add(new CardViewModel(card));
            }
        }

        public void DrawHand(List<string> hand)
        {
            this.CardsOnTable.Clear();
            foreach (string card in hand)
            {
                this.CardsOnHand.Add(new CardViewModel(card));
            }
        }

        public void AddToHand(List<string> cards)
        {
            foreach (string card in cards)
            {
                this.CardsOnHand.Add(new CardViewModel(card));
            }
        }

        public void RemoveFromHand(List<string> cards)
        {
            foreach (string card in cards)
            {
                this.CardsOnHand.Remove( this.CardsOnHand.ToList().Find(x => x.CardName.Equals(card)) );
            }
        }

        // Add face up cards to the table
        public void AddToTable(List<string> cards)
        {
            foreach (string card in cards)
            {
                this.CardsOnTable.Add(new CardViewModel(card));
            }
        }
        // Add face down cards to the table
        public void AddToTable(int nCards)
        {
            for (int i = 0; i < nCards; i++)
            {
                this.CardsOnTable.Add(new CardViewModel());
            }
        }
    }
}
