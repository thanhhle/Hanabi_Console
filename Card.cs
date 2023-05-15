using System;
using System.Collections;
using System.Collections.Generic;

namespace Hanabi
{
    public class Card
    {
        public Color color;
        public int rank;

        public Card(Color color, int rank)
        {
            this.color = color;
            this.rank = rank;
        }

        public override string ToString()
        {
            return "Color: " + color + "\t-\tRank: " + rank;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Card);
        }

        public bool Equals(Card? card)
        {
            return card != null && card.color == this.color && card.rank == this.rank;
        }
    }
}