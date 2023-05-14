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
    }
}