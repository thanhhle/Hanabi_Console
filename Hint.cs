namespace Hanabi
{
    public class Hint
    {
        public Player player;
        public Action action;
        public int colorRank;
        public List<int> indexes;

        public Hint(Player player, Action action, int colorRank, List<int> indexes)
        {
            this.player = player;
            this.action = action;
            this.colorRank = colorRank;
            this.indexes = indexes;
        }

        public override string ToString()
        {
            string output = "\n----> ";
            if (this.player == Player.SELF)
            {
                output += "Player hints AI about all their ";
            }
            else
            {
                output += "AI hints player about all their ";
            }

            if (this.action == Action.HINT_COLOR)
            {
                output += (Color)colorRank + " cards: ";
            }
            else
            {
                output += colorRank + " cards: ";
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