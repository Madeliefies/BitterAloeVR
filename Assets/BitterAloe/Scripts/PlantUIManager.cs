using Cysharp.Threading.Tasks;
using Microsoft.Data.Analysis;
using ProceduralToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PlantUIManager : MonoBehaviour
{
    public LevelData level;
    public GameObject testimonyUIPrefab;
    public GameObject titleUIPrefab;
    public GameObject testimonyUIWindow;
    public Color selectedLineColor = Color.yellow;
    public TextMeshProUGUI pageNumberDisplay;
    public int linesPerPage = 25;
    public Scrollbar scrollbar;

    private int currentPageIndex = 0;

    private GameObject titleUI;
    private GameObject dateUI;
    private GameObject locationUI;
    private GameObject testimonyUI;

    private List<string> dialoguePages = new List<string>();
    private Testimony currentTestimony = new Testimony();
    private int startHighlightIndex;
    public int highlightPageNum;


    public void Start()
    {
        titleUI = Instantiate(titleUIPrefab, testimonyUIWindow.transform);
        dateUI = Instantiate(testimonyUIPrefab, testimonyUIWindow.transform);
        locationUI = Instantiate(testimonyUIPrefab, testimonyUIWindow.transform);
        testimonyUI = Instantiate(testimonyUIPrefab, testimonyUIWindow.transform);
    }


    public void SpawnPlantUI(Vector3 coordinates)
    {
        transform.position = coordinates + Vector3.one.OnlyY();
    }

    public void GetTranscript(Testimony testimony)
    {
        dialoguePages.Clear();
        currentTestimony = testimony;

        Debug.Log($"Current file number: {(int)currentTestimony.file_num}");

        // TODO: Instead of generating list on query, have every per-file testimony list cached
        //level.parq.testimoniesByFileCache.TryGetValue((int)currentTestimony.file_num, out var fileTestimonies);

        var fileTestimonies = level.parq.GetTestimoniesByFile((ushort)currentTestimony.file_num);

        Debug.Log($"Current file length: {fileTestimonies.Count}");
        //var fileTestimonies = level.parq.TestimonySearchByFile(level.parq.testimonies, (int)currentTestimony.file_num);

        // TODO: Restructure this code to make functionality more clear, separate line highlighting from line creation
        string pageText = string.Empty;
        startHighlightIndex = 0;
        for (int lineIndex = 0; lineIndex < fileTestimonies.Count; lineIndex++)
        {
            bool highlight = false;
            if (lineIndex == Convert.ToInt32(currentTestimony.file_index))
                highlight = true;

            if (highlight)
            {
                startHighlightIndex = pageText.Length;
                highlightPageNum = dialoguePages.Count;
                pageText += "<#FFFF00>";
            }
            pageText += $"<u>{fileTestimonies[lineIndex].speaker}:</u><space=1.5em>{fileTestimonies[lineIndex].dialogue}";
            if (highlight)
            {
                pageText += "</color>";
            }
            pageText += "\n";

            if ((lineIndex != 0 && (((lineIndex + 1) % linesPerPage) == 0)) || lineIndex == fileTestimonies.Count - 1)
            {
                dialoguePages.Add(pageText);
                pageText = string.Empty;
            }
            //dialoguePages[dialoguePages.Count-1] += $"<u>{fileTestimonies[line].speaker}:</u><space=1.5em>{fileTestimonies[line].dialogue}\n";
        }
        Debug.Log($"Dialogue pages array length: {dialoguePages.Count}");

        currentPageIndex = 0;
    }

    public void NextPage()
    {
        if (currentPageIndex < dialoguePages.Count - 1)
        {
            //currentPageIndex++;
            DisplayTranscriptPage(currentPageIndex + 1);
            pageNumberDisplay.SetText($"{currentPageIndex + 1} / {dialoguePages.Count}");
        }
    }
    public void PreviousPage()
    {
        if (currentPageIndex > 0)
        {
            //currentPageIndex--;
            DisplayTranscriptPage(currentPageIndex - 1);
            pageNumberDisplay.SetText($"{currentPageIndex + 1} / {dialoguePages.Count}");
        }
    }

    public void DisplayTranscriptPage(int page)
    {
        Debug.Log($"Displaying page {page}");

        currentPageIndex = page;
        pageNumberDisplay.SetText($"{currentPageIndex + 1} / {dialoguePages.Count}");
        //foreach (Transform child in testimonyUIWindow.transform)
        //    Destroy(child.gameObject);

        if (page == 0)
        {
            titleUI.SetActive(true);
            dateUI.SetActive(true);
            locationUI.SetActive(true);
            titleUI.GetComponent<TextMeshProUGUI>().SetText($"<u>TRUTH AND RECONCILIATION COMMISSION\n{currentTestimony.hearing_type}</u>");
            dateUI.GetComponent<TextMeshProUGUI>().SetText("<u>DATE:</u><space=1.5em>" + currentTestimony.date);
            locationUI.GetComponent<TextMeshProUGUI>().SetText("<u>PLACE:</u><space=1.5em>" + currentTestimony.location);
        }
        else
        {
            titleUI.SetActive(false);
            dateUI.SetActive(false);
            locationUI.SetActive(false);
        }

        TextMeshProUGUI testimonyUIComponent = testimonyUI.GetComponent<TextMeshProUGUI>();

        testimonyUIComponent.SetText(dialoguePages[page]);
        testimonyUIComponent.ForceMeshUpdate();

        //float height = 0;
        //if (page == highlightPage)
        //{
        //    await UniTask.WaitForSeconds(0.5f);
        //    // TODO: calculate what determines height offset to be 316
        //    Debug.Log(testimonyUIComponent.text.IndexOf("</color>"));
        //    int lineNum = testimonyUIComponent.textInfo.characterInfo[testimonyUIComponent.text.IndexOf("<#FFFF00>")].lineNumber;
        //    height = testimonyUIComponent.textInfo.characterInfo[testimonyUIComponent.textInfo.lineInfo[lineNum].firstVisibleCharacterIndex].topLeft.y;
        //    Debug.Log(height);
        //}

        scrollbar.value = 1f;
        //testimonyUIWindow.transform.position = new Vector3(testimonyUIWindow.transform.position.x, height - 316, testimonyUIWindow.transform.position.z);
    }
}
