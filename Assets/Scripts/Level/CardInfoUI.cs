using TMPro;
using UnityEngine;

public class CardInfoUI : MonoBehaviour
{
    public static CardInfoUI Instance;
    
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private LayerMask cardLayerMask;

    private Card _lastHoveredCard;

    void Update()
    {
        DetectCardUnderMouse();
    }


    void DetectCardUnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, cardLayerMask);

        if (hit.collider != null)
        {
            var card = hit.collider.GetComponent<Card>();
            if (card != null)
            {
                ShowStackInfo(card);
            }
        }
        else
        {
            descriptionText.text = "";
        }
    }

    void ShowStackInfo(Card card)
    {
        var stack = card.owningStack;
        string output = "";
        foreach (var entry in stack.CardCounts)
        {
            output += $"{entry.Key.description} x {entry.Value} \n";
        }
            
        descriptionText.text = output;
    }

}
