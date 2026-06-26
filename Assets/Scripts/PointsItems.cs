using UnityEngine;

public class PointsItem : MonoBehaviour
{
    [SerializeField]
    private int points = 100;

    private void Start()
    {
        Rigidbody2D rb =
            GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            float saltoX =
                Random.Range(-2f, 2f);

            float saltoY =
                Random.Range(3f, 6f);

            rb.AddForce(
                new Vector2(
                    saltoX,
                    saltoY
                ),
                ForceMode2D.Impulse
            );
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collect(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Collect(collision.gameObject);
    }

    private void Collect(GameObject touchedObject)
    {
        if (!touchedObject.CompareTag("Player"))
        {
            return;
        }

        var session =
            ServiceLocator
            .Get<SessionController>();

        if (session == null)
        {
            return;
        }

        session.AddPoints(points);

        Destroy(gameObject);
    }
}