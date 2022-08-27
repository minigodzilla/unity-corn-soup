using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UserInterfaceController : MonoBehaviour
{

    public static UserInterfaceController Instance { get; private set; }

    public Dictionary<Collectable.IngredientType,Dictionary<string,string>> ingredientDict;

    public UIDocument uiDocument;
    public VisualElement root;

    public VisualElement hudScreen;
    public GroupBox statCorn;
    public Label statCornLabel;

    public VisualElement pickScreen;
    public Label nativeName;
    public Label englishName;

    public VisualElement dialogueScreen;
    public Label dialogueText;
    public Queue<string> dialogueQueue;

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

        // Populate the ingredient dictionary
        ingredientDict = new();

        Dictionary<string,string> freshCornNames = new();
        freshCornNames["en"] = "Fresh Corn";
        freshCornNames["mo"] = "Ohnawénha";
        ingredientDict.Add(Collectable.IngredientType.freshCorn, freshCornNames);

        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        hudScreen = root.Query<VisualElement>("hud-screen").First();
        statCorn = hudScreen.Query<GroupBox>("stat-corn").First();
        statCornLabel = statCorn.Query<Label>("label").First();

        pickScreen = root.Query<VisualElement>("pick-screen").First();
        nativeName = pickScreen.Query<Label>("native-name").First();
        englishName = pickScreen.Query<Label>("english-name").First();

        dialogueScreen = root.Query<VisualElement>("dialogue-screen").First();
        dialogueText = dialogueScreen.Query<Label>("dialogue-text").First();

        dialogueQueue = new();
        dialogueQueue.Enqueue("Hello Steve");
        dialogueQueue.Enqueue("Hello Liam");
    }

    // Update is called once per frame
    void Update()
    {
        // HUD Handling
        if (PickerUpper.Instance.IngredientCount(Collectable.IngredientType.freshCorn) <= 0) {
            statCorn.style.display = DisplayStyle.None;
        }
        else {
            statCorn.style.display = DisplayStyle.Flex;
        }

        statCornLabel.text = PickerUpper.Instance.IngredientCount(Collectable.IngredientType.freshCorn).ToString();

        // PickScreen Handling
        if (PickerUpper.Instance.ingredientInView == null) {
            pickScreen.style.display = DisplayStyle.None;
        }
        else {
           pickScreen.style.display = DisplayStyle.Flex;
           nativeName.text = ingredientDict[PickerUpper.Instance.ingredientInView.type]["mo"];
           englishName.text = ingredientDict[PickerUpper.Instance.ingredientInView.type]["en"];
        }

        if (dialogueQueue.Count > 0) {
            pickScreen.style.display = DisplayStyle.None;
            dialogueScreen.style.display = DisplayStyle.Flex;
            dialogueText.text = dialogueQueue.Peek();

            if (Input.GetMouseButtonDown(0)) {
                dialogueQueue.Dequeue();
            }
        }
        else {
            dialogueScreen.style.display = DisplayStyle.None;
        }
    }
}
