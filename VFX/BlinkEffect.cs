using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class BlinkEffect : NetworkBehaviour
{
    private Material originalMaterial;
    private Color originalColor;
    private Renderer renderer;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        originalMaterial = renderer.material;
        originalColor = originalMaterial.color;
    }

    // This should be called on the server side
    public void Blink()
    {
        BlinkClientRpc();
    }

    [ClientRpc]
    private void BlinkClientRpc()
    {
        // Start the blink coroutine on every client
        StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        // Change color to red (or any other color to indicate damage)
        originalMaterial.color = Color.red;

        // Wait for the desired amount of time
        yield return new WaitForSeconds(0.1f);

        // Change back to the original color
        originalMaterial.color = originalColor;
    }
}
