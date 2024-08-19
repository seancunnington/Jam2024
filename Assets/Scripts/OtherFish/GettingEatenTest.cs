using UnityEngine;


public class GettingEatenTest : MonoBehaviour
{
    
    
    [SerializeField] int foodAmount = 1;
    
    
    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerController>().EatObject(foodAmount);
            this.gameObject.SetActive(false);
        }
    }
}
