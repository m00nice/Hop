using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class CelebrationScreenUI : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text main;
    public TMP_Text secondary;

    public void Set (string main, string secondary)
    {
        gameObject.SetActive(true);
        this.main.text = main;
        this.secondary.text = secondary;
    }
}
