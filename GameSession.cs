using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackJack;
using Microsoft.AspNetCore.Routing;

namespace Bot
{
    class GameSession
    {
        public List<Player> players { get; private set; }
        public Player dealer { get; private set; }
        private Deck deck;

        public int Players {  get { return players.Count; } }

        public bool ToEnd()
        {
            return players.All((x) => x.Stand);
        }

        public Player GetPlayer(long id)
        {
            Player r = null;
            foreach(var p in players)
            {
                if(id == p.Id)
                {
                    r = p;
                    break;
                }
            }

            return r;
        }

        public GameSession()
        {
            players = new();
            dealer = new(-1, "dealer");
            dealer.Stand = true;
            deck = new Deck();
        }

        public void AddPlayer(Player player)
        {
            players.Add(player);
        }

        public Card Start()
        {
            for(int i = 0; i < 2; i++)
            {
                foreach (Player player in players)
                {
                    player.TakeCard(deck.DealCard());
                }
            }

            var c = deck.DealCard();
            dealer.TakeCard(c);
            return c;
        }

        //public bool Turn()
        //{
        //    bool toplay = false;

        //    foreach(Player player in players)
        //    {
        //        if(!player.Stand) {
        //            toplay = true;
        //            player.MakeTurn();
        //        }
        //    }

        //    return toplay;
        //}

        public List<Card> DealerLogic()
        {
            List<Card> list = new List<Card>();
            while (dealer.ShowValue() < 17)
            {
                var c = deck.DealCard();
                dealer.TakeCard(c);
                list.Add(c);
            }

            return list;
        }

        public Player End()
        {
            players.Sort((p1, p2) => p2.ShowValue().CompareTo(p1.ShowValue()));
            int best = 0;
            Player b = null;
            for(int i = 0;i < players.Count;i++)
            {
                if(players[i].ShowValue() <= 21 && players[i].ShowValue() > best)
                {
                    b = players[i];
                    best = b.ShowValue();
                    break;
                }
            }

            if (best == 0 || (best < dealer.ShowValue() && dealer.ShowValue() <= 21))
                b = dealer;

            return b;
        }

        public void HitPlayer(Player p)
        {
            p.TakeCard(deck.DealCard());
        }
    }
}
