using System.Drawing;
using System.Drawing.Drawing2D;

namespace RyzStudio.Drawing
{
    public struct Rectangoid
    {
        int X;
        int Y;
        int Width;
        int Height;
        int Radius;

        public Rectangoid(int x, int y, int width, int height, int radius)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Radius = radius;
        }

        public Rectangoid(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Radius = 0;
        }

        public Rectangoid(int width, int height, int radius)
        {
            X = 0;
            Y = 0;
            Width = width;
            Height = height;
            Radius = radius;
        }

        public Rectangoid(int width, int height)
        {
            X = 0;
            Y = 0;
            Width = width;
            Height = height;
            Radius = 0;
        }

        public Rectangoid(int width)
        {
            X = 0;
            Y = 0;
            Width = width;
            Height = width;
            Radius = 0;
        }
        
        public GraphicsPath ToGraphicsPath()
        {
            GraphicsPath rv = new GraphicsPath();            
            rv.AddLine(X + Radius, Y, X + Width - (Radius * 2), Y);
            rv.AddArc(X + Width - (Radius * 2), Y, Radius * 2, Radius * 2, 270, 90);
            rv.AddLine(X + Width, Y + Radius, X + Width, Y + Height - (Radius * 2));
            rv.AddArc(X + Width - (Radius * 2), Y + Height - (Radius * 2), Radius * 2, Radius * 2, 0, 90);
            rv.AddLine(X + Width - (Radius * 2), Y + Height, X + Radius, Y + Height);
            rv.AddArc(X, Y + Height - (Radius * 2), Radius * 2, Radius * 2, 90, 90);
            rv.AddLine(X, Y + Height - (Radius * 2), X, Y + Radius);
            rv.AddArc(X, Y, Radius * 2, Radius * 2, 180, 90);            
            rv.CloseFigure();
            
            return rv;
        }

        public PointF GetOrigin()
        {
            PointF retval = new PointF();            
            retval.X = ((float)Width / 2) + X;
            retval.Y = ((float)Height / 2) + Y;
            
            return retval;
        }
    }
}