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
    public string[] SpeakerNames;
    public Sprite[] SpeakerPortraits;

    // storage
    private Dictionary<string, TalkTime> talkTimes;
    private TextMeshProUGUI dialogueTextMesh;
    private Image portraitImage;

    // state
    private TalkTime talkTime;
    private int currentPhrase;
    private string textToPrint;
    private float timeSinceChar;
    private bool dialogueIsChoice;


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
                if (!dialogueIsChoice)
                {
                    if (currentPhrase + 1 == talkTime.numPhrases())
                    {
                        EndTalkTime();
                    }
                    else
                    {
                        timeSinceChar = 0;
                        currentPhrase += 1;
                        string speaker;
                        textToPrint = talkTime.getPhrase(currentPhrase, out speaker, out dialogueIsChoice);
                        ChangeSpeaker(speaker);
                        dialogueTextMesh.text = speaker + "\n";
                    }
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    manager.PlayerMadeChoice(talkTime.id, true);
                    EndTalkTime();
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    manager.PlayerMadeChoice(talkTime.id, false);
                    EndTalkTime();
                }
            }
        }
    }


    /* Event Methods */

    public void StartTalkTime(string title, bool downLow)
    {
        manager.freezePhysics();
        talkTime = talkTimes[title];
        currentPhrase = 0;
        timeSinceChar = 0;

        // choose speaker
        string speaker;
        textToPrint = talkTime.getPhrase(0, out speaker, out dialogueIsChoice);
        ChangeSpeaker(speaker);

        // set text
        dialogueTextMesh.text = speaker + "\n";

        /*
        if (downLow)
        {
            foreach (Transform child in canvas.transform)
            {
                RectTransform rect = child.GetComponent<RectTransform>();
                rect.position = new Vector3(
                    rect.position.x,
                    -3.75f,
                    rect.position.z
                    );
            }
        }
        else
        {
            foreach (Transform child in canvas.transform)
            {
                RectTransform rect = child.GetComponent<RectTransform>();
                rect.position = new Vector3(
                    rect.position.x,
                    3.75f,
                    rect.position.z
                    );
            }
        }
        */

        canvas.transform.position = new Vector3(0, 5, 0);
        canvas.enabled = true;
    }

    public void EndTalkTime()
    {
        talkTime = null;
        dialogueTextMesh.text = "";
        canvas.enabled = false;
        manager.unfreezePhysics();
    }

    public void ChangeSpeaker(string speaker)
    {
        for (int i = 0; i < SpeakerNames.Length; i++)
        {
            if (speaker == SpeakerNames[i])
            {
                portraitImage.sprite = SpeakerPortraits[i];
                break;
            }
        }
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
                talk.addPhrase(sections[0], sections[1], sections[2]);
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
        private List<string> metas;
        private List<string> speakers;
        private List<string> phrases;

        public TalkTime(string id)
        {
            this.id = id;

            metas = new List<string>();
            speakers = new List<string>();
            phrases = new List<string>();
        }

        public void addPhrase(string meta, string speaker, string phrase)
        {
            metas.Add(meta);
            speakers.Add(speaker);
            phrases.Add(phrase);
        }
        public string getPhrase (int index, out string speaker, out bool isChoice)
        {
            isChoice = metas[index] == "CHOICE";
            speaker = speakers[index];
            return phrases[index];
        }

        public int numPhrases()
        {
            return phrases.Count;
        }
    }

}
