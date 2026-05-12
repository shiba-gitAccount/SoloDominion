using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleBack : MonoBehaviour
{
    private Button button;
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            SceneManager.LoadScene("TitleScene");
        });
    }
}
