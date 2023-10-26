using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tursch.WPF.ViewModels
{
    internal class CardViewModel : ViewModelBase
    {

        public string CardImagePath
        {
            get
            {
                if (_cardName.Equals(""))
                {
                    return "../resources/cards/cardback.png";
                }
                return "../resources/cards/" + _cardName.First() + "/" + _cardName + ".png";
            }

        }

        private string _cardName;
        public string CardName
        {
            get { return _cardName; }
            set
            {
                _cardName = value;
                OnPropertyChanged(nameof(CardName));
                OnPropertyChanged(nameof(CardImagePath));
            }
        }

        public CardViewModel()
        {
            _cardName = "";
        }
        public CardViewModel(string cardName)
        {
            _cardName = cardName;
        }
    }
}
