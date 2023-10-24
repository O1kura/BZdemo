using UnityEngine;

public class ScoreTextSpawner : MonoBehaviour
{
    [SerializeField] ScoreText damageTextPrefab;

    public void Spawn(int score, Vector2 pos)
    {
        ScoreText instance = Instantiate(damageTextPrefab, transform);
        instance.transform.position = pos;
        instance.SetValue(score);
    }
}
