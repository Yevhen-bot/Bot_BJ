using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackJack;

namespace Bot
{
    internal class Player
    {
        private readonly long _id;
        private readonly string _name;
        private int _balance;
        
        // Cards in player's hand
        private List<Card> _cards;
        public bool Stand { get; set; }
        public long Id { get { return _id; } }
        public string Name { get { return _name; } }

        public Player(long id, string name)
        {
            _id = id;
            _name = name;
            _balance = 500;
            _cards = new List<Card>();
            Stand = false;
        }

        public override string ToString()
        {
            return $"Player with id: {_id}, name: {_name}, balance {_balance}";
        }

        public void TakeCard(Card card)
        {
            _cards.Add(card);
        }

        public int ShowValue()
        {
            int s = 0;
            foreach (Card card in _cards)
            {
                s += card.GetValue();
            }

            return s;
        }

        #region Unused(money focus)
        //public void MakeTurn()
        //{

        //}
        #endregion

        public string PrintCards()
        {
            string res = "";
            foreach (Card card in _cards)
            {
                res += card.ToString();
                if (card != _cards.Last())
                    res += ", ";
            }
            return res;
        }
    }
}
