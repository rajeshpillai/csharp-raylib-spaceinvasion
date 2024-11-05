namespace SpaceInvasionGame.Entity
{
    using Raylib_cs;

    public class Player
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        private int speed;
        public Texture2D Texture { get; private set; }
        private bool moveLeft, moveRight;

        public Player(int x, int y, int speed, Texture2D texture)
        {
            X = x;
            Y = y;
            this.speed = speed;
            Texture = texture;
        }

        public void HandleMovement()
        {
            moveLeft = Raylib.IsKeyDown(KeyboardKey.Left);
            moveRight = Raylib.IsKeyDown(KeyboardKey.Right);
        }

        public void UpdatePosition()
        {
            if (moveLeft)
                X -= speed;
            if (moveRight)
                X += speed;

            if (X < 0) X = 0;
            if (X > 736) X = 736;
        }

        public void Draw()
        {
            Raylib.DrawTexture(Texture, X, Y, Color.White);
        }
    }
}
