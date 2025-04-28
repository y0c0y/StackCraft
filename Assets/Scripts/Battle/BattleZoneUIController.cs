using UnityEngine;
using UnityEngine.UI;

public class BattleZoneUIController : MonoBehaviour
{
	public RectTransform battleZoneUI;

	public void HideZone()
	{
		battleZoneUI.gameObject.SetActive(false);
	}
} 

