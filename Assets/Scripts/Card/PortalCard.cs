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
    
    
    [SerializeField] private float sendTimerDuration = 5f;
    
    private bool isEnemyInvaded;
    private Card _card;
    private Timer _timer;
    private List<Card> _cardsToSend = new List<Card>();
    
    private bool HasAllyInEnemyField =>
        GameTableManager.Instance.GetAllCardsInField(GameTableManager.FieldType.EnemyField).Any(c => c.cardData.cardType == CardType.Person);

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

        BattleManager.Instance.BattleFinished += (b) => OnBattleFinished();
        if (EnemyController.Instance)
        {
            EnemyController.Instance.EnemyInvaded += () =>
            {
                isEnemyInvaded = true;
                portalState = PortalState.CannotSend;
            };
        }
    }
    

    private async UniTask ConnectStackEvent()
    {
        await UniTask.WaitUntil(() => _card.owningStack != null);
        _card.owningStack.OnStackModified += OnStackModified;
    }
    
    private void OnBattleFinished()
    {
        if (portalType != PortalType.PlayerField) return;
        if (portalState != PortalState.CannotSend) return;
        
        var canChangeToSend = true;
        if (isEnemyInvaded)
        {
            // Check if all enemies in our field are dead
            var cardsInPlayerField = GameTableManager.Instance.GetAllCardsInField(GameTableManager.FieldType.PlayerField);
            if (cardsInPlayerField.Any(c => c.cardData.cardType == CardType.Enemy))
            {
                canChangeToSend = false;
            }
            else
            {
                isEnemyInvaded = false;
            }
        }
            
        // Check if all allies in portal field are dead
        if (HasAllyInEnemyField)
        {
            canChangeToSend = false;
        }
            
        if (canChangeToSend)
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
                        UIManager.Instance.OpenConfirmMessage(Global.DescriptionMessageText);
                        break;
                    case PortalState.CannotSend:
                        if (HasAllyInEnemyField)
                        {
                            UIManager.Instance.OpenYesOrNoMessage(Global.MoveToEnemyFieldConfirmationText,
                                                                  () => MoveFieldConfirmCallback(GameTableManager.FieldType.EnemyField));
                        }
                        else
                        {
                            UIManager.Instance.OpenConfirmMessage(Global.CannotSendWhileInvadedText);
                        }
                        break;
                }
                break;
            case PortalType.EnemyField:
                UIManager.Instance.OpenYesOrNoMessage(Global.MoveToPlayerFieldConfirmationText,
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
        UIManager.Instance.OpenYesOrNoMessage(Global.SendConfirmationText + "\n" + sendCardsText, SendConfirmCallback);
    }

    private void SendConfirmCallback()
    {
        GameTableManager.MoveCardsToField(GameTableManager.FieldType.EnemyField, _cardsToSend, null);
        portalState = PortalState.CannotSend;
        GameTableManager.Instance.ChangeField(GameTableManager.FieldType.EnemyField);
    }
}
