using System.Collections;
using UnityEngine;

public class Dead_Prefab : MonoBehaviour
{
    private void DestroyDeadPrefab(){
        StartCoroutine(Destroy_Prefab());
    }

    private IEnumerator Destroy_Prefab(){
        yield return new WaitForSeconds(1);
        Debug.Log("Destroy object");
        Destroy(this.gameObject);
    }
   
}
