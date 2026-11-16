using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [SerializeField] private GameManager game;

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null &&
            other.attachedRigidbody.TryGetComponent<EnemyMovement>(out _))
            game.Crash();
    }
}
