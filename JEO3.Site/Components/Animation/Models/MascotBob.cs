namespace JEO3.Site.Components
{
    public enum MascotState { Erupting, Roaming, Dancing }

    public sealed class MascotBob
    {
        public double Width { get; set; } = 30;
        public double Height { get; set; } = 30;
        public double X { get; set; }
        public double Y { get; set; }
        public int Z { get; set; } = 9999;
        public double Rotation { get; set; }
        public int ScaleX { get; set; } = 1; // 1 for right, -1 for left

        // Physics Vectors
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double RotationalVelocity { get; set; }
        public int RND { get; set; } = 1;
        public bool PacmanHimself { get; set; }
        public string ImageSource => ImageSourceOverride.Length > 0 ? ImageSourceOverride : PacmanHimself ? $"img/pacman/pacman.gif" : $"img/pacman/pacman_ghost_{RND}.png";
        public MascotState State { get; set; } = MascotState.Erupting;
        public string ImageSourceOverride { get; set; } = string.Empty;
    }
}
