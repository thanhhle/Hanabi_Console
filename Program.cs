using System;
using System.Collections;
using System.Collections.Generic;

namespace Hanabi
{
    public enum Player
    {
        SELF,
        OTHER,
    }

    public enum Color
    {
        GREEN,
        YELLOW,
        WHITE,
        BLUE,
        RED,
        UNKNOWN,
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

        public static Card[] playerCards = new Card[5];
        public static Card[] otherCards = new Card[5];

        public static Card[] playerKnownCards = new Card[5];
        public static Card[] otherKnownCards = new Card[5];

        public static int[] board = {0, 0, 0, 0, 0};

        public static List<Card> deck = new List<Card>();
        public static List<Card> discardedDeck = new List<Card>();
        public static List<Hint> hints = new List<Hint>();

        public static Dictionary<Color, int> cardLeft = new Dictionary<Color, int>();

        public static int CARD_QUANTITY = 5;
        public static Color[] ALL_COLORS = (Color[])Enum.GetValues(typeof(Color));
        public static int[] COUNTS = { 3, 2, 2, 2, 1 };

        public static Player currentPlayer = (Player)new Random().Next(0, 2);

        public static void Main(string[] args)
        {
            // Filter out the UNKNOWN color
            ALL_COLORS = ALL_COLORS.Take(ALL_COLORS.Count() - 1).ToArray();
            
            MakeDeck();
            // PrintDeck(Target.DECK);

            Console.WriteLine("\n\n---------- DEAL " + CARD_QUANTITY + " CARDS TO EACH PLAYER ----------");
            for(int i = 0; i < CARD_QUANTITY; i++)
            {
                DealCard(Player.SELF);
                DealCard(Player.OTHER);

                playerKnownCards[i] = new Card(Color.UNKNOWN, 0);
                otherKnownCards[i] = new Card(Color.UNKNOWN, 0);
            }

            UpdateTextFile();

            Console.WriteLine("\n\n---------- START THE GAME ----------");
            while (!winGame && availableMistakes > 0 && deck.Count > 0)
            {
                RunGame();
            }

            if (winGame)
            {
                Console.WriteLine("Congratulations. You won!");
            }
            else
            {
                Console.WriteLine("Game ends. You lose!");
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
                        if (cardLeft.ContainsKey(color))
                        {
                            cardLeft[color] = cardLeft[color] + 1;
                        }
                        else
                        {
                            cardLeft.Add(color, 1);
                        }
                    }
                }
            }

            ShuffleDeck();
            Console.WriteLine("---------- MAKE A DECK OF " + deck.Count + " CARDS ----------");
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


