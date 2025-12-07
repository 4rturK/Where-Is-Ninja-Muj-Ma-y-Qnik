using UnityEngine;

public class ControlModeSwitcher : MonoBehaviour
{
    public PlayerMovement3D player;
    public MicrophoneInput mic;
    public KeyCode toggleKey = KeyCode.V;

    private void Start()
    {
        ApplyMode(player != null ? player.Mode : PlayerMovement3D.ControlMode.Normal);
    }

    private void Update()
    {
        if (player == null) return;

        if (Input.GetKeyDown(toggleKey))
        {
            var next = (player.Mode == PlayerMovement3D.ControlMode.Normal)
                ? PlayerMovement3D.ControlMode.Voice
                : PlayerMovement3D.ControlMode.Normal;

            ApplyMode(next);
        }

        // przyk³ad klasycznego inputu tylko w Normal:
        if (player.Mode == PlayerMovement3D.ControlMode.Normal)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            player.inputDir = new Vector3(h, 0f, v).normalized;
        }
        else
        {
            player.inputDir = Vector3.zero;
        }
    }

    public void ApplyMode(PlayerMovement3D.ControlMode mode)
    {
        player.SetControlMode(mode);

        if (mic != null)
            mic.SetListening(mode == PlayerMovement3D.ControlMode.Voice);

        Debug.Log("Tryb sterowania: " + mode);
    }

    // Te metody mo¿esz podpi¹æ pod UI Button/Toggle
    public void SetNormal() => ApplyMode(PlayerMovement3D.ControlMode.Normal);
    public void SetVoice() => ApplyMode(PlayerMovement3D.ControlMode.Voice);
}
