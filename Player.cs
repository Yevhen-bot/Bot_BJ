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
        private int balance;
        private List<Card> cards;
        public bool Stand { get; set; }
        public long Id { get { return _id; } }
        public string Name { get { return _name; } }

        public Player(long id, string name)
        {
            _id = id;
            _name = name;
            balance = 500;
            cards = new List<Card>();
            Stand = false;
        }

        public override string ToString()
        {
            return $"Player with id: {_id}, name: {_name}, balance {balance}";
        }

        public void TakeCard(Card card)
        {
            cards.Add(card);
        }

        public int ShowValue()
        {
            int s = 0;
            foreach (Card card in cards)
            {
                s += card.GetValue();
            }

            return s;
        }

        //public void MakeTurn()
        //{

        //}

        public string PrintCards()
        {
            string res = "";
            foreach (Card card in cards)
            {
                res += card.ToString();
                if (card != cards.Last())
                    res += ", ";
            }
            return res;
        }
    }
}
