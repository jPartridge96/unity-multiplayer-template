using TMPro;
using UnityEngine;

public class ClearTextOnEnable : MonoBehaviour
{
    /// <summary>
    /// Clear the text when the object is enabled
    /// </summary>
    private void OnEnable()
    {
        GetComponentInChildren<TMP_InputField>().text = "";
    }
}
