using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tursch.Domain.Models
{
    public class PlayerInfo
    {
        private string _playerName;
        private string _avatarPath;
        private float _balance;
        private int _seat; // Host has seat 0, next person to join has 1 etc.
        private List<string> _cardsOnTable;

        public PlayerInfo(string playerName, string avatarPath, float balance, List<string> cardsOnTable, int seat)
        {
            _playerName = playerName;
            _avatarPath = avatarPath;
            _balance = balance;
            _cardsOnTable = cardsOnTable;
            _seat = seat;
        }   

        public PlayerInfo(string playerName, string avatarPath, float balance, List<string> cardsOnTable)
        {
            _playerName = playerName;
            _avatarPath = avatarPath;
            _balance = balance;
            _seat = 0;
            _cardsOnTable = cardsOnTable;
        }

        public PlayerInfo()
        {
            _playerName = "default";
            _avatarPath = "";
            _balance = 0F;
            _seat = 0;
            _cardsOnTable = new List<string>();
        }

        public string PlayerName { get { return _playerName; } set { _playerName = value; } }
        public string AvatarPath { get { return _avatarPath; } set { _avatarPath = value; } }
        public float Balance { get { return _balance; } set { _balance = value; } }
        public int Seat { get { return _seat; } set { _seat = value; } }
        public List<string> CardsOnTable { get { return _cardsOnTable; } set { _cardsOnTable = value; } }

        public void AdjustBalance(float amount)
        {
            _balance += amount;
        }

    }
}
