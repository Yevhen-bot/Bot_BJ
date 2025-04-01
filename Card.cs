using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace BlackJack
{

    enum Suit
    {
        Club,
        Diamond,
        Heart,
        Spade,
        MAX_SUITS
    }

    enum Rank
    {
        R2, R3, R4, R5, R6, R7, R8, R9, R10, RJack, RQueen, RKing, RAce, MAX_RANK
    }

    internal class Card
    {
        private readonly Suit _suit;
        private readonly Rank _rank;
        public Card(Suit suit, Rank rank)
        {
            _suit = suit;
            _rank = rank;
        }
        public override string ToString()
        {
            string res = "";

            switch(_rank)
            {
                case Rank.R2: res += "2"; break;
                case Rank.R3: res += "3"; break;
                case Rank.R4: res += "4"; break;
                case Rank.R5: res += "5"; break;
                case Rank.R6: res += "6"; break;
                case Rank.R7: res += "7"; break;
                case Rank.R8: res += "8"; break;
                case Rank.R9: res += "9"; break;
                case Rank.R10: res += "10"; break;
                case Rank.RJack: res += "J"; break;
                case Rank.RQueen: res += "Q"; break;
                case Rank.RKing: res += "K"; break;
                case Rank.RAce: res += "A";break;
            }

            switch(_suit)
            {
                case Suit.Spade: res += "S"; break;
                case Suit.Diamond: res += "D"; break;
                case Suit.Club: res += "C"; break; 
                case Suit.Heart: res += "H"; break;
            }

            return res;
        }

        public int GetValue()
        {
            switch (_rank)
            {
                case Rank.R2: return 2;
                case Rank.R3: return 3;
                case Rank.R4: return 4;
                case Rank.R5: return 5;
                case Rank.R6: return 6;
                case Rank.R7: return 7;
                case Rank.R8: return 8;
                case Rank.R9: return 9;
                case Rank.R10: return 10;
                case Rank.RJack: return 10;
                case Rank.RQueen: return 10;
                case Rank.RKing: return 10;
                case Rank.RAce: return 11;
            }

            return -1;
        }

        public int Ace()
        {
            return 0;
        }
    }



}
