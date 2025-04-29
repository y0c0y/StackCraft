using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityTimer;

[RequireComponent(typeof(Card))]
public class PortalCard : MonoBehaviour
{
    public enum PortalType
    {
        None,
        PlayerField,
        EnemyField
    }
    
    public enum PortalState
    {
        None,
        CanSend,
        CannotSend
    }

    public PortalType portalType;
    public PortalState portalState;
    
    [SerializeField] private string descriptionMessageText;
    [SerializeField] private string sendConfirmationText;
    [SerializeField] private string moveFieldConfirmationText;
    [SerializeField] private float sendTimerDuration = 5f;
    
    private Card _card;
    private Timer _timer;
    private List<Card> _cardsToSend = new List<Card>();
    
    private void Awake()
    {
        Debug.Assert(portalType != PortalType.None);
        _card = GetComponent<Card>();
        _card.CardClicked += OnCardClicked;
    }

    private void Start()
    {
        portalState = PortalState.CanSend;
        _ = ConnectStackEvent();
        
        BattleManager.Instance.BattleFinished += OnBattleFinished;
    }
    

    private async UniTask ConnectStackEvent()
    {
        await UniTask.WaitUntil(() => _card.owningStack != null);
        _card.owningStack.OnStackModified += OnStackModified;
    }
    
    private void OnBattleFinished()
    {
        if (portalType != PortalType.PlayerField || portalState != PortalState.CannotSend) return;
        
        // Check if all allies in portal field are dead
        var cardsInEnemyField = GameTableManager.Instance.GetAllCardsInField(GameTableManager.FieldType.EnemyField);
        if (cardsInEnemyField.All(c => c.cardData.cardType != CardType.Person))
        {
            portalState = PortalState.CanSend;
        }
    }

    private void OnCardClicked()
    {
        switch (portalType)
        {
            case PortalType.PlayerField:
                switch (portalState)
                {
                    case PortalState.CanSend:
                        UIManager.Instance.OpenConfirmMessage(descriptionMessageText);
                        break;
                    case PortalState.CannotSend:
                        UIManager.Instance.OpenYesOrNoMessage(moveFieldConfirmationText,
                                                              () => MoveFieldConfirmCallback(GameTableManager.FieldType.EnemyField));
                        break;
                }
                break;
            case PortalType.EnemyField:
                UIManager.Instance.OpenYesOrNoMessage(moveFieldConfirmationText,
                                                      () => MoveFieldConfirmCallback(GameTableManager.FieldType.PlayerField));
                break;
        }
    }

    private void MoveFieldConfirmCallback(GameTableManager.FieldType fieldType)
    {
        GameTableManager.Instance.ChangeField(fieldType);
    }

    private void OnStackModified(Stack stack)
    {
        if (portalType == PortalType.EnemyField || portalState == PortalState.CannotSend)
        {
            return;
        }

        if (stack.HasCardType(CardType.Person))
        {
            StartSendTimer();
            _cardsToSend = stack.GetCardTypeInStack(CardType.Person);
        }
        else
        {
            if (_timer != null)
            {
                _timer.Cancel();
                _card.cardTimerUI.gameObject.SetActive(false);
                _timer = null;
            }
        }
    }

    private void StartSendTimer()
    {
        _timer?.Cancel();
        _card.cardTimerUI.gameObject.SetActive(true);
        _timer = this.AttachTimer(
            sendTimerDuration,
            OnSendTimerCompleted,
            UpdateSendTimerUI,
            isLooped: false,
            useRealTime: false);
    }

    private void UpdateSendTimerUI(float f)
    {
        _card.cardTimerUI.SetValue(f / sendTimerDuration);
    }

    private void OnSendTimerCompleted()
    {
        _card.cardTimerUI.gameObject.SetActive(false);
        _timer = null;
        var sendCardsText = string.Join(", ", _cardsToSend.Select(c => c.cardData.cardName));
        UIManager.Instance.OpenYesOrNoMessage(sendConfirmationText + "\n" + sendCardsText, SendConfirmCallback);
    }

    private void SendConfirmCallback()
    {
        GameTableManager.MoveCardsToField(GameTableManager.FieldType.EnemyField, _cardsToSend);
        portalState = PortalState.CannotSend;
        GameTableManager.Instance.ChangeField(GameTableManager.FieldType.EnemyField);
    }
}
