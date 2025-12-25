using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BattleOfLegends;

public class Grid
{
    private readonly int numberOfRows;
    private readonly int numberOfColumns;
    private readonly float hexHeight;

    public Grid(int numberOfRows, int numberOfColumns, float hexHeight)
    {
        this.numberOfRows = numberOfRows;
        this.numberOfColumns = numberOfColumns;
        this.hexHeight = hexHeight;
    }

    private float HexWidth()
    {
        return (float)(hexHeight * Math.Sqrt(3) / 2);
    }

    // Draw a hexagonal grid
    public void DrawHexGrid(SpriteBatch spriteBatch, Texture2D pixel, Color color,
        float xmin, float xmax, float ymin, float ymax, SpriteFont font = null)
    {
        // Loop until a hexagon won't fit.
        for (int row = 0; row < numberOfRows; row++)
        {
            // Get the points for the row's first hexagon.
            Vector2[] points = HexToPoints(row, 0);

            // If it doesn't fit, we're done.
            if (points[4].Y > ymax) break;

            // Draw the row.
            for (int col = 0; col < numberOfColumns; col++)
            {
                // Get the points for the row's next hexagon.
                points = HexToPoints(row, col);

                // If it doesn't fit horizontally, we're done with this row.
                if (points[3].X > xmax) break;

                // Remove last hexagons in odd rows.
                if (row % 2 != 0 && col == numberOfColumns - 1) break;

                // If it fits vertically, draw it.
                if (points[4].Y <= ymax)
                {
                    DrawPolygon(spriteBatch, pixel, points, color);
                }

                // Label the hexagon (if font provided)
                if (font != null)
                {
                    float x = (points[0].X + points[3].X) / 2;
                    float y = (points[1].Y + points[4].Y) / 2;
                    string label = $"({row}, {col})";
                    Vector2 labelSize = font.MeasureString(label);
                    Vector2 labelPos = new Vector2(x - labelSize.X / 2, y - labelSize.Y / 2);
                    spriteBatch.DrawString(font, label, labelPos, Color.DarkGray);
                }
            }
        }
    }

    // Return the points that define the indicated hexagon.
    public Vector2[] HexToPoints(float row, float col)
    {
        // Start with the leftmost upper corner of the upper left hexagon.
        float width = HexWidth();
        float y = hexHeight / 4;
        float x = 0;

        // Move right the required number of columns.
        x += col * width;

        // If the row is odd, move right half a hex more.
        if (row % 2 != 0) x += width / 2;

        // Move over for the row number.
        y += row * (hexHeight * 0.75f);

        // Generate the points.
        return new Vector2[]
        {
            new Vector2(x, y),
            new Vector2(x + width / 2, y - hexHeight * 0.25f),
            new Vector2(x + width, y),
            new Vector2(x + width, y + hexHeight * 0.50f),
            new Vector2(x + width / 2, y + hexHeight * 0.75f),
            new Vector2(x, y + hexHeight * 0.50f),
        };
    }

    public Point HexToPoint(float row, float col)
    {
        // Start with the leftmost upper corner of the upper left hexagon.
        float width = HexWidth();
        float y = hexHeight / 4;
        float x = 0;

        // Move right the required number of columns.
        x += col * width;

        // If the row is odd, move right half a hex more.
        if (row % 2 != 0) x += width / 2;

        // Move over for the row number.
        y += row * (hexHeight * 0.75f);

        // Generate the point.
        return new Point((int)x + 2, (int)y + 1);
    }

    // Return the row and column of the hexagon at this point.
    public void PointToHex(float x, float y, out int row, out int col)
    {
        // Find the test rectangle containing the point.
        float width = HexWidth();

        row = (int)(y / (hexHeight * 0.75f));

        if (row % 2 == 0)
            col = (int)Math.Floor(x / width);
        else
            col = (int)Math.Floor((x - width / 2) / width);

        // Find the test area.
        float testx = col * width;
        float testy = row * hexHeight * 0.75f;
        if (row % 2 != 0) testx += width / 2;

        // See if the point is above or below the test hexagon on the left.
        bool is_right = false, is_left = false;
        float dy = y - testy;
        if (dy < hexHeight / 4)
        {
            float dx = x - (testx + width / 2);
            if (dy < 0.001)
            {
                // The point is on the left edge of the test rectangle.
                if (dx < 0) is_left = true;
                if (dx > 0) is_right = true;
            }
            else if (dx < 0)
            {
                // See if the point is above the test hexagon.
                if (-dx / dy > Math.Sqrt(3)) is_left = true;
            }
            else
            {
                // See if the point is below the test hexagon.
                if (dx / dy > Math.Sqrt(3)) is_right = true;
            }
        }

        // Adjust the row and column if necessary.
        if (is_left)
        {
            if (row % 2 == 0) col--;
            row--;
        }
        else if (is_right)
        {
            if (row % 2 != 0) col++;
            row--;
        }
    }

    public Point NodeToPoint(float row, float col)
    {
        // Start with the leftmost upper corner of the upper left hexagon.
        float width = HexWidth();
        float y = hexHeight / 2;
        float x = width / 2;

        // Move right the required number of columns.
        x += col * width;

        // If the row is odd, move right half a hex more.
        if (row % 2 != 0) x += width / 2;

        // Move over for the row number.
        y += row * (hexHeight * 0.75f);

        // Generate the point.
        return new Point((int)x, (int)y);
    }

    // Helper method to draw a polygon using a line texture
    private void DrawPolygon(SpriteBatch spriteBatch, Texture2D pixel, Vector2[] points, Color color)
    {
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 point1 = points[i];
            Vector2 point2 = points[(i + 1) % points.Length];
            DrawLine(spriteBatch, pixel, point1, point2, color, 1f);
        }
    }

    // Helper method to draw a line
    private void DrawLine(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 end, Color color, float thickness = 1f)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);
        float length = edge.Length();

        spriteBatch.Draw(pixel, start, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0);
    }
}
