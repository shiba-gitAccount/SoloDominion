using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public enum SupplyMode
{
    InitialGame,
    FlexibleSize,
    DeckTop,
    HandTechnique,
    Improvement,
    GoldAndSilver,
}

public class TitleManager : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private List<Button> Buttons;

    SupplyMode supply;

    void Start()
    {
        startButton.onClick.AddListener(() =>
        {
            OnClickStart();
        });
        for (int i = 0; i < 6; i++)
        {
            int index = i;
            Buttons[index].onClick.AddListener(() =>
            {
                ColorReset();
                Buttons[index].GetComponent<Image>().color = new Color32(0, 197, 73, 255);
                switch(index)
                {
                    case 0:
                        supply = SupplyMode.InitialGame;
                        break;
                    case 1:
                        supply = SupplyMode.FlexibleSize;
                        break;
                    case 2:
                        supply = SupplyMode.DeckTop;
                        break;
                    case 3:
                        supply = SupplyMode.HandTechnique;
                        break;
                    case 4:
                        supply = SupplyMode.Improvement;
                        break;
                    case 5:
                        supply = SupplyMode.GoldAndSilver;
                        break;
                }
            });
        }
    }

    public void OnClickStart()
    {
        GameSettings.Instance.selectedSupply = this.supply;
        SceneManager.LoadScene("GameScene");
    }

    void ColorReset()
    {
        foreach (Button button in Buttons)
        {
            button.GetComponent<Image>().color = Color.white;
        }
    }
}
