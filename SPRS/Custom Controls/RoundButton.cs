using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace SPRS
{
    public class RoundButton : Button
    {
        public int CornerRadius { get; set; } = 20; // Default corner radius
        public Color BorderColor { get; set; } = Color.Black; // Default border color
        public int BorderSize { get; set; } = 2; // Default border size

        public RoundButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Define the rounded rectangle path
            using (GraphicsPath path = CreateRoundedRectanglePath(ClientRectangle, CornerRadius))
            {
                // Set the button's region to match the rounded shape
                this.Region = new Region(path);

                // Fill the background
                using (Brush brush = new SolidBrush(BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Draw the border if BorderSize > 0
                if (BorderSize > 0)
                {
                    using (Pen pen = new Pen(BorderColor, BorderSize))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }

            // Draw the text
            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                ClientRectangle,
                ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }

        private GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            // Ensure the rectangle is adjusted for borders
            Rectangle adjustedRect = new Rectangle(
                rect.X + BorderSize / 2,
                rect.Y + BorderSize / 2,
                rect.Width - BorderSize,
                rect.Height - BorderSize
            );

            // Add arcs and lines to create a rounded rectangle
            path.AddArc(adjustedRect.X, adjustedRect.Y, diameter, diameter, 180, 90); // Top-left corner
            path.AddArc(adjustedRect.Right - diameter, adjustedRect.Y, diameter, diameter, 270, 90); // Top-right corner
            path.AddArc(adjustedRect.Right - diameter, adjustedRect.Bottom - diameter, diameter, diameter, 0, 90); // Bottom-right corner
            path.AddArc(adjustedRect.X, adjustedRect.Bottom - diameter, diameter, diameter, 90, 90); // Bottom-left corner
            path.CloseFigure();

            return path;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        private void UpdateRegion()
        {
            // Ensure the button's region matches the rounded corners
            using (GraphicsPath path = CreateRoundedRectanglePath(ClientRectangle, CornerRadius))
            {
                this.Region = new Region(path);
            }
        }
    }
}