using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class Narrator : MonoBehaviour
{
    // reference parameters
    public Manager manager;
    public Canvas canvas;

    // data parameters
    public TextAsset Dialogue;
    public float CharsPerSecond = 12;

    // storage
    private Dictionary<string, TalkTime> talkTimes;
    private TextMeshProUGUI dialogueTextMesh;
    private Image portraitImage;

    // state
    private TalkTime talkTime;
    private int currentPhrase;
    private string textToPrint;
    private float timeSinceChar;


    /* Action Methods */

    void Awake()
    {
        dialogueTextMesh = GameObject.Find("Dialogue-Text").GetComponent<TextMeshProUGUI>();
        portraitImage = GameObject.Find("Dialogue-Portrait").GetComponent<Image>();

        initializeTalkTimes();

        talkTime = null;
        currentPhrase = 0;
        textToPrint = "";
        canvas.enabled = false;

        manager.narrator = this;
    }

    void Update()
    {
        timeSinceChar += Time.deltaTime;

        if (talkTime != null)
        {
            // print out a character
            while (timeSinceChar > 1f / CharsPerSecond && textToPrint.Length > 0)
            {
                dialogueTextMesh.text = dialogueTextMesh.text + textToPrint[0];
                textToPrint = textToPrint.Substring(1);
                timeSinceChar -= 1f / CharsPerSecond;
            }

            // move to next phrase or be done
            if (keyInputDown() && textToPrint.Length == 0)
            {
                if (currentPhrase + 1 == talkTime.numPhrases())
                {
                    EndTalkTime();
                }
                else
                {
                    dialogueTextMesh.text = "";
                    timeSinceChar = 0;
                    currentPhrase += 1;
                    string speaker;
                    textToPrint = talkTime.getPhrase(currentPhrase, out speaker);
                    ChangeSpeaker(speaker);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            StartTalkTime("INTRO_1");
        }
    }


    /* Event Methods */

    public void StartTalkTime(string title)
    {
        manager.freezePhysics();
        talkTime = talkTimes[title];
        currentPhrase = 0;
        timeSinceChar = 0;

        string speaker;
        textToPrint = talkTime.getPhrase(0, out speaker);
        ChangeSpeaker(speaker);

        canvas.enabled = true;
    }

    public void EndTalkTime()
    {
        talkTime = null;
        canvas.enabled = false;
        manager.unfreezePhysics();
    }

    public void ChangeSpeaker(string speaker)
    {

    }


    /* Utility Methods */

    private void initializeTalkTimes()
    {
        // initialize dictionary
        talkTimes = new Dictionary<string, TalkTime>();

        // state
        bool nextIsTitle = true;
        TalkTime talk = null;

        // loop through lines
        string[] lines = Dialogue.text.Split("\n");
        foreach (string line in lines)
        {
            string[] sections = line.Split("\t");

            if (nextIsTitle)
            {
                talk = new TalkTime(sections[0]);
                talkTimes.Add(sections[0], talk);
                nextIsTitle = false;
            }
            else if (sections[0] == "")
            {
                nextIsTitle = true;
                talk = null;
            }
            else
            {
                talk.addPhrase(sections[1], sections[2]);
            }
        }
    }

    private bool keyInputDown()
    {
        return Input.GetKeyDown(KeyCode.W)
            || Input.GetKeyDown(KeyCode.A)
            || Input.GetKeyDown(KeyCode.S)
            || Input.GetKeyDown(KeyCode.D);
    }


    /* Utility Classes */

    private class TalkTime
    {
        public string id;
        private List<string> speakers;
        private List<string> phrases;

        public TalkTime(string id)
        {
            this.id = id;

            speakers = new List<string>();
            phrases = new List<string>();
        }

        public void addPhrase(string speaker, string phrase)
        {
            speakers.Add(speaker);
            phrases.Add(phrase);
        }
        public string getPhrase (int index, out string speaker)
        {
            speaker = speakers[index];
            return phrases[index];
        }

        public int numPhrases()
        {
            return phrases.Count;
        }
    }

}
