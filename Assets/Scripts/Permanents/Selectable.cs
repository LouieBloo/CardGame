using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{

    [SerializeField] protected GameObject selectedIndicatorPrefab;
    private GameObject selectedIndicator;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void select()
    {
        if (!selectedIndicator)
        {
            selectedIndicator = Instantiate(selectedIndicatorPrefab, new Vector3(transform.position.x, transform.position.y + 3f, transform.position.z), selectedIndicatorPrefab.transform.rotation);
        }
    }

    public void deselect()
    {
        if (selectedIndicator)
        {
            Destroy(selectedIndicator);
            selectedIndicator = null;
        }
    }
    
    //if we are selected and the user left clicks on another permanent
    public virtual void commandIssuedToCell(PermanentCell target,Grid grid)
    {
        Debug.Log("Issueing command at cell: " + target);
    }
}
