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
        
        // For low difficulty, pick a random move.
        MoveCandidate selectedCandidate;
        if (difficultyLevel <= 3)
        {
            int index = Random.Range(0, candidates.Count);
            selectedCandidate = candidates[index];
        }
        else
        {
            // For higher difficulty, use minimax search.
            selectedCandidate = SelectCandidate(candidates, difficultyLevel);
        }
        
        // Execute the move using the GameManager's AI move method.
        gameManager.ExecuteAIMove(selectedCandidate.Piece, selectedCandidate.Target);
    }

    private MoveCandidate SelectCandidate(List<MoveCandidate> candidates, int difficultyLevel)
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

    // Uses minimax to evaluate the move.
    private float EvaluateMove(MoveCandidate candidate, int difficultyLevel)
    {
        // Simulate candidate move.
        MoveRecord record = AIUtility.MakeMove(candidate.Piece, candidate.Target);
        // Use minimax search with a depth based on difficulty.
        // Depth could be set, for example, to difficultyLevel (or a function thereof).
        float score = AIUtility.Minimax(difficultyLevel, float.NegativeInfinity, float.PositiveInfinity, false, AiPlaysAsWhite);
        AIUtility.UndoMove(record);
        return score;
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