using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{

    public enum IngredientType { strawberry, freshBeans, freshCorn, driedBeans, driedCorn, venison, recipeBook, ash };
    
    public IngredientType type;
    //public Dictionary<string,string> nameDictionary;

    public Material highlightMaterial;
    private Material defaultMaterial;

    private MeshRenderer mr;

    private void Awake()
    {
        mr = GetComponentInChildren<MeshRenderer>();
        defaultMaterial = mr.material;
    }

    public void OnViewed()
    {
        mr.material = highlightMaterial;
    }

    public void OnLeftView()
    {
        mr.material = defaultMaterial;
    }

    public void OnCollected()
    {
        Destroy(this.gameObject);//for now just delete it so it looks like it picked up
    }
}
