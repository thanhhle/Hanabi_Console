namespace Hanabi
{
    public enum Player
    {
        SELF,
        OTHER,
    }

    public enum Color
    {
        GREEN = 0, 
        YELLOW = 1, 
        WHITE = 2, 
        BLUE = 3, 
        RED = 4,
    }

    public enum Action
    {
        HINT_COLOR,
        HINT_RANK,
        PLAY,
        DISCARD,
    }

    class Program
    {
        public static bool winGame = false;
        public static int availableHints = 8;
        public static int availableMistakes = 3;

        public static Card[] playerCards = {null, null, null, null, null};
        public static Card[] otherCards = {null, null, null, null, null};

        public static int[] board = {0, 0, 0, 0, 0};

        public static List<Card> deck = new List<Card>();
        public static List<Card> discardedDeck = new List<Card>();
        public static List<Hint> hints = new List<Hint>();

        public static readonly int CARD_QUANTITY = 5;
        public static readonly Color[] ALL_COLORS = (Color[])Enum.GetValues(typeof(Color));
        public static readonly int[] COUNTS = { 3, 2, 2, 2, 1 };

        public static Player currentPlayer = Player.SELF;

        public static void Main(string[] args)
        {
            Console.WriteLine("---------- MAKING A DECK ----------");
            MakeDeck();
            // PrintDeck(Target.DECK);

            for(int i = 0; i < CARD_QUANTITY; i++)
            {
                DrawCard(Player.SELF);
                DrawCard(Player.OTHER);
            }

            Console.WriteLine("\n---------- PLAYER'S CARDS ----------");
            PrintCards(Player.SELF);

            Console.WriteLine("\n---------- OTHER'S CARDS ----------");
            PrintCards(Player.OTHER);

            UpdateTextFile();

            Console.WriteLine("\n\n---------- START THE GAME ----------");
            while (!winGame && availableMistakes > 0 && deck.Count > 0)
            {
                RunGame();
            }
        }


        private static void MakeDeck()
        {
            foreach (Color color in ALL_COLORS)
            {
                for (int rank = 1; rank <= COUNTS.Length; rank++)
                {
                    for (int i = 0; i < COUNTS[rank - 1]; i++)
                    {
                        deck.Add(new Card(color, rank));
                    }
                }
            }

            ShuffleDeck();
        }


        private static void ShuffleDeck()  
        {  
            int n = deck.Count;  
            while (n > 1) 
            {  
                n--;  
                int k = new System.Random().Next(n + 1);  
                Card card = deck[k];  
                deck[k] = deck[n];  
                deck[n] = card;  
            }  
        }


        private static void DrawCard(Player player)
        {
            Card[] cards = GetCards(player);
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null)
                {
                    cards[i] = deck[0];
                    deck.RemoveAt(0);
                    return;
                }
            }
        }


        private static List<int> HintColor(Player target, Color color)
        {
            List<int> hint = new List<int>();
            Card[] cards = GetCards(target);
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i].color == color)
                {
                    hint.Add(i);
                }
            }

            return hint;
        }


        private static List<int> HintRank(Player target, int rank)
        {
            List<int> hint = new List<int>();
            Card[] cards = GetCards(target);
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i].rank == rank)
                {
                    hint.Add(i);
                }
            }
    
            return hint;
        }


        private static void Play(Player player, int cardIndex)
        {
            Card[] cards = GetCards(player);
            Card card = cards[cardIndex];
            Console.Write(card.color + "(" + card.rank + ")\n\n");

            int colorIndex = (int)card.color;
            if (card.rank == board[colorIndex] + 1)
            {
                board[colorIndex]++;
                if (board[colorIndex] == 5)
                {
                    winGame = true;
                }
            }
            else
            {
                availableMistakes--;
            }
       
            cards[cardIndex] = deck[0];
            deck.RemoveAt(0);
        }


        private static void Discard(Player target, int cardIndex)
        {
            Card[] cards = GetCards(target);
            Card card = cards[cardIndex];
            Console.Write(card.color + "(" + card.rank + ")\n\n");

            discardedDeck.Add(card);
            cards[cardIndex] = deck[0];
            deck.RemoveAt(0);
        }


        private static Card[] GetCards(Player player)
        {
            Card[] cards = player == Player.SELF ? playerCards : otherCards;
            return cards;
        }


        private static void RunGame()
        {
            if (currentPlayer == Player.SELF)
            {
                Console.WriteLine("PLAYER'S TURN");
            }
            else
            {
                Console.WriteLine("AI'S TURN");
            }
            
            Console.WriteLine("Pick one action:\n1. Hint Color\n2. Hint Rank\n3. Play\n4. Discard");
            int action = Convert.ToInt32(Console.ReadLine());

            if (action == 1)
            {
                Console.WriteLine("\nPick one color (1 - 5):");
                int count = 1;
                foreach (string color in Enum.GetNames(typeof(Color)))
                {
                    Console.WriteLine(count + ". " + color);
                    count++;
                }

                action = Convert.ToInt32(Console.ReadLine()) - 1;

                List<int> indexes = new List<int>();
                if (currentPlayer == Player.SELF)
                {
                    indexes = HintColor(Player.OTHER, (Color)action);
                }
                else
                {
                    indexes = HintColor(Player.SELF, (Color)action);
                }

                Hint hint = new Hint(currentPlayer, Action.HINT_COLOR, action, indexes);
                hints.Add(hint);
                Console.WriteLine(hint);
            }

            else if (action == 2)
            {
                Console.WriteLine("\nPick one rank (1 - 5):");
                action = Convert.ToInt32(Console.ReadLine());

                List<int> indexes = new List<int>();
                if (currentPlayer == Player.SELF)
                {
                    indexes = HintRank(Player.OTHER, action);
                }
                else
                {
                    indexes = HintRank(Player.SELF, action);
                }

                Hint hint = new Hint(currentPlayer, Action.HINT_RANK, action, indexes);
                hints.Add(hint);
                Console.WriteLine(hint);
            }

            else if (action == 3)
            {
                Console.WriteLine("\nEnter the card index (1 - 5) to be played");
                action = Convert.ToInt32(Console.ReadLine()) - 1;

                if (currentPlayer == Player.SELF)
                {
                    Console.Write("\n----> Player plays ");
                    Play(Player.SELF, action);     
                }
                else
                {
                    Console.Write("\n----> AI plays ");
                    Play(Player.OTHER, action);       
                } 
            }

            else if (action == 4)
            {
                Console.WriteLine("\nEnter the card index to be discarded");
                action = Convert.ToInt32(Console.ReadLine()) - 1;

                if (currentPlayer == Player.SELF)
                {
                    Console.Write("\n----> Player discards ");
                    Discard(Player.SELF, action);
                }
                else
                {
                    Console.Write("\n----> AI discards ");
                    Discard(Player.OTHER, action); 
                }
            }

            currentPlayer = currentPlayer == Player.SELF ? Player.OTHER : Player.SELF;
            UpdateTextFile();
        }


        private static void PrintCards(Player player)
        {
            foreach(Card card in GetCards(player))
            {
                Console.WriteLine(card);
            }
        }

        private static void PrintDeck()
        {
            foreach(Card card in deck)
            {
                Console.WriteLine(card);
            }
        }

        private static void PrintBoard()
        {
            for (int i = 0; i < ALL_COLORS.Length; i++)
            {
                Console.WriteLine((i + 1) + ": " + (Color)i + " - " + board[i]);
            }
        }

        private static void UpdateTextFile()
        {
            string output = "Available hints: " + availableHints;
            output += "\nAvailable mistakes: " + availableMistakes;
            output += "\nCards left in deck: " + deck.Count;

            output += "\n\n\n\n------------------------------\t\t AI \t\t------------------------------\n";
            foreach(Card card in otherCards)
            {
                output += card.color + "(" + card.rank + ")" + "\t\t";
            }

            output += "\n\n\n\n\n------------------------------\t\t BOARD \t\t------------------------------\n";
            for (int i = 0; i < ALL_COLORS.Length; i++)
            {
                output += (Color)i + "(" + board[i] + ")" + "\t\t";
            }

            output += "\n\n\n\n\n------------------------------\t\t PLAYER \t\t------------------------------\n";
            foreach(Card card in playerCards)
            {
                output += card.color + "(" + card.rank + ")" + "\t\t";
            }

            output += "\n\n\n\n\nDISCARDED\n";
            foreach(Card card in discardedDeck)
            {
                output += card.color + "(" + card.rank + ")" + "\n";
            }

            File.WriteAllText("Gameplay.txt", output);
        }
    }

}