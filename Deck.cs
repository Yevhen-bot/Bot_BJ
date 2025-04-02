using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack
{
    internal class Deck
    {
        private readonly Card[] _deck;
        // Pointer to the next card to deal in the deck
        private int cardPtr;

        public Deck() {
            cardPtr = 0;
            _deck = new Card[52];

            int counter = 0;

            for (int i = 0; i < (int)Suit.MAX_SUITS; i++)
            {
                for (int j = 0; j < (int)Rank.MAX_RANK; j++)
                {
                    _deck[counter] = new((Suit)i, (Rank)j);
                    counter++;
                }
            }

            Shuffle();
        }

        private void Shuffle()
        {
            Random rnd = new Random();
            rnd.Shuffle(_deck);
            cardPtr = 0;
        }

        public Card DealCard()
        {
            if (cardPtr > 51)
            {
                Shuffle();
            }
            return _deck[cardPtr++];
        }

        public override string ToString()
        {
            string res = "";
            for(int i = 0; i < _deck.Length; i++)
            {
                if (i % 13 == 0) res += "\n";
                res += _deck[i].ToString() + " ";
            }

            return res;
        }
    }
}
