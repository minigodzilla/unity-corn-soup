using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UserInterfaceController : MonoBehaviour
{

    public static UserInterfaceController Instance { get; private set; }

    public UIDocument uiDocument;
    public VisualElement root;

    public GroupBox statCorn;
    public Label statCornLabel;

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 

        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        statCorn = root.Query<GroupBox>("stat-corn").First();
        statCornLabel = statCorn.Query<Label>("label").First();
    }

    // Update is called once per frame
    void Update()
    {
        if (PickerUpper.Instance.IngredientCount(Collectable.IngredientType.freshCorn) <= 0) {
            statCorn.style.display = DisplayStyle.None;
        }
        else {
            statCorn.style.display = DisplayStyle.Flex;
        }

        statCornLabel.text = PickerUpper.Instance.IngredientCount(Collectable.IngredientType.freshCorn).ToString();
    }
}
