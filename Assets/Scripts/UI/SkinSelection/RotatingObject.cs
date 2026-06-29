using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    public Vector3 m_Axis = Vector3.up;
    public float m_DegreesPerSecond = 45f;
    public bool m_UnscaledTime = true;

    private void Update()
    {
        float dt = m_UnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        transform.Rotate(m_Axis, m_DegreesPerSecond * dt, Space.Self);
    }
}
