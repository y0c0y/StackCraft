using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager Instance;
    [SerializeField] private Recipe[] recipes;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        
        foreach (var recipe in recipes)
        {
            recipe.EnsureCacheGenerated();
        }
    }

    public bool CheckRecipe(Stack stack, Recipe recipe)
    {
        if (stack.Length < recipe.TotalInputCount)
        {
            return false;
        }

        var stackCardCounts = stack.CardCounts;
            
        var inputCards = recipe.GetInputCards();
        bool canMatch = true;
            
        foreach (var requirement in inputCards)
        {
            if (!stackCardCounts.TryGetValue(requirement.Key, out var count))
            {
                canMatch = false;
                break;
            }
            if (count < requirement.Value)
            {
                canMatch = false;
                break;
            }
        }

        return canMatch;
    }

    public bool TryFindMatchingRecipe(Stack stack, out Recipe matchedRecipe, out List<Card> consumedCards)
    {
        matchedRecipe = null;
        consumedCards = new List<Card>();

        var stackCardCounts = stack.CardCounts;
        
        foreach (var recipe in recipes)
        {
            if (stack.Length < recipe.TotalInputCount)
            {
                continue;
            }
            
            var inputCards = recipe.GetInputCards();
            Debug.Log($"Checking recipe {recipe.recipeName} with input cards: {string.Join(", ", inputCards)}");
            bool canMatch = true;
            
            foreach (var requirement in inputCards)
            {
                if (!stackCardCounts.TryGetValue(requirement.Key, out var count))
                {
                    canMatch = false;
                    break;
                }
                if (count < requirement.Value)
                {
                    canMatch = false;
                    break;
                }
            }

            if (canMatch)
            {
                matchedRecipe = recipe;
                consumedCards = FindConsumedCards(stack.cards, inputCards);

                if (consumedCards != null && consumedCards.Count == recipe.TotalInputCount)
                {
                    return true;
                }
                else
                {
                    Debug.LogError("incorrect number of consumed cards");
                }
            }
        }

        return false;
    }

    private List<Card> FindConsumedCards(List<Card> cards, Dictionary<CardData, int> inputCards)
    {
        List<Card> consumedCards = new List<Card>();
        // Copy the dictionary first
        Dictionary<CardData, int> needed = new Dictionary<CardData, int>(inputCards);

        foreach (var card in cards)
        {
            if (needed.ContainsKey(card.cardData))
            {
                consumedCards.Add(card);
                needed[card.cardData]--;
                
                if (needed[card.cardData] <= 0)
                {
                    needed.Remove(card.cardData);
                }
            }
            
            if (needed.Count == 0)
            {
                break;
            }
        }
        
        if (needed.Count > 0)
        {
            Debug.LogError("Not all input cards were consumed");
            return null;
        }
        
        return consumedCards;
    }
}
