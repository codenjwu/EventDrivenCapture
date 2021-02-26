namespace EventDrivenCapture
{
    public partial class CaptureSetting
    {
        public CaptureSetting(int startX, int startY, int width, int height)
        {
            StartX = startX;
            StartY = startY;
            Width = width;
            Height = height;
        }
        public int StartX { get; private set; }
        public int StartY { get; private set; }
        public int EndX { get => StartX + Width; }
        public int EndY { get => StartY + Height; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int ID { get; set; }
        public object Info { get; set; } 
    }
}
