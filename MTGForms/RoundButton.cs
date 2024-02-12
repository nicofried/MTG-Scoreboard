using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class RoundButton : Button
{
    private int cornerRadius = 30;
    private bool isPressed = false;
    private bool isHovering = false;
    private Color baseColor;
    private Color hoverColor;
    private Color clickColor;

    public int CornerRadius
    {
        get { return cornerRadius; }
        set { cornerRadius = value; Invalidate(); }
    }

    public RoundButton()
    {
        // Set base color
        baseColor = this.BackColor;

        // Calculate hover and click colors based on the base color
        hoverColor = LightenColor(baseColor, 20); // Lighten for hover
        clickColor = DarkenColor(baseColor, 30);  // Darken for click

        // Event handlers
        this.MouseEnter += (sender, e) => { isHovering = true; this.Invalidate(); };
        this.MouseLeave += (sender, e) => { isHovering = false; this.Invalidate(); };
        this.MouseDown += (sender, e) => { isPressed = true; this.Invalidate(); };
        this.MouseUp += (sender, e) => { isPressed = false; this.Invalidate(); };
    }

    private Color LightenColor(Color color, int amount)
    {
        return ChangeColorBrightness(color, amount / 100.0f);
    }

    private Color DarkenColor(Color color, int amount)
    {
        return ChangeColorBrightness(color, -amount / 100.0f);
    }

    private Color ChangeColorBrightness(Color color, float correctionFactor)
    {
        float red = Math.Min(color.R + correctionFactor * 255, 255);
        float green = Math.Min(color.G + correctionFactor * 255, 255);
        float blue = Math.Min(color.B + correctionFactor * 255, 255);
        return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        GraphicsPath grPath = new GraphicsPath();
        Rectangle newRectangle = this.ClientRectangle;

        newRectangle.Inflate(-1, -1);
        grPath.AddArc(newRectangle.X, newRectangle.Y, CornerRadius, CornerRadius, 180, 90);
        grPath.AddArc(newRectangle.X + newRectangle.Width - CornerRadius, newRectangle.Y, CornerRadius, CornerRadius, 270, 90);
        grPath.AddArc(newRectangle.X + newRectangle.Width - CornerRadius, newRectangle.Y + newRectangle.Height - CornerRadius, CornerRadius, CornerRadius, 0, 90);
        grPath.AddArc(newRectangle.X, newRectangle.Y + newRectangle.Height - CornerRadius, CornerRadius, CornerRadius, 90, 90);
        grPath.CloseFigure();

        this.Region = new Region(grPath);
        pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        Color fillColor = baseColor;
        if (isPressed)
        {
            fillColor = clickColor;
        }
        else if (isHovering)
        {
            fillColor = hoverColor;
        }

        Brush brush = new SolidBrush(fillColor);
        pevent.Graphics.FillPath(brush, grPath);

        base.OnPaint(pevent);
    }
}
