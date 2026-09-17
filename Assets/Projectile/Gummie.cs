using UnityEngine;

public class Gummie : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerManager>();
        if(player != null)
        {
            player.playerHealth.GetHealthBack();
            Destroy(gameObject);
        }
    }
}
