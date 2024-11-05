namespace SpaceInvasionGame.Entity
{
    using Raylib_cs;

    public class Bullet
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        private int speedY = 6;
        public Texture2D Texture { get; private set; }

        public Bullet(int x, int y, Texture2D texture)
        {
            X = x;
            Y = y;
            Texture = texture;
        }

        public void Move()
        {
            Y -= speedY;
        }

        public void Draw()
        {
            Raylib.DrawTexture(Texture, X, Y, Color.White);
        }
    }
}
