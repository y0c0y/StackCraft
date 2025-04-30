using System;
using System.Collections.Generic;

public static class Global
{
    // Portal
    public const string DescriptionMessageText = "이 곳에 병력 카드를 두면 5초 후에 적진으로 이동합니다.";
    public const string SendConfirmationText = "다음의 카드들이 적진으로 이동합니다:";
    public const string CannotSendWhileInvadedText = "침공당할 때에는 적진으로 이동할 수 없습니다.";
    public const string MoveFieldConfirmationText = "본진으로 이동하시겠습니까?";
    
    // Stage
    public const string StageClearMessageText = "승리했습니다!";
    public const string StageFailedMessageText = "패배했습니다...";
    
    
    public static T Random<T> (this List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            return default;
        }
        
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
}
