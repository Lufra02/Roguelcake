using TMPro;
using UnityEngine;

public class SurvivalTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    private float elapsedSeconds;

    private void Awake()
    {
        if (timerText == null) timerText = GetComponent<TMP_Text>();
        Debug.Assert(timerText != null, "SurvivalTimer necesita un TMP_Text asignado.", this);
        UpdateText();
    }

    private void Update()
    {
        if (timerText == null || GameManager.Instance == null || GameManager.Instance.gameState != GameState.Play ||
            PlayerManager.Instance?.isDead == true || PlayerManager.Instance?.isInShop == true)
            return;

        elapsedSeconds += Time.deltaTime;
        UpdateText();
    }

    private void UpdateText()
    {
        if (timerText == null) return;

        int totalSeconds = Mathf.FloorToInt(elapsedSeconds);
        timerText.text = $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }
}
