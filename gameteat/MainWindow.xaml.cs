
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace JapanMonopoly
{
    public partial class MainWindow : Window
    {
        private class City
        {
            public string Name;
            public int Price;
            public int Rent;
            public int Owner = -1;
            public string Color;

            public City(string name, int price, int rent, string color)
            {
                Name = name;
                Price = price;
                Rent = rent;
                Color = color;
            }
        }

        private class Player
        {
            public string Name;
            public int Money = 15000;
            public int Position = 0;
            public int Owned = 0;
            public bool Active = true;

            public Player(string name) { Name = name; }
        }

        private readonly Random random = new Random();
        private readonly List<Player> players = new List<Player>();

        private readonly List<City> cities = new List<City>();
        private bool busy = false;
        private bool waitingForPurchase = false;
        private Player? pendingPlayer;
        private int currentPlayerIndex = 0;
        private int turn = 1;

        private readonly string green = "#A8C5A5";
        private readonly string pink = "#E9A9B5";
        private readonly string blue = "#A8C5D9";
        private readonly string gold = "#E8C982";

        public MainWindow()
        {
            InitializeComponent();

            cities.Add(new City("起點", 0, 0, green));
            cities.Add(new City("橫濱", 1800, 250, pink));
            cities.Add(new City("靜岡", 1500, 200, green));
            cities.Add(new City("名古屋", 2500, 400, gold));
            cities.Add(new City("京都", 3000, 500, pink));
            cities.Add(new City("大阪", 3500, 600, gold));
            cities.Add(new City("神戶", 2200, 350, blue));
            cities.Add(new City("旅遊卡", 0, 0, blue));
            cities.Add(new City("岡山", 1600, 220, green));
            cities.Add(new City("廣島", 2400, 380, blue));
            cities.Add(new City("福岡", 3200, 550, gold));
            cities.Add(new City("鹿兒島", 1800, 280, green));
            cities.Add(new City("沖繩", 2800, 450, pink));
            cities.Add(new City("札幌", 3000, 500, blue));
            cities.Add(new City("仙台", 2200, 350, green));
            cities.Add(new City("新潟", 1700, 250, blue));
            cities.Add(new City("金澤", 2000, 320, pink));
            cities.Add(new City("長野", 1900, 300, green));
            cities.Add(new City("機會卡", 0, 0, gold));
            cities.Add(new City("東京", 4500, 800, pink));

            players.Add(new Player("玩家 1"));
            players.Add(new Player("玩家 2"));
            players.Add(new Player("玩家 3"));
            players.Add(new Player("玩家 4"));

            BoardCanvas.SizeChanged += (s, e) => DrawBoard();

            Log("🌸 歡迎來到日本環島之旅！");
            Log("四名真人玩家輪流遊玩，每位玩家有 ¥15,000 旅行資金。");
            UpdateUI();
            Loaded += (s, e) => DrawBoard();
        }

        private Brush BrushOf(string color)
        {
            return new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(color));
        }

        private void DrawBoard()
        {
            if (BoardCanvas.ActualWidth < 100 ||
                BoardCanvas.ActualHeight < 100)
                return;

            BoardCanvas.Children.Clear();

            double w = BoardCanvas.ActualWidth / 7.0;
            double h = BoardCanvas.ActualHeight / 5.0;

            // 20 格環形棋盤
            var positions = new List<Point>();

            for (int x = 0; x < 7; x++)
                positions.Add(new Point(x, 0));

            for (int y = 1; y < 5; y++)
                positions.Add(new Point(6, y));

            for (int x = 5; x >= 0; x--)
                positions.Add(new Point(x, 4));

            for (int y = 3; y >= 1; y--)
                positions.Add(new Point(0, y));

            // 中央富士山與櫻花太陽
            var sun = new Ellipse
            {
                Width = 45,
                Height = 45,
                Fill = BrushOf("#E9A9B5")
            };
            Canvas.SetLeft(sun, w * 3.1);
            Canvas.SetTop(sun, h * 1.35);
            BoardCanvas.Children.Add(sun);

            var fuji = new Polygon
            {
                Points = new PointCollection
                {
                    new Point(0, h * 0.85),
                    new Point(w * 1.25, 0),
                    new Point(w * 2.5, h * 0.85)
                },
                Fill = BrushOf("#A8C5D9")
            };
            Canvas.SetLeft(fuji, w * 2.2);
            Canvas.SetTop(fuji, h * 1.65);
            BoardCanvas.Children.Add(fuji);

            AddText("日本環島之旅", w * 2.0, h * 2.6,
                w * 3.0, 25, 19, "#405F4D", true);

            AddText("TOKYO  ·  KYOTO  ·  OSAKA",
                w * 2.0, h * 3.0, w * 3.0, 20,
                10, "#87918A", true);

            // 城市格子
            for (int i = 0; i < cities.Count; i++)
            {
                City city = cities[i];
                Point p = positions[i];

                double x = p.X * w + 3;
                double y = p.Y * h + 3;
                double tw = w - 6;
                double th = h - 6;

                var card = new Border
                {
                    Width = tw,
                    Height = th,
                    CornerRadius = new CornerRadius(8),
                    Background = BrushOf("#FFFDF7"),
                    BorderBrush = BrushOf("#E8E0D4"),
                    BorderThickness = new Thickness(1)
                };

                Canvas.SetLeft(card, x);
                Canvas.SetTop(card, y);
                BoardCanvas.Children.Add(card);

                var stripe = new Rectangle
                {
                    Width = tw - 4,
                    Height = 6,
                    RadiusX = 3,
                    RadiusY = 3,
                    Fill = BrushOf(city.Color)
                };

                Canvas.SetLeft(stripe, x + 2);
                Canvas.SetTop(stripe, y + 2);
                BoardCanvas.Children.Add(stripe);

                string label = city.Name;
                if (city.Owner >= 0 && city.Owner < players.Count)
                    label += "\n●" + players[city.Owner].Name;

                AddText(label, x + 2, y + th * 0.25,
                    tw - 4, th * 0.45,
                    Math.Max(9, Math.Min(13, tw / 5)),
                    "#253B36", true);

                if (city.Price > 0)
                {
                    AddText("¥" + city.Price,
                        x + 1, y + th * 0.70,
                        tw - 2, 16, 9, "#87918A", false);
                }
            }

            // 玩家棋子
            string[] tokenColors = { "#4D8DC8", "#D9788A", "#8B6BB1", "#D49A45" };
            for (int i = 0; i < players.Count; i++)
                if (players[i].Active)
                    DrawToken(players[i], positions, w, h,
                        tokenColors[i], (i - (players.Count - 1) / 2.0) * 8);
        }

        private int PlayerIndex(Player player)
        {
            return players.IndexOf(player);
        }

        private void AddText(string text, double x, double y,
            double width, double height, double size,
            string color, bool bold)
        {
            var tb = new TextBlock
            {
                Text = text,
                Width = width,
                Height = height,
                FontSize = size,
                Foreground = BrushOf(color),
                FontWeight = bold
                    ? FontWeights.Bold : FontWeights.Normal,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            };

            Canvas.SetLeft(tb, x);
            Canvas.SetTop(tb, y);
            BoardCanvas.Children.Add(tb);
        }

        private void DrawToken(Player player, List<Point> positions,
            double w, double h, string color, double offset)
        {
            Point p = positions[player.Position];

            var token = new Ellipse
            {
                Width = 16,
                Height = 16,
                Fill = BrushOf(color),
                Stroke = Brushes.White,
                StrokeThickness = 2
            };

            Canvas.SetLeft(token,
                p.X * w + w / 2 - 8 + offset);
            Canvas.SetTop(token,
                p.Y * h + h * 0.78 - 8);

            BoardCanvas.Children.Add(token);
        }

        private async void RollButton_Click(
            object sender, RoutedEventArgs e)
        {
            if (busy) return;
            Player player = players[currentPlayerIndex];

            if (waitingForPurchase)
            {
                waitingForPurchase = false;
                BuyButton.IsEnabled = false;
                pendingPlayer = null;
                FinishTurn();
                return;
            }

            busy = true;
            RollButton.IsEnabled = false;

            int dice = random.Next(1, 7);
            DiceText.Text = "🎲 " + dice;
            Log(player.Name + " 擲出了 " + dice + " 點。");

            await MovePlayer(player, dice);
            if (player.Active && ResolveLanding(player))
            {
                pendingPlayer = player;
                waitingForPurchase = true;
                BuyButton.IsEnabled = true;
                RollButton.Content = "略過購買／支付租金";
                RollButton.IsEnabled = true;
                busy = false;
                UpdateUI();
                return;
            }

            busy = false;
            FinishTurn();
        }

        private async Task MovePlayer(Player player, int steps, int delay = 180)
        {
            for (int i = 0; i < steps; i++)
            {
                player.Position++;

                if (player.Position >= cities.Count)
                {
                    player.Position = 0;
                    player.Money += 2000;
                    Log("🎉 經過起點，獲得 ¥2,000！");
                }

                DrawBoard();
                UpdateUI();
                await Task.Delay(delay);
            }

            Log(player.Name + " 抵達「" +
                cities[player.Position].Name + "」。");
        }

        private bool ResolveLanding(Player player)
        {
            City city = cities[player.Position];

            if (city.Name == "旅遊卡")
            {
                int bonus = random.Next(300, 1001);
                player.Money += bonus;
                Log("🧳 旅行獎勵：獲得 ¥" + bonus + "！");
            }
            else if (city.Name == "機會卡")
            {
                int cost = random.Next(200, 701);
                Charge(player, cost, "突發事件");
                Log("🌸 突發事件：支出 ¥" + cost + "。");
            }
            else if (city.Price > 0 && city.Owner == -1 &&
                     player.Money >= city.Price)
            {
                return true;
            }
            else if (city.Owner != -1 && city.Owner != PlayerIndex(player))
            {
                PayRent(player, city);
            }

            UpdateUI();
            DrawBoard();
            return false;
        }

        private void BuyButton_Click(
            object sender, RoutedEventArgs e)
        {
            if (busy || !waitingForPurchase) return;

            Player player = pendingPlayer ?? players[currentPlayerIndex];
            City city = cities[player.Position];

            if (city.Price > 0 && city.Owner == -1 &&
                player.Money >= city.Price)
            {
                BuyCity(player, city);
            }

            waitingForPurchase = false;
            BuyButton.IsEnabled = false;
            RollButton.Content = "🎲 擲骰子・開始旅行";
            UpdateUI();
            DrawBoard();

            pendingPlayer = null;
            FinishTurn();
        }

        private void BuyCity(Player buyer, City city)
        {
            if (city.Owner != -1 || buyer.Money < city.Price) return;

            int buyerIndex = PlayerIndex(buyer);
            buyer.Money -= city.Price;
            buyer.Owned++;
            city.Owner = buyerIndex;

            Log("🏡 " + buyer.Name + " 買下了「" + city.Name +
                "」，花費 ¥" + city.Price + "。");
        }

        private void FinishTurn()
        {
            if (players.Count(p => p.Active) <= 1)
            {
                busy = false;
                RollButton.IsEnabled = false;
                BuyButton.IsEnabled = false;
                UpdateUI();
                DrawBoard();
                return;
            }

            do
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
                if (currentPlayerIndex == 0)
                    turn++;
            }
            while (!players[currentPlayerIndex].Active);

            busy = false;
            RollButton.Content = "🎲 擲骰子・開始旅行";
            RollButton.IsEnabled = true;
            UpdateUI();
            DrawBoard();
        }

        private void PayRent(Player player, City city)
        {
            if (city.Owner < 0 || city.Owner == PlayerIndex(player)) return;

            Player owner = players[city.Owner];
            Charge(player, city.Rent, "租金");
            if (player.Active)
                owner.Money += city.Rent;
            Log("🏠 " + player.Name + " 支付租金 ¥" + city.Rent + " 給 " +
                owner.Name + "。");
        }

        private void Charge(Player player, int amount, string reason)
        {
            player.Money -= amount;
            if (player.Money < 0)
                Eliminate(player, reason);
        }

        private void Eliminate(Player player, string reason)
        {
            if (!player.Active) return;

            player.Active = false;
            player.Money = 0;
            foreach (City city in cities)
            {
                if (city.Owner == PlayerIndex(player))
                    city.Owner = -1;
            }
            player.Owned = 0;
            Log("💥 " + player.Name + " 因為無法支付" + reason + "而破產，退出遊戲！");

            if (players.Count(p => p.Active) == 1)
            {
                Player winner = players.Find(p => p.Active);
                Log("🏆 " + winner.Name + " 成為最後的勝利者！");
                RollButton.IsEnabled = false;
                BuyButton.IsEnabled = false;
            }
        }

        private void RulesButton_Click(
            object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "【日本環島大富翁】\n\n" +
                "1. 擲骰子移動棋子。\n" +
                "2. 抵達未被購買的城市，可以買地。\n" +
                "3. 踩到其他玩家的土地只能支付租金，不能買走。\n" +
                "4. 無法支付租金或事件支出就會破產，土地變成無主。\n" +
                "5. 經過起點可獲得 ¥2,000。\n" +
                "6. 四名真人玩家依序輪流操作。\n" +
                "7. 最後存活的玩家獲勝。",
                "遊戲規則",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void UpdateUI()
        {
            Player currentPlayer = players[currentPlayerIndex];
            MoneyText.Text = "¥ " + currentPlayer.Money.ToString("N0");
            PropertyText.Text = currentPlayer.Owned + " 座城市";
            PositionText.Text = cities[currentPlayer.Position].Name;
            TurnText.Text = "第 " + turn + " 回合";
            PlayerText.Text = currentPlayer.Active
                ? "目前玩家：「" + currentPlayer.Name + "」"
                : "目前玩家已破產";
        }

        private void Log(string message)
        {
            LogBox.AppendText(message + Environment.NewLine);
            LogBox.ScrollToEnd();
        }
    }
}
