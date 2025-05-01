using TMPro;
using UnityEngine;

public class CardInfoUI : MonoBehaviour
{
    public static CardInfoUI Instance;
    
    [SerializeField] private TMP_Text descriptionText;

    private Card _draggingCard;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
    }

    private void Start()
    {
        HideStackInfo();
        
        GameTableManager.Instance.CardAddedOnTable += card =>
        {
            card.CardPointerEntered += () => CardPointerEntered(card);
            card.CardPointerExited += CardPointerExited;


            if (card.GetComponent<CardDrag>() is { } cardDrag)
            {
                cardDrag.CardDragStarted += CardDragStarted;
                cardDrag.CardDragEnded += CardDragEnded;
            }
        };
        
        GameTableManager.Instance.CardRemovedFromTable += card =>
        {
            card.CardPointerExited -= CardPointerExited;

            if (card.GetComponent<CardDrag>() is { } cardDrag)
            {
                cardDrag.CardDragStarted -= CardDragStarted;
                cardDrag.CardDragEnded -= CardDragEnded;
            }
        };
    }

    private void CardPointerEntered(Card card)
    {
        if (!_draggingCard)
        {
            ShowStackInfo(card);
        }
    }

    private void CardPointerExited()
    {
        if (!_draggingCard)
        {
            HideStackInfo();
        }
    }
    
    void OnStackModified(Stack stack) => ShowStackInfo(stack.TopCard);
    
    private void CardDragStarted(Card card)
    {
        _draggingCard = card;
        ShowStackInfo(card);
        card.owningStack.OnStackModified += OnStackModified;
    }

    private void CardDragEnded(Card card)
    {
        _draggingCard = null;
        HideStackInfo();
        card.owningStack.OnStackModified -= OnStackModified;
    }

    public void ShowStackInfo(Card card)
    {
        var stack = card.owningStack;
        string output = "";
        foreach (var entry in stack.CardCounts)
        {
            output += $"{entry.Key.description} x {entry.Value} ";
        }
            
        descriptionText.text = output;
    }

    public void HideStackInfo()
    {
        descriptionText.text = "";
    }
}
