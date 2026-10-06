using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace CardGames.Managers { 
public class BuyCoinsController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button button500;
    [SerializeField] private Button button1000;
    [SerializeField] private Button button10000;
    [SerializeField] private Button buttonBack;

    [Header("Scene")]
    [SerializeField] private string mainSceneName = "CartGamesFirst";


    private void Awake()
    {
        FindButtons();
        SetupButtons();
    }


    // ==================================================
    // Find Buttons
    // ==================================================

    private void FindButtons()
    {
        if (button500 == null)
        {
            GameObject obj = GameObject.Find("Button_500");

            if (obj != null)
                button500 = obj.GetComponent<Button>();
        }


        if (button1000 == null)
        {
            GameObject obj = GameObject.Find("Button_1000");

            if (obj != null)
                button1000 = obj.GetComponent<Button>();
        }


        if (button10000 == null)
        {
            GameObject obj = GameObject.Find("Button_10000");

            if (obj != null)
                button10000 = obj.GetComponent<Button>();
        }


        if (buttonBack == null)
        {
            GameObject obj = GameObject.Find("Button_Back");

            if (obj != null)
                buttonBack = obj.GetComponent<Button>();
        }
    }


    // ==================================================
    // Setup
    // ==================================================

    private void SetupButtons()
    {
        if (button500 != null)
        {
            button500.onClick.RemoveListener(Add500Coins);
            button500.onClick.AddListener(Add500Coins);
        }


        if (button1000 != null)
        {
            button1000.onClick.RemoveListener(Add1000Coins);
            button1000.onClick.AddListener(Add1000Coins);
        }


        if (button10000 != null)
        {
            button10000.onClick.RemoveListener(Add10000Coins);
            button10000.onClick.AddListener(Add10000Coins);
        }


        if (buttonBack != null)
        {
            buttonBack.onClick.RemoveListener(BackToMainScene);
            buttonBack.onClick.AddListener(BackToMainScene);
        }
    }


    // ==================================================
    // 500 Coins
    // ==================================================

    private void Add500Coins()
    {
        AddCoins(500);
    }


    // ==================================================
    // 1000 Coins
    // ==================================================

    private void Add1000Coins()
    {
        AddCoins(1000);
    }


    // ==================================================
    // 10000 Coins
    // ==================================================

    private void Add10000Coins()
    {
        AddCoins(10000);
    }


    // ==================================================
    // Add Coins
    // ==================================================

    private void AddCoins(int amount)
    {
        PlayerData.AddCoins(amount);

        Debug.Log(
            $"{amount} coins added. " +
            $"Total coins: {PlayerData.Coins}"
        );
    }


    // ==================================================
    // Back
    // ==================================================

    private void BackToMainScene()
    {
        if (string.IsNullOrEmpty(mainSceneName))
        {
            Debug.LogError(
                "Main Scene Name is empty."
            );

            return;
        }


        SceneManager.LoadScene(mainSceneName);
    }
}
}