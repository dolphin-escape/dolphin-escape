using UnityEngine;

public class BoxManager : MonoBehaviour
{
    public CoinManager coinManager;
    public GameObject[] boxes;
    public int coinsPerBox = 5;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int activeBoxes = coinManager.CurrentCoins / coinsPerBox;

        for (int i = 0; i < boxes.Length; i++)
        {
            boxes[i].SetActive(i < activeBoxes);
        }
    }
}
