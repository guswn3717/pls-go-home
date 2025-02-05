using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour
{
    Text message;

    // Start is called before the first frame update
    void Start()
    {
        message = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowMessageBox(string msg)
    {
        message.text = msg;
        message.enabled = true;
        Invoke("CloseMessageBox", 1.0f);
    }

    void CloseMessageBox()
    {
        message.enabled = false;
    }
}
