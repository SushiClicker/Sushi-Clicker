using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private GameObject floatingTextParent;
    private GameManager theGameManager;

    //Start function is called just before any of the Update methods is called the first time
    private void Start()
    {
        theGameManager = FindObjectOfType<GameManager>();
    }

    //Function to instantiate floatingText using prefab
    public void PopText()
    {
        if (floatingTextPrefab)
        {
            //Instantiating new GameObject using floatingTextPrefab
            GameObject prefab = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity);

            //Setting floatingTextParent as parent of prefab
            prefab.transform.SetParent(floatingTextParent.transform);

            //Setting the prefab's text with the current amount of sushi per click
            prefab.GetComponentInChildren<Text>().text = "+ " + theGameManager.perClickAdd;
        }
    }
}
