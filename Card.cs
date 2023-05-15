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
            return this.color + "(" + this.rank + ")";
        }

        public override bool Equals(object? obj)
        {
            Card? card = obj as Card;
            return card != null && card.color == this.color && card.rank == this.rank;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.color, this.rank);
        }
    }
}