using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangePercentage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        text.text = $"{slider.value} %";
    }

    public void OnValueChanged()
    {
        text.text = $"{slider.value} %";
    }
}
