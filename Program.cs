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

        public static Player currentPlayer = Player.SELF;

        public static void Main(string[] args)
        {
            // Ignore the UNKNOWN color
            ALL_COLORS = ALL_COLORS.Take(ALL_COLORS.Count() - 1).ToArray();
            
            Console.WriteLine("---------- MAKE A DECK ----------");
            MakeDeck();
            // PrintDeck(Target.DECK);

            Console.WriteLine("\n\n---------- DEAL CARDS TO EACH PLAYER ----------");
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


        private static List<int> HintColor(Player target, Color color)
        {
            List<int> hint = new List<int>();
            Card[] cards = GetCards(target).Item1;
            Card[] knownCards = GetCards(target).Item2;
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i].color == color)
                {
                    hint.Add(i);
                    int knownRank = knownCards[i] != null ? knownCards[i].rank : 0;
                    knownCards[i] = new Card(color, knownRank);
                }
            }

            availableHints--;
            return hint;
        }


        private static List<int> HintRank(Player target, int rank)
        {
            List<int> hint = new List<int>();
            Card[] cards = GetCards(target).Item1;
            Card[] knownCards = GetCards(target).Item2;
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i].rank == rank)
                {
                    hint.Add(i);
                    Color knownColor = knownCards[i] != null ? knownCards[i].color : Color.UNKNOWN;
                    knownCards[i] = new Card(knownColor, rank);
                }
            }

            availableHints--;
            return hint;
        }


        private static void Play(Player player, int cardIndex)
        {
            Card[] cards = GetCards(player).Item1;
            Card card = cards[cardIndex];
            Console.Write(card.color + "(" + card.rank + ")\n\n");

            int colorIndex = (int)card.color;
            if (card.rank == board[colorIndex] + 1)
            {
                board[colorIndex]++;
                if (board[colorIndex] == 5)
                {
                    availableHints++;
                    UpdateGameStatus();
                }
            }
            else
            {
                availableMistakes--;
            }
       
            cards[cardIndex] = deck[0];
            cardLeft[deck[0].color]--;
            deck.RemoveAt(0);

            Card[] knownCards = GetCards(player).Item2;
            knownCards[cardIndex] = new Card(Color.UNKNOWN, 0);
        }


        private static void Discard(Player player, int cardIndex)
        {
            Card[] cards = GetCards(player).Item1;
            Card card = cards[cardIndex];
            Console.Write(card.color + "(" + card.rank + ")\n\n");

            discardedDeck.Add(card);
            cards[cardIndex] = deck[0];
            cardLeft[deck[0].color]--;
            deck.RemoveAt(0);

            Card[] knownCards = GetCards(player).Item2;
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
            Console.WriteLine("Pick one action:\n1. Hint Color\n2. Hint Rank\n3. Play\n4. Discard");
            int action = Convert.ToInt32(Console.ReadLine());

            if (action == 1)
            {
                Console.WriteLine("\nPick one color (1 - 5):");
                int count = 1;
                foreach (Color color in ALL_COLORS)
                {
                    Console.WriteLine(count + ". " + color);
                    count++;
                }

                action = Convert.ToInt32(Console.ReadLine()) - 1;

                List<int> indexes = HintColor(Player.OTHER, (Color)action);
                Hint hint = new Hint(currentPlayer, Action.HINT_COLOR, action, indexes);
                hints.Add(hint);
                Console.WriteLine(hint);
            }

            else if (action == 2)
            {
                Console.WriteLine("\nPick one rank (1 - 5):");
                action = Convert.ToInt32(Console.ReadLine());

                List<int> indexes = HintRank(Player.OTHER, action);
                Hint hint = new Hint(currentPlayer, Action.HINT_RANK, action, indexes);
                hints.Add(hint);
                Console.WriteLine(hint);
            }

            else if (action == 3)
            {
                Console.WriteLine("\nEnter the card index (1 - 5) to be played");
                action = Convert.ToInt32(Console.ReadLine()) - 1;
                Console.Write("\n----> Player plays ");
                Play(Player.SELF, action);
            }

            else if (action == 4)
            {
                Console.WriteLine("\nEnter the card index to be discarded");
                action = Convert.ToInt32(Console.ReadLine()) - 1;
                Console.Write("\n----> Player discards ");
                Discard(Player.SELF, action);
            }
        }


        private static void ProcessAITurn()
        {
            for (int i = 0; i < otherKnownCards.Length; i++)
            {
                Card knownCard = otherKnownCards[i];
                if (knownCard.rank != 0)
                {
                    int colorIndex = (int)otherCards[i].color;
                    if (knownCard.rank == board[colorIndex] + 1)
                    {
                        Console.Write("\n----> AI plays ");
                        Play(Player.OTHER, i);
                        return;
                    }
                }
            }

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
                    List<int> indexes = HintColor(Player.SELF, card.color);
                    if (playerKnownCards[indexes[0]].color == Color.UNKNOWN)
                    {
                        hint = new Hint(currentPlayer, Action.HINT_COLOR, (int)card.color, indexes);
                    }
                    else
                    {
                        indexes = HintRank(Player.SELF, card.rank);
                        if (playerKnownCards[indexes[0]].rank == 0)
                        {
                            hint = new Hint(currentPlayer, Action.HINT_RANK, card.rank, indexes);
                        }
                    }
                }
                else
                {
                    List<int> indexes = HintRank(Player.SELF, card.rank);
                    if (playerKnownCards[indexes[0]].rank == 0)
                    {
                        hint = new Hint(currentPlayer, Action.HINT_RANK, card.rank, indexes);
                    }
                    else
                    {
                        indexes = HintColor(Player.SELF, card.color);
                        if (playerKnownCards[indexes[0]].color == Color.UNKNOWN)
                        {
                            hint = new Hint(currentPlayer, Action.HINT_COLOR, (int)card.color, indexes);
                        }
                    }
                }

                if (hint != null)
                {
                    hints.Add(hint);
                    Console.WriteLine(hint);
                    return;
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
                        Console.Write("\n----> AI discards ");
                        Discard(currentPlayer, i);
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
                        Console.Write("\n----> AI discards ");
                        Discard(currentPlayer, i);
                        return;
                    }
                }
            }


            Dictionary<Color, int> ordered = cardLeft.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            Console.WriteLine(ordered);
            foreach (Color color in ordered.Keys)
            {
                for (int i = 0; i < otherKnownCards.Length; i++)
                {
                    Card knownCard = otherKnownCards[i];
                    if (knownCard.color == color)
                    {
                        Console.Write("\n----> AI discards ");
                        Discard(currentPlayer, i);
                        return;
                    }
                }
            }
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
    }

}