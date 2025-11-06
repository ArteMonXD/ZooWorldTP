using UnityEngine;
using Zenject;

public class ScreenBoundary
{
    private Camera _camera;
    private readonly float _padding = 2f;

    public Vector3 Center => Vector3.zero;

    [Inject]
    public void Construct(Camera mainCamera)
    {
        _camera = mainCamera;
        Debug.Log($"ScreenBoundary: Constructed with camera {_camera != null}");
    }

    public Vector3 GetRandomSpawnPosition()
    {
        if (_camera == null)
        {
            Debug.LogError("ScreenBoundary: Camera is null!");
            return GetFallbackPosition();
        }

        try
        {
            // Генерируем случайную позицию в пределах видимой области
            var viewportX = Random.Range(0.1f, 0.9f);
            var viewportZ = Random.Range(0.1f, 0.9f); // ИСПРАВЛЕНИЕ: равномерное распределение по Z

            var viewportPos = new Vector3(viewportX, 0.5f, viewportZ); // Y=0.5 для правильного преобразования

            // Преобразуем в мировые координаты
            var worldPos = _camera.ViewportToWorldPoint(viewportPos);
            worldPos.y = 0; // Устанавливаем Y=0 для наземных животных

            Debug.Log($"Generated spawn position - Viewport: ({viewportX:F2}, {viewportZ:F2}), World: {worldPos}");

            return worldPos;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error in GetRandomSpawnPosition: {ex.Message}");
            return GetFallbackPosition();
        }
    }

    public bool IsWithinBounds(Vector3 position)
    {
        if (_camera == null)
        {
            Debug.LogWarning("ScreenBoundary: Camera is null in bounds check");
            return IsWithinFallbackBounds(position);
        }

        try
        {
            var viewportPos = _camera.WorldToViewportPoint(position);

            bool isWithin = viewportPos.x >= 0 - _padding &&
                           viewportPos.x <= 1 + _padding &&
                           viewportPos.y >= 0 - _padding && // Используем Y вместо Z для Viewport
                           viewportPos.y <= 1 + _padding;

            if (!isWithin)
            {
                Debug.Log($"Position {position} is out of bounds. Viewport: {viewportPos}");
            }

            return isWithin;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error in IsWithinBounds: {ex.Message}");
            return IsWithinFallbackBounds(position);
        }
    }

    private Vector3 GetFallbackPosition()
    {
        return new Vector3(
            Random.Range(-8f, 8f),
            0,
            Random.Range(-8f, 8f)
        );
    }

    private bool IsWithinFallbackBounds(Vector3 position)
    {
        return position.x >= -10f && position.x <= 10f &&
               position.z >= -10f && position.z <= 10f;
    }
}
