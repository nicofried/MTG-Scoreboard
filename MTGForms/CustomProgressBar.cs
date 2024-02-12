using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class CustomProgressBar : ProgressBar
{


    // Define the ProgressBarType enum inside the CustomProgressBar class
    public enum ProgressBarType
    {
        Commander,
        Poison
    }

    // Property to hold the type of the progress bar
    public ProgressBarType BarType { get; set; } = ProgressBarType.Commander;



    public Color StartColor { get; set; }
    public Color EndColor { get; set; }

    public string PlayerName { get; set; }

    public Color TextStartColor { get; set; }

    public CustomProgressBar()
    {
        // Set properties for custom appearance
        SetStyle(ControlStyles.UserPaint, true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        SetStyle(ControlStyles.ResizeRedraw, true);
    }

    protected override void OnMouseHover(EventArgs e)
    {
        base.OnMouseHover(e);

        // Show the player's name in a tooltip when the mouse hovers over the progress bar
        if (!string.IsNullOrEmpty(PlayerName))
        {
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(this, PlayerName);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // Create a graphics object
        Graphics g = e.Graphics;

        // Set the background color to black
        //this.BackColor = Color.DarkGray;

        // Set the background color to a specific shade of dark gray using RGB values
        this.BackColor = Color.FromArgb(30, 30, 30);

        // Create a linear gradient brush
        LinearGradientBrush brush = new LinearGradientBrush(
            new Point(0, 0),
            new Point(Width, 0),
            StartColor,
            EndColor); // Use the custom start and end colors

        // Calculate the width of the progress
        int width = (int)((float)Value / Maximum * Width);

        // Create a rectangle for the progress bar
        Rectangle rect = new Rectangle(0, 0, width, Height);

        // Fill the progress bar with the gradient brush
        g.FillRectangle(brush, rect);

        // Determine the text to be displayed based on the value and bar type
        string text;
        if (Value == 0)
        {
            if (BarType == ProgressBarType.Poison)
            {
                text = "Poison";
            }
            else if (BarType == ProgressBarType.Commander)
            {
                // Display "Commander X" where X is the player number
                text = $"{PlayerName}";
            }
            else
            {
                text = "0"; // Default text if no specific type is set
            }
        }
        else
        {
            text = Value.ToString(); // Display the numeric value
        }

        // Determine the text color based on the progress bar's value
        Color textColor = Value > 0 ? Color.White : TextStartColor;

        // Draw the text
        Font font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold); // Define the font
        Brush textBrush = new SolidBrush(textColor); // Use the determined text color
        SizeF textSize = g.MeasureString(text, font); // Measure the size of the text
        PointF textLocation = new PointF((Width - textSize.Width) / 2, (Height - textSize.Height) / 2); // Center the text
        g.DrawString(text, font, textBrush, textLocation);
    }

}
