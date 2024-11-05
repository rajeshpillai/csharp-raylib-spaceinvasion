namespace SpaceInvasionGame.Entity
{
    using Raylib_cs;

    public abstract class GameEntity
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Texture2D Texture { get; protected set; }

        public GameEntity(int x, int y, Texture2D texture)
        {
            X = x;
            Y = y;
            Texture = texture;
        }

        // Abstract method for moving, to be implemented by derived classes
        public abstract void Move();

        // Draws the entity on the screen
        public virtual void Draw()
        {
            Raylib.DrawTexture(Texture, X, Y, Color.White);
        }
    }
}
