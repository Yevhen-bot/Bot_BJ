using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackJack;
using Microsoft.AspNetCore.Routing;

namespace Bot
{
    // GameSession class is responsible for managing the game session
    class GameSession
    {
        public List<Player> Players { get; private set; }
        public Player Dealer { get; private set; }
        private readonly Deck _deck;

        public int PlayersCount {  get { return Players.Count; } }
        public bool IsStarted { get; set; }
        public GameSession()
        {
            Players = new();
            Dealer = new(-1, "dealer");
            Dealer.Stand = true;
            _deck = new Deck();
            IsStarted = false;
        }

        // Check if all players have made their turn
        public bool ToEnd()
        {
            return Players.All((x) => x.Stand);
        }

        public Player GetPlayer(long id)
        {
            Player r = null;
            foreach(var p in Players)
            {
                if(id == p.Id)
                {
                    r = p;
                    break;
                }
            }

            return r;
        }

        public void AddPlayer(Player player)
        {
            Players.Add(player);
        }

        public Card Start()
        {
            for(int i = 0; i < 2; i++)
            {
                foreach (Player player in Players)
                {
                    player.TakeCard(_deck.DealCard());
                }
            }

            var c = _deck.DealCard();
            Dealer.TakeCard(c);
            return c;
        }

        #region Unused(money focus)

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

        #endregion

        // Method that implements how the dealer operates
        public List<Card> DealerLogic()
        {
            List<Card> list = new List<Card>();
            while (Dealer.ShowValue() < 17)
            {
                var c = _deck.DealCard();
                Dealer.TakeCard(c);
                list.Add(c);
            }

            return list;
        }

        public Player End()
        {
            Players.Sort((p1, p2) => p2.ShowValue().CompareTo(p1.ShowValue()));
            int best = 0;
            Player b = null;
            for(int i = 0;i < Players.Count;i++)
            {
                if(Players[i].ShowValue() <= 21 && Players[i].ShowValue() > best)
                {
                    b = Players[i];
                    best = b.ShowValue();
                    break;
                }
            }

            if (best == 0 || (best < Dealer.ShowValue() && Dealer.ShowValue() <= 21))
                b = Dealer;

            return b;
        }

        public void HitPlayer(Player p)
        {
            p.TakeCard(_deck.DealCard());
        }
    }
}
