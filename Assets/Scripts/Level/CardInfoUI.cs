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
        var cardDataField = typeof(Card).GetField("cardData");
        if (cardDataField == null) return;

        CardData hoveredData = cardDataField.GetValue(card) as CardData;
        if (hoveredData == null) return;
        
        var stackField = typeof(Card).GetField("owningStack"); // 필드명 정확히 확인
        var stack = stackField.GetValue(card) as Stack;

        if (stack != null)
        {
            string output = "";
            foreach (var entry in stack.CardCounts)
            {
                output += $"{entry.Key.description} x {entry.Value} ";
            }
            
            descriptionText.text = output;
            return;
        }

        descriptionText.text = "Stack 없음";
    }

}
