using TMPro;
using UnityEngine;

public class ScoreText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI damageText = null;
    public void DestroyText()
    {
        Destroy(gameObject);
    }
    public void SetValue(int score)
    {
        damageText.text = "+" + score.ToString();
    }
}
