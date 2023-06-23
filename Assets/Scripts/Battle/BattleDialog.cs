using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialog : MonoBehaviour
{
    [SerializeField] int lettersPerSecond;

    [SerializeField] Text dialogText;
    [SerializeField] Image dialogBackGround;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject choiceBox;

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> moveTexts;
    [SerializeField] List<Image> Type_Cover;
    [SerializeField] List<Image> Corner_Cover;
    [SerializeField] List<Image> MoveImage;
    [SerializeField] List<Image> ActionImage;
    [SerializeField] List<Image> ActionIcon;

    [SerializeField] ColorMap colorMap;

    [SerializeField] List<Text> ppText;
    [SerializeField] List<Text> powerText;

    [SerializeField] Text yesText;
    [SerializeField] Text noText;
    [SerializeField] Image yesImage;
    [SerializeField] Image NoImage;

    Color highlightedColor;

    private void Start()
    {
        highlightedColor = GlobalSettings.i.HighlightedColor;
    }

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    public IEnumerator TypeDialog(string dialog)
    {
        EnableDialogText(true);
        GetComponent<Image>().enabled = true;
        dialogBackGround.enabled = true;

        EnableActionSelector(false);

        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        yield return new WaitForSeconds(1f);

        EnableDialogText(false);
        GetComponent<Image>().enabled = false;
        dialogBackGround.enabled = false;
    }

    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
    }

    public void EnableChoiceBox(bool enabled)
    {
        choiceBox.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < actionTexts.Count; ++i)
        {
            if (i == selectedAction)
            {
                actionTexts[i].color = Color.white;
                ActionImage[i].color = Color.black;

            }
            else
            {
                actionTexts[i].color = Color.black;
                ActionImage[i].color = Color.white;
            }
        }
    }

    public void UpdateMoveSelection(int selectedMove, Moves move)
    {
        for (int i = 0; i < moveTexts.Count; ++i)
        {
            if (i == selectedMove)
            {
                ppText[i].color = Color.white;
                powerText[i].color = Color.white;
                MoveImage[i].color = Color.black;
                Corner_Cover[i].color = Color.black;
            }
            else
            {
                ppText[i].color = Color.black;
                powerText[i].color = Color.black;
                MoveImage[i].color = new Color(0.8980393f, 0.8980393f, 0.8980393f);
                Corner_Cover[i].color = new Color(0.854902f, 0.854902f, 0.882353f);
            }
        }
    }

    public void SetMoveNames(List<Moves> moves)
    {
        for (int i = 0; i < moveTexts.Count; ++i)
        {
            if (i < moves.Count)
            {
                moveTexts[i].text = moves[i].Base.Name;

                ppText[i].text = $"{moves[i].PP}/{moves[i].Base.PP}";
                powerText[i].text = $"PW {moves[i].Power}";

                Type_Cover[i].color = colorMap.GetColorByType(moves[i].Base.Type);

                if (moves[i].Base.Type != DeityMonsType.Normal && moves[i].Base.Type != DeityMonsType.None)
                {
                    moveTexts[i].color = Color.white;
                }
                else
                {
                    moveTexts[i].color = Color.black;
                }
            }
            else
            {
                moveTexts[i].text = "-";
            }
        }
    }

    public void UpdateChoiceBox(bool yesSelected)
    {
        if (yesSelected)
        {
            yesText.color = Color.white;
            yesImage.color = Color.black;
            noText.color = Color.black;
            NoImage.color = Color.white;
        }
        else
        {
            yesText.color = Color.black;
            yesImage.color = Color.white;
            noText.color = Color.white;
            NoImage.color = Color.black;
        }
    }
}
