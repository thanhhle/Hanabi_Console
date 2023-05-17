namespace Hanabi
{
    public class Hint
    {
        public Action action;
        public Color color;
        public int rank;
        public List<int> indexes;


        public Hint(Action action, Color color, List<int> indexes)
        {
            this.action = action;
            this.color = color;
            this.indexes = indexes;
        }


        public Hint(Action action, int rank, List<int> indexes)
        {
            this.action = action;
            this.rank = rank;
            this.indexes = indexes;
        }


        public override string ToString()
        {
            string output = "\n----> ";
            if (Program.currentPlayer == Player.SELF)
            {
                output += "Player hints AI about all their ";
            }
            else
            {
                output += "AI hints player about all their ";
            }

            if (this.action == Action.HINT_COLOR)
            {
                output += color + " cards: ";
            }
            else
            {
                output += rank + " cards: ";
            }

            foreach (int index in indexes)
            {
                output += (index + 1) + " ";
            }

            output += "\n";

            return output;
        }
    }
}