using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;

public class TastyTextSpawner : MonoBehaviour
{
    [SerializeField] private GameObject tastyTextPrefab;
    [SerializeField] private Canvas worldCanvas;

    private readonly CompositeDisposable _disposables = new();

    public void ShowTastyText(Vector3 position) => _ = SpawnTastyTextAsync(position);

    private async UniTaskVoid SpawnTastyTextAsync(Vector3 worldPosition)
    {
        var textObject = Instantiate(tastyTextPrefab, worldPosition, Quaternion.identity, worldCanvas.transform);
        var textMesh = textObject.GetComponent<TextMeshProUGUI>();

        var startTime = Time.time;
        var duration = 1.5f;

        while (Time.time - startTime < duration)
        {
            var progress = (Time.time - startTime) / duration;
            var color = textMesh.color;
            color.a = 1f - progress;
            textMesh.color = color;
            textObject.transform.position += Vector3.up * Time.deltaTime * 2f;
            textObject.transform.rotation = Quaternion.Euler(90, 0, 0);
            await UniTask.NextFrame();
        }

        Destroy(textObject);
    }

    private void OnDestroy() => _disposables.Dispose();
}
