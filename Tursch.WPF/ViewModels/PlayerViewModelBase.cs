using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tursch.Domain.Models;
using System.Windows;

namespace Tursch.WPF.ViewModels
{
    internal class PlayerViewModelBase : ViewModelBase
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
            set
            {
                _avatarImagePath = value;
                OnPropertyChanged(AvatarImagePath);
            }
        }

        private bool _isActivePlayer;
        public bool IsActivePlayer
        {
            get { return _isActivePlayer; }
            set
            {
                _isActivePlayer = value;
                OnPropertyChanged(nameof(IsActivePlayer));
                OnPropertyChanged(nameof(ActiveIndicatorVisibility));
            }
        }

        public Visibility ActiveIndicatorVisibility
        {
            // get { return IsActivePlayer ? Visibility.Visible : Visibility.Hidden; }
            get
            {
                if (IsActivePlayer) return Visibility.Visible;
                return Visibility.Hidden;
            }
        }

        private string _activeIndicatorImagePath;
        public string ActiveIndicatorImagePath
        {
            get { return _activeIndicatorImagePath; }
            set
            {
                _activeIndicatorImagePath = value;
                OnPropertyChanged(ActiveIndicatorImagePath);
            }
        }

        private bool _isWinner;
        public bool IsWinner
        {
            get { return _isWinner; }
            set { 
                _isWinner = value; 
                IsActivePlayer = false; // Ensures active player indicator doesn't overlap with crown
                OnPropertyChanged(nameof(IsWinner));
                OnPropertyChanged(nameof(CrownVisibility));
            }
        }

        public Visibility CrownVisibility
        {
            get
            {
                if (IsWinner) return Visibility.Visible;
                return Visibility.Hidden;
            }
        }

        private Visibility _cardCrownVisibility;
        public Visibility CardCrownVisibility
        {
            get { return _cardCrownVisibility; }
            set
            {
                _cardCrownVisibility = value;
                OnPropertyChanged(nameof(CardCrownVisibility));
            }
        }

        private string _crownImagePath;
        public string CrownImagePath
        {
            get { return _crownImagePath; }
            set
            {
                _crownImagePath = value;
                OnPropertyChanged(CrownImagePath);
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
        public float Balance
        {
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

        public PlayerViewModelBase()
        {
            BackgroundImagePath = "../resources/playerframe.png";
            AvatarImagePath = "../resources/defaultavatar.png";
            ActiveIndicatorImagePath = "../resources/activeindicator.png";
            CrownImagePath = "../resources/crown.png";
            CardCrownVisibility = Visibility.Hidden;
            PlayerName = "DefaultName";
            Balance = 0F;
            CardsOnHand = new ObservableCollection<CardViewModel>();
            CardsOnTable = new ObservableCollection<CardViewModel>();
            IsActivePlayer = false;
            IsWinner = false;
        }

        public PlayerViewModelBase(PlayerInfo playerInfo)
        {
            BackgroundImagePath = "../resources/playerframe.png";
            AvatarImagePath = playerInfo.AvatarPath;
            ActiveIndicatorImagePath = "../resources/activeindicator.png";
            CrownImagePath = "../resources/crown.png";
            CardCrownVisibility = Visibility.Hidden;
            PlayerName = playerInfo.PlayerName;
            Balance = playerInfo.Balance;
            CardsOnHand = new ObservableCollection<CardViewModel>();
            CardsOnTable = new ObservableCollection<CardViewModel>();
            IsActivePlayer = false;
            IsWinner = false;

            // Displays cards on table
            foreach (string card in playerInfo.CardsOnTable)
            {
                this.CardsOnTable.Add(new CardViewModel(card));
            }
        }

        // Add specified face up cards to the table
        public void AddToTable(List<string> cards)
        {
            foreach (string card in cards)
            {
                this.CardsOnTable.Add(new CardViewModel(card));
            }
        }
        // Add nCards face down cards to the table
        public void AddToTable(int nCards)
        {
            for (int i = 0; i < nCards; i++)
            {
                this.CardsOnTable.Add(new CardViewModel());
            }
        }

    }
}
