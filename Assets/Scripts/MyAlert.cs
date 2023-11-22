using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PixelCrushers.DialogueSystem;

public class MyAlert : MonoBehaviour
{
    public Text tipText;
    public Image tipImage;
    public bool canAnyKeyExit;
    public bool useImage;

    void Start()
    {
        canAnyKeyExit = false;
        useImage = false;
        SystemFacade.instance.RegisterMyAlert(this);
        this.gameObject.SetActive(false);
        //DontDestroyOnLoad(this.gameObject);
        //tipText = GetComponentInChildren<Text>();
    }
    void OnEnable()
    {
        if (SystemFacade.instance != null)
            SystemFacade.instance.RegisterMyAlert(this);
    }
    void Update()
    {
        if (useImage)
        {
            tipImage.gameObject.SetActive(true);
        }
        else
        {
            tipImage.gameObject.SetActive(false);
        }
    }

    public void ShowAlert(string s)
    {
        useImage = false;
        RefreshText(s);
        gameObject.SetActive(true);
        StartCoroutine(Wait1(s, 2.0f));


    }
    public void ShowAlert(string s, float t)
    {
        useImage = false;
        RefreshText(s);
        gameObject.SetActive(true);
        StartCoroutine(Wait1(s, t));
        StartCoroutine(Wait1SAnyKeyExit());
    }
    public void ShowAlert(string s, float t, Sprite sp)
    {
        useImage = true;
        RefreshImage(sp);
        RefreshText(s);
        gameObject.SetActive(true);
        StartCoroutine(Wait1(s, t));
        StartCoroutine(Wait1SAnyKeyExit());
    }
    public IEnumerator Wait1(string s, float t)
    {
        yield return new WaitForSeconds(t);
        useImage = false;

        Close();
    }
    public IEnumerator Wait1SAnyKeyExit()
    {
        canAnyKeyExit = false;
        yield return new WaitForSeconds(0.7f);
        canAnyKeyExit = true;

    }


    public void Close()
    {
        gameObject.SetActive(false);
        tipImage.sprite = null;
        tipImage.gameObject.SetActive(false);
    }
    private void RefreshText(string s)
    {
        if (tipText != null)
            tipText.text = s;


    }
    private void RefreshImage(Sprite sp)
    {
        if (tipImage != null)
            tipImage.sprite = sp;


    }
}
