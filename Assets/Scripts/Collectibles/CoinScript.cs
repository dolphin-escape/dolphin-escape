using UnityEngine;

public class CoinScript : MonoBehaviour
{
    [SerializeField] private int value;
    private bool hasTriggered;
    private CoinManager coinManager;
    public ParticleSystem particles;
    public Animator animCoin;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        coinManager = CoinManager.instance;
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            coinManager.ChangeCoins(value);

            particles.Play();

            animCoin.SetTrigger("Collect");
            Destroy(gameObject, 1f);
        }
    }
}
