using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickerUpper : MonoBehaviour
{
    public float pickupRange = 3;

    private Transform cameraTransform;


    public List<Collectable.IngredientType> ingredientsInInventory = new List<Collectable.IngredientType>();

    private Collectable ingredientInView = null;

    private void Awake()
    {
        cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (ingredientInView && Input.GetMouseButtonDown(0))
        {
            PickUp(ingredientInView);
            ingredientInView = null;
        }
        else
        {
            CheckForCollectables();
        }

    }

    private void PickUp(Collectable ingredientToPickUp)
    {
        ingredientToPickUp.OnCollected();

        ingredientsInInventory.Add(ingredientToPickUp.type);

        UpdateInventory();
    }

    private void CheckForCollectables()
    {
        //TODO: always check, and then highlight objects that you see

        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, pickupRange))
        {
            Collectable hitCollectable = hit.transform.GetComponentInParent<Collectable>();

            if (hitCollectable)
            {
                hitCollectable.OnViewed();

                ingredientInView = hitCollectable;
                return;
            }
        }

        if (ingredientInView)
        {
            ingredientInView.OnLeftView();
            ingredientInView = null;
        }
    }

    private void UpdateInventory()
    {
        //TODO: notify UI and stuff
        //check off a bool if you have all you need of something
        string message = "You now have";
        for (int i = 0; i < ingredientsInInventory.Count; i++)
        {
            message += " a " + ingredientsInInventory[i].ToString() + ",";
        }

        print(message);
    }
}