        private static void DealCard(Player player)
        {
            Card[] cards = GetCards(player).Item1;
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null)
                {
                    cards[i] = deck[0];
                    cardLeft[deck[0].color]--;
                    deck.RemoveAt(0);
                    return;
                }
            }
        }


        private static List<int> HintColor(Color color)
        {
            Player target = currentPlayer == Player.SELF ? Player.OTHER : Player.SELF;
            List<int> indexes = new List<int>();
            Card[] cards = GetCards(target).Item1;
            Card[] knownCards = GetCards(target).Item2;
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i].color == color)
                {
                    indexes.Add(i);
                }
            }

            return indexes;
        }


        private static List<int> HintRank(int rank)
        {
            Player target = currentPlayer == Player.SELF ? Player.OTHER : Player.SELF;
            List<int> indexes = new List<int>();
            Card[] cards = GetCards(target).Item1;
            Card[] knownCards = GetCards(target).Item2;
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i].rank == rank)
                {
                    indexes.Add(i);
                }
            }

            return indexes;
        }


        private static void GiveHint(Color color, List<int> indexes)
        {
            Player target = currentPlayer == Player.SELF ? Player.OTHER : Player.SELF;
            Card[] knownCards = GetCards(target).Item2;

            foreach (int i in indexes)
            {
                int knownRank = knownCards[i] != null ? knownCards[i].rank : 0;
                knownCards[i] = new Card(color, knownRank);
            }

            availableHints--;

            Hint hint = new Hint(Action.HINT_COLOR, color, indexes);
            hints.Add(hint);
            Console.WriteLine(hint);
        }


        private static void GiveHint(int rank, List<int> indexes)
        {
            Player target = currentPlayer == Player.SELF ? Player.OTHER : Player.SELF;
            Card[] knownCards = GetCards(target).Item2;

            foreach (int i in indexes)
            {
                Color knownColor = knownCards[i] != null ? knownCards[i].color : Color.UNKNOWN;
                knownCards[i] = new Card(knownColor, rank);
            }

            availableHints--;

            Hint hint = new Hint(Action.HINT_RANK, rank, indexes);
            hints.Add(hint);
            Console.WriteLine(hint);
        }


        private static void Play(int cardIndex)
        {
            Card[] cards = GetCards(currentPlayer).Item1;
            Card card = cards[cardIndex];
            if (currentPlayer == Player.SELF)
            {
                Console.WriteLine("\n----> Player plays " + card + "\n\n");
            }
            else
            {
                Console.WriteLine("\n----> AI plays " + card + "\n\n");
            }

            int colorIndex = (int)card.color;
            if (card.rank == board[colorIndex] + 1)
            {
                board[colorIndex]++;
                if (board[colorIndex] == 5)
                {
                    availableHints++;
                    UpdateGameStatus();
                    Console.WriteLine("Full " + card.color + " firework was built!");
                }
            }
            else
            {
                availableMistakes--;
            }
       
            cards[cardIndex] = deck[0];
            cardLeft[deck[0].color]--;
            deck.RemoveAt(0);

            Card[] knownCards = GetCards(currentPlayer).Item2;
            knownCards[cardIndex] = new Card(Color.UNKNOWN, 0);
        }


        private static void Discard(int cardIndex)
        {
            Card[] cards = GetCards(currentPlayer).Item1;
            Card card = cards[cardIndex];
            if (currentPlayer == Player.SELF)
            {
                Console.WriteLine("\n----> Player discards " + card + "\n\n");
            }
            else
            {
                Console.WriteLine("\n----> AI discards " + card + "\n\n");
            }

            discardedDeck.Add(card);
            cards[cardIndex] = deck[0];
            cardLeft[deck[0].color]--;
            deck.RemoveAt(0);

            Card[] knownCards = GetCards(currentPlayer).Item2;
            knownCards[cardIndex] = new Card(Color.UNKNOWN, 0);
            availableHints++;
        }


        private static (Card[], Card[]) GetCards(Player player)
        {
            return player == Player.SELF ? (playerCards, playerKnownCards) : (otherCards, otherKnownCards);
        }


        private static void RunGame()
        {
            if (currentPlayer == Player.SELF)
            {
                Console.WriteLine("\nPLAYER'S TURN");
                ProcessPlayerTurn();
            }
            else
            {
                Console.WriteLine("\nAI'S TURN");
                ProcessAITurn();
            }

            currentPlayer = currentPlayer == Player.SELF ? Player.OTHER : Player.SELF;
            UpdateTextFile();
        }


        private static void PrintCards(Player player)
        {
            foreach(Card card in GetCards(player).Item1)
            {
                Console.WriteLine(card);
            }
        }

        private static void PrintKnownCards(Player player)
        {
            foreach(Card card in GetCards(player).Item2)
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

            output += "\n\n\n\n\n------------------------------\t\t AI'S KNOWN CARDS \t\t------------------------------\n";
            foreach(Card card in otherKnownCards)
            {
                output += card + "\t\t";
            }

            output += "\n\n\n\n\n------------------------------\t\t AI \t\t------------------------------\n";
            foreach(Card card in otherCards)
            {
                output += card + "\t\t";
            }

            output += "\n\n\n\n\n------------------------------\t\t BOARD \t\t------------------------------\n";
            for (int i = 0; i < ALL_COLORS.Length; i++)
            {
                output += (Color)i + "(" + board[i] + ")" + "\t\t";
            }

            output += "\n\n\n\n\n------------------------------\t\t PLAYER \t\t------------------------------\n";
            foreach(Card card in playerKnownCards)
            {
                output += card + "\t\t";
            }

            output += "\n\n\n\n\nDISCARDED DECK\n";
            foreach(Card card in discardedDeck)
            {
                output += card + "\n";
            }

            File.WriteAllText("Gameplay.txt", output);
        }


        private static void ProcessPlayerTurn()
        {
            bool isValid = false;
            int option = 0;
            while(!isValid)
            {
                if (availableHints > 0)
                {
                    Console.WriteLine("Pick one option:\n1. Play\n2. Discard\n3. Hint Color\n4. Hint Rank");
                    (option, isValid) = ProcessInput(Console.ReadLine(), 1, 4);
                }
                else
                {
                    Console.WriteLine("Pick one option:\n1. Play\n2. Discard");
                    (option, isValid) = ProcessInput(Console.ReadLine(), 1, 2);
                }
            }
            

            if (option == 1)
            {
                isValid = false;
                int index = 0;
                while (!isValid)
                {
                    Console.WriteLine("\nEnter the card index (1 - 5) to be played");
                    (index, isValid) = ProcessInput(Console.ReadLine(), 1, 5);
                }

                Play(index - 1);
            }

            else if (option == 2)
            {
                isValid = false;
                int index = 0;
                while (!isValid)
                {
                    Console.WriteLine("\nEnter the card index (1 - 5) to be discarded");
                    (index, isValid) = ProcessInput(Console.ReadLine(), 1, 5);
                }

                Discard(index - 1);
            }

            else if (option == 3 && availableHints > 0)
            {
                HashSet<Color> colors = getCardColors(otherCards);
                isValid = false;
                int index = 0;
                while (!isValid)
                {
                    Console.WriteLine("\nPick one color (1 - " + colors.Count + ")");
                    int count = 1;
                    foreach (Color c in colors)
                    {
                        Console.WriteLine(count + ". " + c);
                        count++;
                    }
 
                    (index, isValid) = ProcessInput(Console.ReadLine(), 1, colors.Count);
                }

                Color color = colors.ToList()[index - 1];
                List<int> indexes = HintColor(color);
                GiveHint(color, indexes);
            }

            else if (option == 4 && availableHints > 0)
            {
                HashSet<int> ranks = getCardRanks(otherCards);
                isValid = false;
                int index = 0;
                while (!isValid)
                {
                    Console.WriteLine("\nPick one rank (1 - " + ranks.Count + ")");
                    int count = 1;
                    foreach (int r in ranks)
                    {
                        Console.WriteLine(count + ". " + r);
                        count++;
                    }
 
                    (index, isValid) = ProcessInput(Console.ReadLine(), 1, ranks.Count);
                }

                int rank = ranks.ToList()[index - 1];
                List<int> indexes = HintRank(rank);
                GiveHint(rank, indexes);
            }
        }


        private static void ProcessAITurn()
        {
            // Play
            for (int i = 0; i < otherKnownCards.Length; i++)
            {
                Card knownCard = otherKnownCards[i];
                if (knownCard.rank != 0)
                {
                    int colorIndex = (int)otherCards[i].color;
                    if (knownCard.rank == board[colorIndex] + 1)
                    {
                        Play(i);
                        return;
                    }
                }
            }

            // Give hints
            if (availableHints > 0)
            {
                Card? card = null;
                for (int i = 0; i < playerCards.Length; i++)
                {
                    int colorIndex = (int)playerCards[i].color;
                    int rank = playerCards[i].rank;
                    if (playerCards[i].rank == board[colorIndex] + 1)
                    {
                        card = playerCards[i];
                        break;
                    }
                }

                if (card != null && availableHints > 0)
                {
                    Hint? hint = null;
                    if (GetColorCount(card.color, playerCards) > GetRankCount(card.rank, playerCards))
                    {
                        List<int> indexes = HintColor(card.color);
                        if (playerKnownCards[indexes[0]].color == Color.UNKNOWN)
                        {
                            hint = new Hint(Action.HINT_COLOR, card.color, indexes);
                        }
                        else
                        {
                            indexes = HintRank(card.rank);
                            if (playerKnownCards[indexes[0]].rank == 0)
                            {
                                hint = new Hint(Action.HINT_RANK, card.rank, indexes);
                            }
                        }
                    }
                    else
                    {
                        List<int> indexes = HintRank(card.rank);
                        if (playerKnownCards[indexes[0]].rank == 0)
                        {
                            hint = new Hint(Action.HINT_RANK, card.rank, indexes);
                        }
                        else
                        {
                            indexes = HintColor(card.color);
                            if (playerKnownCards[indexes[0]].color == Color.UNKNOWN)
                            {
                                hint = new Hint(Action.HINT_COLOR, card.color, indexes);
                            }
                        }
                    }

                    if (hint != null)
                    {
                        if (hint.action == Action.HINT_COLOR)
                        {
                            GiveHint(hint.color, hint.indexes);
                        }
                        else
                        {
                            GiveHint(hint.rank, hint.indexes);
                        }

                        return;
                    }
                }
            }
            
            // Discard a card
            for (int i = 0; i < otherKnownCards.Length; i++)
            {
                Card knownCard = otherKnownCards[i];
                if (knownCard.rank > 0 && knownCard.color != Color.UNKNOWN)
                {
                    int colorIndex = (int)knownCard.color;
                    if (knownCard.rank <= board[colorIndex])
                    {
                        Discard(i);
                        return;
                    }
                }
            }

            for (int i = 0; i < otherKnownCards.Length; i++)
            {
                Card knownCard = otherKnownCards[i];
                foreach (Card playerCard in playerCards)
                {
                    if (knownCard == playerCard)
                    {
                        Discard(i);
                        return;
                    }
                }
            }

            Dictionary<Color, int> ordered = cardLeft.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            foreach (Color color in ordered.Keys)
            {
                for (int i = 0; i < otherKnownCards.Length; i++)
                {
                    Card knownCard = otherKnownCards[i];
                    if (knownCard.color == color)
                    {
                        Discard(i);
                        return;
                    }
                }
            }

            int randIndex = new Random().Next(0, ordered.Count);
            Discard(randIndex);
        }


        private static int GetColorCount(Color color, Card[] cards)
        {
            int count = 0;
            foreach (Card card in cards)
            {
                if (card.color == color)
                {
                    count++;
                }
            }

            return count;
        }


        private static int GetRankCount(int rank, Card[] cards)
        {
            int count = 0;
            foreach (Card card in cards)
            {
                if (card.rank == rank)
                {
                    count++;
                }
            }

            return count;
        }


        private static void UpdateGameStatus()
        {
            int count = 0;
            foreach(int rank in board)
            {
                if (rank == 5)
                {
                    count++;
                }
            }

            winGame = count == 5 ? true : false;
        }


        private static HashSet<Color> getCardColors(Card[] cards)
        {
            HashSet<Color> colors = new HashSet<Color>();
            foreach(Card card in cards)
            {
                colors.Add(card.color);
            }

            return colors;
        }


        private static HashSet<int> getCardRanks(Card[] cards)
        {
            HashSet<int> ranks = new HashSet<int>();
            foreach(Card card in cards)
            {
                ranks.Add(card.rank);
            }

            return ranks;
        }


        private static (int, bool) ProcessInput(string? input, int min, int max)
        {
            bool isValid = false;
            bool isNumeric = !string.IsNullOrEmpty(input) && input.All(Char.IsDigit);
            int inputInt = 0;
            if (isNumeric)
            {
                inputInt = Convert.ToInt32(input);
                if (inputInt >= min && inputInt <= max)
                {
                    isValid = true;
                }
            }

            if (!isValid)
            {
                Console.WriteLine("\n!!! Invalid option. Please pick again !!!\n");
            }

            return (inputInt, isValid);
        }
    }
}