using System.Collections;
using UnityEngine;


public class LevelView : MonoBehaviour
{
    [SerializeField] private LevelController _level;
    [SerializeField] private Camera _camera;

    void OnEnable()
    {
        _level.LifeLost       += OnLifeLost;
        _level.LevelCompleted += OnLevelCompleted;
    }

    void OnDisable()
    {
        if (_level == null) return;
        _level.LifeLost       -= OnLifeLost;
        _level.LevelCompleted -= OnLevelCompleted;
    }

    private void OnLifeLost(int livesLeft)
    {
        AudioManager.Play(Sfx.LoseLife);
        StartCoroutine(Shake(0.3f, 0.16f));
    }

    private void OnLevelCompleted()
    {
        AudioManager.Play(Sfx.LevelComplete);
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 origin = _camera.transform.position;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float fade = 1f - t / duration;
            Vector2 offset = Random.insideUnitCircle * magnitude * fade;
            _camera.transform.position = origin + new Vector3(offset.x, offset.y, 0f);
            yield return null;
        }
        _camera.transform.position = origin;
    }
}
