using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Classic_Snakes_Game_Tutorial___MOO_ICT
{
    //################################################################################
    // ENUMS - A much safer way to handle directions and game states than strings!
    //################################################################################

    /// <summary>
    /// Represents the possible directions the snake can move.
    /// </summary>
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    /// Represents the current state of the game.
    /// </summary>
    public enum GameState
    {
        Playing,
        GameOver
    }

    //################################################################################
    // DATA CLASSES - Simple classes to hold data.
    //################################################################################

    /// <summary>
    /// A simple class to store X and Y coordinates.
    /// Using a class is cleaner than the old 'Circle' name.
    /// </summary>
    public class GamePiece
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    /// <summary>
    /// Static class to hold game settings.
    /// </summary>
    public static class Settings
    {
        public static int PieceWidth { get; } = 16;
        public static int PieceHeight { get; } = 16;
        public static int InitialSpeed { get; } = 150; // Milliseconds
        public static int SpeedIncrement { get; } = 5;  // Milliseconds to reduce
        public static int MinSpeed { get; } = 40;     // Max speed
    }

    //################################################################################
    // LOGIC CLASSES - These classes now manage the game's logic.
    //################################################################################

    /// <summary>
    /// Manages all logic and drawing for the Snake.
    /// </summary>
    public class Snake
    {
        public List<GamePiece> Body { get; private set; }
        public Direction CurrentDirection { get; set; }

        public Snake()
        {
            // Initialize the snake with a head and a few body parts
            Body = new List<GamePiece>();
            CurrentDirection = Direction.Right; // Start moving right

            // Create a head
            Body.Add(new GamePiece { X = 10, Y = 5 });
            // Add initial body parts
            for (int i = 0; i < 5; i++)
            {
                Body.Add(new GamePiece { X = Body[Body.Count - 1].X - 1, Y = 5 });
            }
        }

        public void Move()
        {
            // Move the body
            for (int i = Body.Count - 1; i >= 1; i--)
            {
                Body[i].X = Body[i - 1].X;
                Body[i].Y = Body[i - 1].Y;
            }

            // Move the head
            GamePiece head = Body[0];
            switch (CurrentDirection)
            {
                case Direction.Left:  head.X--; break;
                case Direction.Right: head.X++; break;
                case Direction.Up:    head.Y--; break;
                case Direction.Down:  head.Y++; break;
            }
        }

        public void Grow()
        {
            // Add a new piece to the end of the snake
            GamePiece tail = Body[Body.Count - 1];
            Body.Add(new GamePiece { X = tail.X, Y = tail.Y });
        }

        public bool CheckSelfCollision()
        {
            GamePiece head = Body[0];
            for (int i = 1; i < Body.Count; i++)
            {
                if (head.X == Body[i].X && head.Y == Body[i].Y)
                {
                    return true;
                }
            }
            return false;
        }

        public void Draw(Graphics canvas)
        {
            Brush snakeColour;
            for (int i = 0; i < Body.Count; i++)
            {
                // Head is black, body is green
                snakeColour = (i == 0) ? Brushes.Black : Brushes.DarkGreen;
                canvas.FillEllipse(snakeColour, new Rectangle(
                    Body[i].X * Settings.PieceWidth,
                    Body[i].Y * Settings.PieceHeight,
                    Settings.PieceWidth, Settings.PieceHeight));
            }
        }
    }

    /// <summary>
    /// Manages the food's position and drawing.
    /// </summary>
    public class Food
    {
        public GamePiece Position { get; private set; }
        private Random rand = new Random();

        public Food()
        {
            Position = new GamePiece();
        }

        public void Generate(int maxWidth, int maxHeight, List<GamePiece> snakeBody)
        {
            bool invalidPosition;
            do
            {
                // Get a new random position
                Position.X = rand.Next(2, maxWidth);
                Position.Y = rand.Next(2, maxHeight);

                // Check if the new position is on the snake
                invalidPosition = false;
                foreach (var piece in snakeBody)
                {
                    if (piece.X == Position.X && piece.Y == Position.Y)
                    {
                        invalidPosition = true;
                        break;
                    }
                }
            } while (invalidPosition); // Keep trying until we find a valid spot
        }

        public void Draw(Graphics canvas)
        {
            canvas.FillEllipse(Brushes.DarkRed, new Rectangle(
                Position.X * Settings.PieceWidth,
                Position.Y * Settings.PieceHeight,
                Settings.PieceWidth, Settings.PieceHeight));
        }
    }

    /// <summary>
    /// The main Game Engine. This class controls the game loop,
    /// state, score, and all game objects.
    /// </summary>
    public class GameEngine
    {
        public GameState CurrentState { get; private set; }
        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public int CurrentSpeed { get; private set; }

        private Snake snake;
        private Food food;
        private int maxWidth;
        private int maxHeight;

        // Flags for pending direction change
        private bool goLeft, goRight, goDown, goUp;

        public GameEngine(int picBoxWidth, int picBoxHeight)
        {
            maxWidth = picBoxWidth / Settings.PieceWidth - 1;
            maxHeight = picBoxHeight / Settings.PieceHeight - 1;
            CurrentState = GameState.GameOver; // Start in GameOver state
        }

        public void StartGame()
        {
            Score = 0;
            CurrentSpeed = Settings.InitialSpeed;
            goLeft = goRight = goDown = goUp = false;

            snake = new Snake();
            food = new Food();
            food.Generate(maxWidth, maxHeight, snake.Body);

            CurrentState = GameState.Playing;
        }

        /// <summary>
        /// This is the main game loop, called by the timer.
        /// </summary>
        public void Update()
        {
            if (CurrentState != GameState.Playing)
            {
                return; // Do nothing if game isn't running
            }

            // Update direction based on pending input
            if (goLeft && snake.CurrentDirection != Direction.Right) snake.CurrentDirection = Direction.Left;
            if (goRight && snake.CurrentDirection != Direction.Left) snake.CurrentDirection = Direction.Right;
            if (goUp && snake.CurrentDirection != Direction.Down) snake.CurrentDirection = Direction.Up;
            if (goDown && snake.CurrentDirection != Direction.Up) snake.CurrentDirection = Direction.Down;

            // Reset flags
            goLeft = goRight = goDown = goUp = false;

            // Move the snake
            snake.Move();

            // Check for wall collision
            GamePiece head = snake.Body[0];
            if (head.X < 0 || head.X > maxWidth || head.Y < 0 || head.Y > maxHeight)
            {
                SetGameOver();
                return;
            }

            // Check for self collision
            if (snake.CheckSelfCollision())
            {
                SetGameOver();
                return;
            }

            // Check for food collision
            if (head.X == food.Position.X && head.Y == food.Position.Y)
            {
                EatFood();
            }
        }

        private void EatFood()
        {
            Score += 1;
            snake.Grow();
            food.Generate(maxWidth, maxHeight, snake.Body);

            // Increase speed
            if (CurrentSpeed > Settings.MinSpeed)
            {
                CurrentSpeed -= Settings.SpeedIncrement;
            }
        }

        private void SetGameOver()
        {
            CurrentState = GameState.GameOver;
            if (Score > HighScore)
            {
                HighScore = Score;
            }
        }

        /// <summary>
        /// Handles user input.
        /// </summary>
        public void KeyDown(Keys key)
        {
            // Set flags. Logic is checked in the Update() loop
            // This prevents changing direction multiple times between frames
            if (key == Keys.Left) goLeft = true;
            if (key == Keys.Right) goRight = true;
            if (key == Keys.Up) goUp = true;
            if (key == Keys.Down) goDown = true;
        }

        /// <summary>
        /// Main drawing method, called by the PictureBox's Paint event.
        /// </summary>
        public void Draw(Graphics canvas)
        {
            // Draw all game elements
            snake?.Draw(canvas);
            food?.Draw(canvas);

            // If game is over, draw the "Game Over" message
            if (CurrentState == GameState.GameOver)
            {
                string gameOverText = "Game Over!\nScore: " + Score + "\nPress 'Start' to play again";
                Font font = new Font("Arial", 16, FontStyle.Bold);
                SizeF textSize = canvas.MeasureString(gameOverText, font);
                
                // Center the text
                PointF textLocation = new PointF(
                    (maxWidth * Settings.PieceWidth - textSize.Width) / 2,
                    (maxHeight * Settings.PieceHeight - textSize.Height) / 2
                );

                // Draw a semi-transparent background for readability
                canvas.FillRectangle(new SolidBrush(Color.FromArgb(200, 255, 255, 255)),
                    textLocation.X - 10, textLocation.Y - 10,
                    textSize.Width + 20, textSize.Height + 20);
                
                // Draw the text
                canvas.DrawString(gameOverText, font, Brushes.Red, textLocation);
            }
        }
    }

    //################################################################################
    // FORM1 - The "View". It only handles UI events and tells the engine what to do.
    //################################################################################
    public partial class Form1 : Form
    {
        private GameEngine game;

        public Form1()
        {
            InitializeComponent();
            // We create the game engine, but don't start it yet.
            game = new GameEngine(picCanvas.Width, picCanvas.Height);
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            // Pass the key press to the game engine
            game.KeyDown(e.KeyCode);
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            // We don't need this anymore, the engine handles input logic
        }

        private void StartGame(object sender, EventArgs e)
        {
            game.StartGame();
            gameTimer.Interval = game.CurrentSpeed;
            gameTimer.Start();

            // Update UI
            startButton.Enabled = false;
            snapButton.Enabled = false;
            txtScore.Text = "Score: 0";
            txtHighScore.Text = "High Score: \n" + game.HighScore;
        }

        private void TakeSnapShot(object sender, EventArgs e)
        {
            Label caption = new Label();
            caption.Text = "I scored: " + game.Score + " and my Highscore is " + game.HighScore;
            caption.Font = new Font("Ariel", 12, FontStyle.Bold);
            caption.ForeColor = Color.Purple;
            caption.AutoSize = false;
            caption.Width = picCanvas.Width;
            caption.Height = 30;
            caption.TextAlign = ContentAlignment.MiddleCenter;
            picCanvas.Controls.Add(caption);

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "Snake Game SnapShot";
            dialog.DefaultExt = "jpg";
            dialog.Filter = "JPG Image File | *.jpg";
            dialog.ValidateNames = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int width = Convert.ToInt32(picCanvas.Width);
                int height = Convert.ToInt3T(picCanvas.Height);
                Bitmap bmp = new Bitmap(width, height);
                picCanvas.DrawToBitmap(bmp, new Rectangle(0, 0, width, height));
                bmp.Save(dialog.FileName, ImageFormat.Jpeg);
                picCanvas.Controls.Remove(caption);
            }
        }

        private void GameTimerEvent(object sender, EventArgs e)
        {
            // 1. Tell the engine to update its logic
            game.Update();

            // 2. Check the game state from the engine
            if (game.CurrentState == GameState.Playing)
            {
                // Update score and speed
                txtScore.Text = "Score: " + game.Score;
                if (gameTimer.Interval != game.CurrentSpeed)
                {
                    gameTimer.Interval = game.CurrentSpeed;
                }
            }
            else
            {
                // Game must be over
                GameOver();
            }

            // 3. Redraw the screen
            picCanvas.Invalidate();
        }

        private void UpdatePictureBoxGraphics(object sender, PaintEventArgs e)
        {
            // Just tell the game engine to draw itself
            game.Draw(e.Graphics);
        }

        private void GameOver()
        {
            gameTimer.Stop();
            startButton.Enabled = true;
            snapButton.Enabled = true;

            if (game.Score > game.HighScore)
            {
                txtHighScore.Text = "High Score: \n" + game.HighScore;
                txtHighScore.ForeColor = Color.Maroon;
                txtHighScore.TextAlign = ContentAlignment.MiddleCenter;
            }
        }
    }
}
