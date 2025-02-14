using UnityEngine;
using UnityEngine.UI;

public class PromotionUIHandler : MonoBehaviour
{
    [Header("Promotion Option Buttons")]
    public Button queenButton;
    public Button rookButton;
    public Button knightButton;
    public Button bishopButton;

    [Header("White Piece Sprites")]
    public Sprite whiteQueenSprite;
    public Sprite whiteRookSprite;
    public Sprite whiteKnightSprite;
    public Sprite whiteBishopSprite;

    [Header("Black Piece Sprites")]
    public Sprite blackQueenSprite;
    public Sprite blackRookSprite;
    public Sprite blackKnightSprite;
    public Sprite blackBishopSprite;

     // Reference to the currently active PromotionHandler (set when promotion is triggered)
    public PromotionHandler currentPromotionHandler;

    /// <summary>
    /// Configures the promotion UI so that the buttons display the correct piece sprites
    /// based on whether the promoting pawn is white or black.
    /// </summary>
    /// <param name="isWhite">True if the promoting pawn is white; false if black.</param>
    public void SetPromotionUI(bool isWhite, PromotionHandler promotionHandler)
    {
        currentPromotionHandler = promotionHandler;
        if (isWhite)
        {
            if (queenButton != null) queenButton.image.sprite = whiteQueenSprite;
            if (rookButton != null) rookButton.image.sprite = whiteRookSprite;
            if (knightButton != null) knightButton.image.sprite = whiteKnightSprite;
            if (bishopButton != null) bishopButton.image.sprite = whiteBishopSprite;
        }
        else
        {
            if (queenButton != null) queenButton.image.sprite = blackQueenSprite;
            if (rookButton != null) rookButton.image.sprite = blackRookSprite;
            if (knightButton != null) knightButton.image.sprite = blackKnightSprite;
            if (bishopButton != null) bishopButton.image.sprite = blackBishopSprite;
        }

        queenButton.onClick.AddListener(this.OnQueenButtonClicked);
        rookButton.onClick.AddListener(this.OnRookButtonClicked);
        knightButton.onClick.AddListener(this.OnKnightButtonClicked);
        bishopButton.onClick.AddListener(this.OnBishopButtonClicked); 
    }

     // These methods are hooked to the button OnClick events via the Inspector.

    public void OnQueenButtonClicked()
    {
        if (currentPromotionHandler != null)
        {
            currentPromotionHandler.PromoteToQueen();
        }
    }

    public void OnRookButtonClicked()
    {
        if (currentPromotionHandler != null)
        {
            currentPromotionHandler.PromoteToRook();
        }
    }

    public void OnKnightButtonClicked()
    {
        if (currentPromotionHandler != null)
        {
            currentPromotionHandler.PromoteToKnight();
        }
    }

    public void OnBishopButtonClicked()
    {
        if (currentPromotionHandler != null)
        {
            currentPromotionHandler.PromoteToBishop();
        }
    }
}
