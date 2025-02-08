using UnityEngine;

/// <summary>
/// A utility class for common chess board calculations and validations.
/// </summary>
public static class ChessUtilities
{
    /// <summary>
    /// Converts any given position into a standard board position.
    /// The board is assumed to lie on the X-Y plane with a fixed Z value (here, -1).
    /// </summary>
    public static Vector3 BoardPosition(Vector3 pos)
    {
        return new Vector3(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), -1);
    }

    /// <summary>
    /// Returns true if the given board position lies within a standard 8x8 grid.
    /// </summary>
    public static bool IsWithinBounds(Vector3 pos)
    {
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
    }
}