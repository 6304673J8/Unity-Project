﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogImporter : MonoBehaviour
{

    public TextAsset textFile;
    public string[] textLines;

    // Start is called before the first frame update
    void Start()
    {
        if(textFile != null)
        {
            textLines = (textFile.text.Split('\n'));

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
