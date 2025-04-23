using UnityEngine;
using UnityEngine.UI;

public class BattleZoneUIController : MonoBehaviour
{
	public RectTransform battleZoneUI;
	public Image background;
	public Image border;
	
	public void ShowZone(Vector3 worldPosition, Vector2 size)
	{
		battleZoneUI.position = worldPosition;
		battleZoneUI.sizeDelta = size;
		
		background.rectTransform.sizeDelta = size;
		border.rectTransform.sizeDelta = size;
	}

	public void HideZone()
	{
		battleZoneUI.gameObject.SetActive(false);
	}
} 

