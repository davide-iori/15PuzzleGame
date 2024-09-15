using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _15PuzzleGame {
    internal class FifteenPuzzle {

        const int GRIDSIZE = 4; // 4x4 is standard game size
        const int BLOCKCOUNT = 16;

        Random rnd = new Random();

        List<Button> tiles = new List<Button>();
        int moveCount = 0;
        DateTime startTime;


        /// <summary>
        /// Represents a tile of the puzzle. To be engased in the Tag property of a button.
        /// </summary>
        public class Tile {

            /// <summary>
            /// The place this tile has to be in for the puzzle to be solved.
            /// </summary>
            int orderedNumber;

            /// <summary>
            /// The place this tile currently is in.
            /// </summary>
            public int currentNumber;

            /// <summary>
            /// The x coordinate of this tile in the grid.
            /// </summary>
            public int X;
            /// <summary>
            /// The y coordinate of this tile in the grid.
            /// </summary>
            public int Y;

            public int InvX {

                get { return (GRIDSIZE - 1) - X; }
            }
            public int InvY {

                get { return (GRIDSIZE - 1) - Y; }
            }

            public Tile(int OrderedNumer) {

                orderedNumber = OrderedNumer;

                currentNumber = OrderedNumer;

                X = OrderedNumer % GRIDSIZE;
                Y = OrderedNumer / GRIDSIZE;
            }

            public Tile(int OrderedNumer, int CurrentNumber) : this(OrderedNumer) {

                this.currentNumber = CurrentNumber;
            }

            /// <summary>
            /// Checks whether the tile is in an empty space or not.
            /// </summary>
            public bool IsEmpty {

                get { return currentNumber >= (BLOCKCOUNT - 1); }
            }

            /// <summary>
            /// Checks whether the tile is at the right place or not.
            /// </summary>
            public bool IsRightPlace {

                get { return (currentNumber == orderedNumber); }
            }

            /// <summary>
            /// Checks whether this tile is adjacent to the one given as a parameter.
            /// </summary>
            /// <param name="otherTile"></param>
            /// <returns></returns>
            public bool NearestWith(Tile otherTile) {
                int dx = (X - otherTile.X);
                int dy = (Y - otherTile.Y);

                if ((dx == 0) && (dy <= 1) && (dy >= -1)) return true;
                if ((dy == 0) && (dx <= 1) && (dx >= -1)) return true;

                return false;
            }

            public override string ToString() {
                return (currentNumber + 1).ToString();
            }
        }

        public static void Main(string[] args) {
            FifteenPuzzle Game = new FifteenPuzzle();
            Application.Run(Game.CreateForm());
        }

        /// <summary>
        /// Style and formatting shenanigans to create the actual play board.
        /// Every button contains a Tile instance in its Tag property, so that when a button is swapped the corresponding tile follows along.
        /// </summary>
        /// <returns></returns>
        Form CreateForm() {

            int btnSize = 50;
            int btnMargin = 3;
            int formEdge = 9;

            Font btnFont = new Font("Arial", 15.75F, FontStyle.Regular);

            Button startBtn = new Button();
            startBtn.Location = new Point(formEdge, (GRIDSIZE * (btnMargin + btnSize)) + formEdge);
            startBtn.Size = new Size(86, 23);
            startBtn.Font = new Font("Arial", 9.75F, FontStyle.Regular);
            startBtn.Text = "New Game";
            startBtn.UseVisualStyleBackColor = true;
            startBtn.TabStop = false;

            startBtn.Click += new EventHandler(NewGame);

            int formWidth = (GRIDSIZE * btnSize) + ((GRIDSIZE - 1) * btnMargin) + (formEdge * 2);
            int formHeigth = formWidth + startBtn.Height;

            Form form = new Form();
            form.Text = "Fifteen puzzle game";
            form.ClientSize = new Size(formWidth, formHeigth);
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
            form.SuspendLayout();

            // place the tiles on the board
            for (int i = 0; i < BLOCKCOUNT; i++) {

                Button btn = new Button();
                Tile tile = new Tile(i);

                int PosX = formEdge + (tile.X) * (btnSize + btnMargin);
                int PosY = formEdge + (tile.Y) * (btnSize + btnMargin);
                btn.Location = new Point(PosX, PosY);

                btn.Size = new Size(btnSize, btnSize);
                btn.Font = btnFont;

                btn.Text = tile.ToString();
                btn.Tag = tile;
                btn.UseVisualStyleBackColor = true;
                btn.TabStop = false;

                btn.Enabled = false;
                if (tile.IsEmpty) btn.Visible = false;

                btn.Click += new EventHandler(MovePuzzle);

                tiles.Add(btn);
                form.Controls.Add(btn);
            }

            form.Controls.Add(startBtn);
            form.ResumeLayout();

            return form;
        }

        /// <summary>
        /// Shuffles the tiles on the board until a solvable arrangement is found.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NewGame(object sender, EventArgs e) {

            do {

                for (int i = 0; i < tiles.Count; i++) {

                    Button button1 = tiles[rnd.Next(i, tiles.Count)];
                    Button button2 = tiles[i];

                    SwapTiles(button1, button2);
                }
            }
            while (!IsPuzzleSolvable());

            for (int i = 0; i < tiles.Count; i++) {
                tiles[i].Enabled = true;
            }

            moveCount = 0;
            startTime = DateTime.Now;
        }

        /// <summary>
        /// If there is a space adjacent to the one clicked that has no button in it, switch these spaces by calling the SwapTiles method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MovePuzzle(object sender, EventArgs e) {

            Button button1 = (Button)sender;
            Tile tile1 = (Tile)button1.Tag;

            Button button2 = tiles.Find(button => ((Tile)button.Tag).IsEmpty);
            Tile tile2 = (Tile)button2.Tag;

            if (tile1.NearestWith(tile2)) {

                SwapTiles(button1, button2);
                moveCount++;
            }

            CheckWin();
        }

        /// <summary>
        /// Checks if every tile is in the right place.
        /// </summary>
        void CheckWin() {

            // find a button so that its Tag is not in the right place. If not found, wrongTile is null
            Button wrongTile = tiles.Find(button => !((Tile)button.Tag).IsRightPlace);

            bool hasWon = (wrongTile == null);

            if (hasWon) {

                for (int i = 0; i < tiles.Count; i++) {

                    tiles[i].Enabled = false;
                }

                TimeSpan timeElapsed = DateTime.Now - startTime;
                timeElapsed = TimeSpan.FromSeconds(Math.Round(timeElapsed.TotalSeconds, 0));

                // plays a rendition of the FFVII victory fanfare (async)
                SoundPlayer sound = new SoundPlayer(@".\ffviiVictoryFanfare.wav");
                sound.Play();

                MessageBox.Show($"Solved in {moveCount} moves. Time elapsed: {timeElapsed}", "You won!");
            }
        }

        /// <summary>
        /// Swaps two given Buttons by doing the good old switcheroo with their tiles
        /// </summary>
        /// <param name="btn1"></param>
        /// <param name="btn2"></param>
        void SwapTiles(Button btn1, Button btn2) {

            if (btn1 == btn2) return;

            Tile tile1 = (Tile)btn1.Tag;
            Tile tile2 = (Tile)btn2.Tag;

            int g = tile1.currentNumber;
            tile1.currentNumber = tile2.currentNumber;
            tile2.currentNumber = g;

            btn1.Visible = true;
            btn1.Text = tile1.ToString();
            if (tile1.IsEmpty) btn1.Visible = false;

            btn2.Visible = true;
            btn2.Text = tile2.ToString();
            if (tile2.IsEmpty) btn2.Visible = false;
        }

        bool IsPuzzleSolvable() {
            // NOTE: puzzle board side MUST be even (so only 2x2, 4x4, 6x6 etc) 
            // Code from: https://www.geeksforgeeks.org/check-instance-15-puzzle-solvable/

            int invCount = 0;

            for (int i = 0; i < tiles.Count - 1; i++) {

                for (int j = i + 1; j < tiles.Count; j++) {

                    Tile tile1 = (Tile)tiles[i].Tag;
                    if (tile1.IsEmpty) continue;

                    Tile tile2 = (Tile)tiles[j].Tag;
                    if (tile2.IsEmpty) continue;

                    if (tile1.currentNumber > tile2.currentNumber) invCount++;
                }
            }

            Button emptyButton = tiles.Find(button => ((Tile)button.Tag).IsEmpty);
            Tile emptyTile = (Tile)emptyButton.Tag;

            if ((emptyTile.InvY + 1) % 2 == 0) {

                if (invCount % 2 != 0) return true;
            }
            else {

                if (invCount % 2 == 0) return true;
            }

            return false;
        }
    }
}
