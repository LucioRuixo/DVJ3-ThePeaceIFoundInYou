﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using nullbloq.Noodles;

[Serializable]
public class DialogueController : NodeController
{
    public override Type NodeType { protected set; get; }

    bool typing = false;

    string sentence;

    int currentDialogueStripIndex = 0;

    public float letterDisplayWaitTime;
    public float pauseWaitTime;
    public float whisperFontSizeFactor = 0.5f;
    float fontSize;

    IEnumerator typingCoroutine;

    public GameObject dialogue;
    public Image dialogueBox;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    CharacterController characterManager;
    NoodlesNodeMultipleDialogue node;

    Queue<string> sentenceQueue = new Queue<string>();

    void Awake()
    {
        NodeType = typeof(NoodlesNodeMultipleDialogue);

        characterManager = transform.parent.GetComponent<CharacterController>();
        fontSize = dialogueText.fontSize;
    }

    void OnEnable()
    {
        //NodeManager.OnDialogue += Begin;
    }

    void Update()
    {
        if (!dialogue.activeInHierarchy) return;

        if (Input.GetButtonDown("Continue"))
        {
            if (typing)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = sentence;

                typing = false;
            }
            else ExecuteNextDialogueStrip();
        }
    }

    void OnDisable()
    {
        //NodeManager.OnDialogue -= Begin;
    }

    void Begin(NoodlesNodeMultipleDialogue _node)
    {
        node = _node;
        dialogue.SetActive(true);

        ExecuteNextDialogueStrip();
    }

    public void ExecuteNextDialogueStrip() // Cuando cambie sentence de DialogueStrip por una lista de oraciones volver a hacerla privada y hacer que Continue llame a DisplayNextSentence
    {
        if (node.dialogueStrips.Count > currentDialogueStripIndex)
        {
            CharacterController.Character key = node.dialogueStrips[currentDialogueStripIndex].character;
            if (characterManager.characterDictionary.TryGetValue(key, out CharacterSO character))
            {
                dialogueBox.sprite = character.dialogueBoxSprite;

                if (node.dialogueStrips[currentDialogueStripIndex].status == CharacterController.Status.Known)
                    nameText.text = character.characterName;
                else
                    nameText.text = "???";
            }

            //DisplayNextSentence();
            if (typing) StopCoroutine(typingCoroutine);

            sentence = node.dialogueStrips[currentDialogueStripIndex].sentence;
            typingCoroutine = TypeSentence(CheckForWhisper());
            StartCoroutine(typingCoroutine);
            typing = true;

            currentDialogueStripIndex++;
        }
        else End();
    }

    bool CheckForWhisper()
    {
        return sentence.ToCharArray()[0] == '[';
    }

    void End()
    {
        currentDialogueStripIndex = 0;
        dialogue.SetActive(false);

        CallNodeExecutionCompletion(0); // Adaptar en NodeManager para que funcione al conectar el puerto con varios nodos en vez de uno solo
    }

    //public void DisplayNextSentence()
    //{
    //    if (sentenceQueue.Count == 0)
    //    {
    //        End();
    //        return;
    //    }
    //
    //    StartCoroutine(TypeSentence(sentenceQueue.Dequeue()));
    //}

    public override void Execute(NoodlesNode genericNode)
    {
        var node = genericNode as NoodlesNodeMultipleDialogue;

        Begin(node);
    }

    IEnumerator TypeSentence(bool whispering)
    {
        dialogueText.fontSize = fontSize;
        if (whispering) dialogueText.fontSize *= whisperFontSizeFactor;

        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;

            if (letter == '.' || letter == ',' || letter == '-')
                yield return new WaitForSeconds(pauseWaitTime);

            yield return new WaitForSeconds(letterDisplayWaitTime);
        }

        typing = false;
    }
}