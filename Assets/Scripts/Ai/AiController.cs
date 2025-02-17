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
        // Use AIUtility to get all legal moves for AI's color.
        List<MoveCandidate> candidates = AIUtility.GetAllLegalMoves(AiPlaysAsWhite);
        if (candidates.Count == 0)
        {
            Debug.Log("AI has no legal moves.");
            return;
        }

        MoveCandidate selectedCandidate;
        if (difficultyLevel <= 3)
        {
            int index = Random.Range(0, candidates.Count);
            selectedCandidate = candidates[index];
        }
        else
        {
            selectedCandidate = SelectCandidate(candidates, difficultyLevel);
        }
        
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

   private float EvaluateMove(MoveCandidate candidate, int difficultyLevel)
{
    // Record the start time for this minimax search.
    float startTime = Time.realtimeSinceStartup;

    // Simulate candidate move.
    MoveRecord record = AIUtility.MakeMove(candidate.Piece, candidate.Target);
    // Use minimax search with a depth based on difficulty, passing startTime.
    float score = AIUtility.Minimax(difficultyLevel, float.NegativeInfinity, float.PositiveInfinity, false, AiPlaysAsWhite, startTime);
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
