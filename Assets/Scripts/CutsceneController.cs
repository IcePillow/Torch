using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class CutsceneController : MonoBehaviour
{

    public TextMeshProUGUI TextMesh;
    public TextAsset CutsceneText;

    private string[] lines;
    private int lines_length;
    private int i;

    // Start is called before the first frame update
    void Start()
    {
        lines = CutsceneText.text.Split("\n"); 
        lines_length = lines.Length;
        TextMesh.text = lines[0];
        i = 1;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown) {
            if (i < lines_length) {
                TextMesh.text = lines[i];
                i++;
            } else { SceneManager.LoadScene("Tutorial"); }
        }
    }

    }


