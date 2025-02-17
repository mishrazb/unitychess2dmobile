using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    // Difficulty level from 1 (easiest) to 10 (hardest)
    public int difficultyLevel;
    // Set whether the AI is playing as White (true) or Black (false)
    public bool AiPlaysAsWhite = false;
    // Reference to GameManager for triggering moves
    public GameManager gameManager;
    public static AIController Instance;

    private void Awake()
    {
        Instance = this;
        difficultyLevel = PlayerPrefs.GetInt("DifficultyLevel", 1);
    }

    // Called when it's the AI's turn.
    public void MakeMove()
    {
        // Use the AIUtility to get all legal moves for the AI's color.
        List<MoveCandidate> candidates = AIUtility.GetAllLegalMoves(AiPlaysAsWhite);

        if (candidates.Count == 0)
        {
            Debug.Log("AI has no legal moves.");
            return;
        }

        // Select a candidate move based on difficulty.
        MoveCandidate selectedCandidate = SelectCandidate(candidates, difficultyLevel);

        // Execute the move using the GameManager's AI move method.
        gameManager.ExecuteAIMove(selectedCandidate.Piece, selectedCandidate.Target);
    }

    // Selects a candidate move. For low difficulty levels, pick a random move;
    // for higher difficulty, evaluate moves (you can expand this with a minimax search later).
    private MoveCandidate SelectCandidate(List<MoveCandidate> candidates, int difficultyLevel)
    {
        if (difficultyLevel <= 3)
        {
            int index = Random.Range(0, candidates.Count);
            return candidates[index];
        }
        else
        {
            MoveCandidate bestCandidate = null;
            float bestScore = float.NegativeInfinity;
            foreach (MoveCandidate candidate in candidates)
            {
                float score = EvaluateMove(candidate, difficultyLevel);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestCandidate = candidate;
                }
            }
            return bestCandidate;
        }
    }

    // A simple evaluation function that scores moves.
    // This is a placeholder; you could later replace it with a minimax evaluation.
    private float EvaluateMove(MoveCandidate candidate, int difficultyLevel)
    {
        float baseScore = Random.Range(0f, 10f);
        return baseScore + difficultyLevel * 0.5f;
    }
}

public class MoveCandidate
{
    public PieceController Piece;
    public Vector3 Target;

    public MoveCandidate(PieceController piece, Vector3 target)
    {
        Piece = piece;
        Target = target;
    }
}
