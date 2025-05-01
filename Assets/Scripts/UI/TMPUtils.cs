using TMPro;
using UnityEngine;

public static class TMPUtils
{

    public static string GetEllipsizedTextWithAutoSize(TMP_Text tmp, string fullText)
    {
        bool    origAuto    = tmp.enableAutoSizing;
        float   maxFont     = tmp.fontSizeMax;
        float   minFont     = tmp.fontSizeMin;
        float   width       = tmp.rectTransform.rect.width;
        float   height      = tmp.rectTransform.rect.height;

        tmp.enableAutoSizing = false;
        tmp.fontSize         = maxFont;
        var sizeMax = tmp.GetPreferredValues(fullText, width, height);
        if (sizeMax.x <= width && sizeMax.y <= height)
        {
            tmp.enableAutoSizing = origAuto;
            return fullText;
        }

        tmp.fontSize = minFont;
        var sizeMin = tmp.GetPreferredValues(fullText, width, height);
        if (sizeMin.x <= width && sizeMin.y <= height)
        {
            tmp.enableAutoSizing = origAuto;
            return fullText;
        }

        int maxChars = GetMaxVisibleCharsAtFont(tmp, fullText, width, height, minFont);
        string clipped = fullText.Substring(0, maxChars) + "…";

        tmp.enableAutoSizing = origAuto;

        return clipped;
    }
    
    private static int GetMaxVisibleCharsAtFont(TMP_Text tmp, string text,
        float maxWidth, float maxHeight, float fontSize)
    {
        tmp.fontSize = fontSize;

        int low = 0, high = text.Length;
        while (low < high)
        {
            int mid = (low + high + 1) / 2;
            string sample = text.Substring(0, mid) + "…";
            Vector2 size = tmp.GetPreferredValues(sample, maxWidth, maxHeight);

            if (size.x <= maxWidth && size.y <= maxHeight)
                low = mid;
            else
                high = mid - 1;
        }
        return low;
    }
}
